using System.Security.Claims;
using Domain;
using Domain.Album;
using Domain.Album.Image;
using Domain.Api;
using Domain.Category;
using Domain.User;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Query.Albums.Queries;
using Query.Categories.Queries;
using Query.Database;
using Query.Images.Queries;
using Query.Users.Queries;
using S3Storage;
using Silo.Services;

using AlbumGrain = Domain.Album.IAlbumGrain;
using CategoryGrain = Domain.Category.ICategoryGrain;
using UserGrain = Domain.User.IUserGrain;
using AlbumId = Domain.Album.AlbumId;
using AlbumTitle = Domain.Album.AlbumTitle;
using AlbumDescription = Domain.Album.AlbumDescription;
using AlbumTags = Domain.Album.AlbumTags;
using DomainImageId = Domain.Album.Image.ImageId;
using DomainImageTitle = Domain.Album.Image.ImageTitle;
using DomainImageTags = Domain.Album.Image.ImageTags;
using DomainCategoryId = Domain.Category.CategoryId;
using CategoryName = Domain.Category.CategoryName;
using CategoryDescription = Domain.Category.CategoryDescription;
using DomainUserId = Domain.User.UserId;
using Username = Domain.User.Username;
using Nickname = Domain.User.Nickname;
using Biography = Domain.User.Biography;
using DomainImageFile = Domain.ImageFile;

namespace Silo.Hubs;

public class MainHub : Hub
{
    private readonly IGrainFactory _grains;
    private readonly IMediator _mediator;
    private readonly JwtTokenService _jwt;
    private readonly IDbContextFactory<QueryDbContext> _queryDb;

    public MainHub(
        IGrainFactory grains,
        IMediator mediator,
        JwtTokenService jwt,
        IDbContextFactory<QueryDbContext> queryDb)
    {
        _grains = grains;
        _mediator = mediator;
        _jwt = jwt;
        _queryDb = queryDb;
    }

    // ─── Account ────────────────────────────────────────────────

    public async Task<JwtTokenResponse> Login(LoginRequest request)
    {
        var existence = await _mediator.Send(
            new UsernameExistenceQuery(new Username(request.Username)));
        if (!existence.IsExist)
            throw new HubException("User not found");

        // Dev mode: no password check, just issue token
        // Find the user ID from the query database
        await using var db = await _queryDb.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
            throw new HubException("User not found");

        var token = _jwt.Generate(new UserId(user.Id), new Username(user.Username), Role.User);
        return new JwtTokenResponse(token.AccessToken, token.RefreshToken, token.ExpireIn);
    }

    public async Task<JwtTokenResponse> Register(RegisterRequest request)
    {
        SetActor(null);
        var grain = _grains.GetGrain<IUserGrain>(0);
        var userId = await grain.Register(
            new Username(request.Username),
            new Nickname(request.Nickname),
            new Biography(request.Biography));

        var token = _jwt.Generate(userId, new Username(request.Username), Role.User);
        return new JwtTokenResponse(token.AccessToken, token.RefreshToken, token.ExpireIn);
    }

    public JwtTokenResponse RefreshToken(string refreshToken)
    {
        var (userId, isValid) = _jwt.DecodeRefreshToken(refreshToken);
        if (!isValid)
            throw new HubException("Invalid or expired refresh token");

        var username = Context.User?.FindFirst("username")?.Value ?? "unknown";
        var role = Context.User?.FindFirst("role")?.Value;
        var userRole = role == ((byte)Role.Admin).ToString() ? Role.Admin : Role.User;

        var token = _jwt.Generate(userId, new Username(username), userRole);
        return new JwtTokenResponse(token.AccessToken, token.RefreshToken, token.ExpireIn);
    }

    // ─── Albums ─────────────────────────────────────────────────

    [Authorize]
    public async Task<AlbumResponse[]> GetAlbums(long? categoryId, long? cursor)
    {
        var actor = GetActor();
        var albums = await _mediator.Send(new AlbumsQuery(categoryId, null, null, cursor, actor));
        return await ToAlbumResponses(albums);
    }

    [Authorize]
    public async Task<AlbumResponse?> GetAlbum(long id)
    {
        var actor = GetActor();
        var albums = await _mediator.Send(new AlbumsQuery(null, null, null, null, actor));
        var album = albums.FirstOrDefault(a => a.Id == id);
        if (album is null) return null;

        var responses = await ToAlbumResponses([album]);
        return responses[0];
    }

    [Authorize]
    public async Task<AlbumResponse> CreateAlbum(CreateAlbumRequest request)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(0);
        var albumId = await grain.Create(
            new AlbumTitle(request.Title),
            new AlbumDescription(request.Description),
            new AlbumTags(request.Tags),
            new CategoryId(request.CategoryId));

        return new AlbumResponse(
            albumId.Value, request.Title, request.Description,
            actor.Id.Value, "unknown", request.CategoryId, "unknown",
            request.Tags, 0, AccessLevel.PublicReadWrite,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    [Authorize]
    public async Task UpdateAlbum(long id, UpdateAlbumRequest request)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(id);
        await grain.Update(
            request.Title is not null ? new AlbumTitle(request.Title) : null,
            request.Description is not null ? new AlbumDescription(request.Description) : null,
            request.Tags is not null ? new AlbumTags(request.Tags) : null,
            request.CategoryId is not null ? new CategoryId(request.CategoryId.Value) : null);
    }

    [Authorize]
    public async Task RemoveAlbum(long id)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(id);
        await grain.Remove();
    }

    [Authorize]
    public async Task SubscribeAlbum(long id)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(id);
        await grain.Subscribe();
    }

    [Authorize]
    public async Task UnsubscribeAlbum(long id)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(id);
        await grain.Unsubscribe();
    }

    // ─── Images ──────────────────────────────────────────────────

    [Authorize]
    public async Task<ImageResponse> AddImage(long albumId, AddImageRequest request)
    {
        var actor = GetActor();
        SetActor(actor);

        var imageId = ImageId.GenerateNew();

        // Save to persistent image storage
        var imagesDir = Path.Combine(AppContext.BaseDirectory, "images");
        Directory.CreateDirectory(imagesDir);
        var persistPath = Path.Combine(imagesDir, $"{imageId.Value}.img");
        await File.WriteAllBytesAsync(persistPath, request.FileBytes);

        // Also save to temp for grain processing
        var tmpDir = Path.Combine(Path.GetTempPath(), "sastimg", "images");
        Directory.CreateDirectory(tmpDir);
        var filePath = Path.Combine(tmpDir, $"{Guid.NewGuid()}.img");
        await File.WriteAllBytesAsync(filePath, request.FileBytes);

        try
        {
            var grain = _grains.GetGrain<IAlbumGrain>(albumId);
            await grain.AddImage(
                imageId,
                new ImageTitle(request.Title),
                new ImageTags(request.Tags),
                new ImageFile(filePath));

            return new ImageResponse(
                imageId.Value, albumId, request.Title, actor.Id.Value, "unknown",
                request.Tags, 0, false,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                $"/images/{imageId.Value}");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Authorize]
    public async Task RemoveImage(long albumId, long imageId)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(albumId);
        await grain.RemoveImage(new ImageId(imageId));
    }

    [Authorize]
    public async Task LikeImage(long albumId, long imageId)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(albumId);
        await grain.LikeImage(new ImageId(imageId));
    }

    [Authorize]
    public async Task UnlikeImage(long albumId, long imageId)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IAlbumGrain>(albumId);
        await grain.UnLikeImage(new ImageId(imageId));
    }

    [Authorize]
    public async Task<ImageResponse[]> GetImages(long albumId, long? cursor)
    {
        var actor = GetActor();
        var images = await _mediator.Send(new ImagesQuery(null, albumId, cursor, actor));
        return images.Select(i => new ImageResponse(
            i.Id, i.AlbumId, i.Title, i.UploaderId, "unknown",
            i.Tags, i.Likes, i.Requester.Liked,
            new DateTimeOffset(i.UploadedAt).ToUnixTimeSeconds(),
            $"/images/{i.Id}")).ToArray();
    }

    // ─── Categories ─────────────────────────────────────────────

    public async Task<CategoryDto[]> GetCategories()
    {
        return await _mediator.Send(new CategoriesQuery());
    }

    [Authorize]
    public async Task<CategoryDto> CreateCategory(CreateCategoryRequest request)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<ICategoryGrain>(0);
        var id = await grain.Create(
            new CategoryName(request.Name),
            new CategoryDescription(request.Description));
        return new CategoryDto(id.Value, request.Name, request.Description);
    }

    [Authorize]
    public async Task UpdateCategory(long id, UpdateCategoryRequest request)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<ICategoryGrain>(id);
        await grain.Update(
            request.Name is not null ? new CategoryName(request.Name) : null,
            request.Description is not null ? new CategoryDescription(request.Description) : null);
    }

    [Authorize]
    public async Task DeleteCategory(long id)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<ICategoryGrain>(id);
        await grain.Delete();
    }

    // ─── User / Profile ─────────────────────────────────────────

    [Authorize]
    public async Task<UserProfileResponse?> GetProfile(long? userId)
    {
        var actor = GetActor();
        var id = userId.HasValue ? userId.Value : actor.Id.Value;
        var dto = await _mediator.Send(new UserProfileQuery(id));

        if (dto is null) return null;

        await using var db = await _queryDb.CreateDbContextAsync();
        var registeredAt = await db.Users
            .Where(u => u.Id == dto.Id)
            .Select(u => u.RegisteredAt)
            .FirstOrDefaultAsync();

        return new UserProfileResponse(
            dto.Id, dto.Username, dto.Nickname, dto.Biography,
            new DateTimeOffset(registeredAt).ToUnixTimeSeconds());
    }

    [Authorize]
    public async Task UpdateProfile(UpdateProfileRequest request)
    {
        var actor = GetActor();
        SetActor(actor);
        var grain = _grains.GetGrain<IUserGrain>(actor.Id.Value);
        await grain.UpdateProfile(
            request.Username is not null ? new Username(request.Username) : null,
            request.Nickname is not null ? new Nickname(request.Nickname) : null,
            request.Biography is not null ? new Biography(request.Biography) : null);
    }

    [Authorize]
    public async Task UpdateAvatar(byte[] fileBytes)
    {
        var actor = GetActor();
        var tmpDir = Path.Combine(Path.GetTempPath(), "sastimg", "avatars");
        Directory.CreateDirectory(tmpDir);
        var filePath = Path.Combine(tmpDir, $"{Guid.NewGuid()}.img");
        await File.WriteAllBytesAsync(filePath, fileBytes);

        try
        {
            SetActor(actor);
            var grain = _grains.GetGrain<IUserGrain>(actor.Id.Value);
            await grain.UpdateAvatar(new ImageFile(filePath));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Authorize]
    public async Task UpdateHeader(byte[] fileBytes)
    {
        var actor = GetActor();
        var tmpDir = Path.Combine(Path.GetTempPath(), "sastimg", "headers");
        Directory.CreateDirectory(tmpDir);
        var filePath = Path.Combine(tmpDir, $"{Guid.NewGuid()}.img");
        await File.WriteAllBytesAsync(filePath, fileBytes);

        try
        {
            SetActor(actor);
            var grain = _grains.GetGrain<IUserGrain>(actor.Id.Value);
            await grain.UpdateHeader(new ImageFile(filePath));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Actor GetActor()
    {
        if (Context.User?.Identity?.IsAuthenticated != true)
            throw new HubException("Not authenticated");

        var userIdClaim = Context.User.FindFirst("id")?.Value;
        var usernameClaim = Context.User.FindFirst("username")?.Value;
        var roleClaim = Context.User.FindFirst("role")?.Value;

        if (userIdClaim is null || !long.TryParse(userIdClaim, out var uid))
            throw new HubException("Invalid token: missing user id");

        return new Actor
        {
            Id = new UserId(uid),
            IsAuthenticated = true,
            Role = roleClaim == ((byte)Role.Admin).ToString() ? Role.Admin : Role.User,
        };
    }

    private static void SetActor(Actor? actor)
    {
        RequestContext.Set("Actor", actor ?? new Actor { Id = new UserId(0), IsAuthenticated = false, Role = Role.None });
    }

    private async Task<AlbumResponse[]> ToAlbumResponses(AlbumDto[] albums)
    {
        if (albums.Length == 0) return [];

        await using var db = await _queryDb.CreateDbContextAsync();
        var userIds = albums.Select(a => a.Author).Distinct().ToArray();
        var categoryIds = albums.Select(a => a.Category).Distinct().ToArray();

        var users = await db.Users.Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);
        var categories = await db.Categories.Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        return albums.Select(a => new AlbumResponse(
            a.Id, a.Title, a.Description,
            a.Author, users.GetValueOrDefault(a.Author, "unknown"),
            a.Category, categories.GetValueOrDefault(a.Category, "unknown"),
            a.Tags, a.SubscribeCount, AccessLevel.PublicReadWrite,
            new DateTimeOffset(a.CreatedAt).ToUnixTimeSeconds(),
            new DateTimeOffset(a.UpdatedAt).ToUnixTimeSeconds()
        )).ToArray();
    }
}
