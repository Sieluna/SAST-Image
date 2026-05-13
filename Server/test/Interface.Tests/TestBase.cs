using Domain;
using Domain.Album;
using Domain.Album.Events;
using Domain.Album.Image;
using Domain.Category;
using Domain.Category.Events;
using Domain.File;
using Domain.User;
using Domain.User.Events;
using Interface.Services;
using Mediator;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Concurrency;
using Orleans.Runtime;
using Query.Albums.EventHandlers;
using Query.Albums.Queries;
using Query.Categories.EventHandlers;
using Query.Categories.Queries;
using Query.Database;
using Query.Images;
using Query.Images.Queries;
using Query.Users.EventHandlers;
using Query.Users.Queries;
using Storage.Database;
using Storage.Images;
using Storage.Images.Queries;
using Storage.Services;

namespace Interface.Tests;

public abstract class TestBase
{
    protected WebApplicationFactory<Program> Factory { get; set; } = null!;
    protected HttpClient Client { get; set; } = null!;
    protected Mock<IGrainFactory> GrainFactoryMock { get; } = new();
    protected Mock<IAlbumGrain> AlbumGrainMock { get; } = new();
    protected Mock<IUserGrain> UserGrainMock { get; } = new();
    protected Mock<ICategoryGrain> CategoryGrainMock { get; } = new();
    protected Mock<IFileManagerGrain> FileManagerGrainMock { get; } = new();
    protected Mock<IMediator> MediatorMock { get; } = new();
    protected Mock<IImageFileManager> ImageFileManagerMock { get; } = new();
    protected JwtTokenService JwtService { get; set; } = null!;

    private readonly string _dbName = Guid.NewGuid().ToString();

    [TestInitialize]
    public virtual async Task SetUp()
    {
        ResetMocksAndSetupDefaults();

        var authOpts = Options.Create(new AuthOptions
        {
            SecKey = "I'm a super secure secret key for JWT HS256!",
            Algorithm = "HS256",
            Expires = 3600,
        });
        JwtService = new JwtTokenService(authOpts);

        var dbName = _dbName; // Capture for lambdas
        var testConfig = new Dictionary<string, string?>
        {
            ["Auth:SecKey"] = "I'm a super secure secret key for JWT HS256!",
            ["Auth:Algorithm"] = "HS256",
            ["Auth:Expires"] = "3600",
            ["ConnectionStrings:Domain"] = "Server=localhost;Database=test;User Id=test;Password=test",
            ["ConnectionStrings:Query"] = "Server=localhost;Database=test;User Id=test;Password=test",
            ["ConnectionStrings:Storage"] = "Server=localhost;Database=test;User Id=test;Password=test",
            ["S3Storage:ServiceUrl"] = "http://127.0.0.1:9000",
            ["S3Storage:BucketName"] = "test",
            ["Storage:BaseUri"] = "file:///tmp/test-images",
        };

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(testConfig);
                });

                builder.ConfigureServices(services =>
                {
                    // Remove hosted services
                    var hosted = services
                        .Where(d => typeof(IHostedService).IsAssignableFrom(d.ServiceType))
                        .Select(d => d.ServiceType)
                        .ToList();
                    foreach (var t in hosted)
                    {
                        var descriptors = services.Where(d => d.ServiceType == t).ToList();
                        foreach (var d in descriptors)
                            services.Remove(d);
                    }

                    // Remove IClusterClient
                    var clusterDescriptors = services
                        .Where(d => d.ServiceType == typeof(IClusterClient)).ToList();
                    foreach (var d in clusterDescriptors)
                        services.Remove(d);

                    // Replace with mocks
                    services.AddSingleton(GrainFactoryMock.Object);
                    services.AddSingleton(MediatorMock.Object);
                    services.AddSingleton(ImageFileManagerMock.Object);

                    // Remove all EF Core registrations (pool, factory, options) for Query and Storage DB
                    var toRemove = services
                        .Where(d => d.ServiceType.FullName?.Contains("QueryDbContext") == true
                                 || d.ServiceType.FullName?.Contains("StorageDbContext") == true
                                 || d.ServiceType.FullName?.Contains("DbContextPool") == true
                                 || d.ServiceType.FullName?.Contains("ScopedDbContextLease") == true)
                        .ToList();
                    foreach (var d in toRemove)
                        services.Remove(d);

                    // Register InMemory options and factory
                    var queryOptions = new DbContextOptionsBuilder<QueryDbContext>()
                        .UseInMemoryDatabase(dbName)
                        .EnableSensitiveDataLogging()
                        .Options;
                    services.AddSingleton(queryOptions);
                    var queryFactory = new InMemoryDbContextFactory(dbName);
                    services.AddSingleton<IDbContextFactory<QueryDbContext>>(queryFactory);
                    services.AddScoped(sp => queryFactory.CreateDbContext());

                    // Also register StorageDbContext options (needed by DbContextPool validators)
                    var storageOptions = new DbContextOptionsBuilder<Storage.Database.StorageDbContext>()
                        .UseInMemoryDatabase(dbName)
                        .EnableSensitiveDataLogging()
                        .Options;
                    services.AddSingleton(storageOptions);

                    // Remove all Mediator handler registrations and Storage services
                    // that depend on DB contexts we can't provide
                    var implsToRemove = services
                        .Where(d => d.ImplementationType?.FullName is string f &&
                            (f.Contains(".EventHandlers.") || f.Contains(".Queries.")
                             || f.Contains("Storage.Services.AccessChecker")
                             || f.Contains("Storage.Services.LocalImageFileManager")
                             || f.Contains("Storage.Services.LocalCompressProcessor")))
                        .ToList();
                    foreach (var d in implsToRemove)
                        services.Remove(d);

                    // Register mocks for Storage services
                    services.AddSingleton(new Mock<IAccessChecker>().Object);
                    services.AddSingleton(new Mock<ICompressProcessor>().Object);
                });
            });

        Client = Factory.CreateClient();

        // Ensure DB is created
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [TestCleanup]
    public virtual void TearDown()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }

    // ─── DbContext helpers ─────────────────────────────────────────

    protected async Task<T> RunWithDbAsync<T>(Func<QueryDbContext, Task<T>> action)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        return await action(db);
    }

    protected async Task RunWithDbAsync(Func<QueryDbContext, Task> action)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        await action(db);
    }

    // ─── Mock setup ───────────────────────────────────────────────

    protected void ResetMocksAndSetupDefaults()
    {
        GrainFactoryMock.Reset();
        AlbumGrainMock.Reset();
        UserGrainMock.Reset();
        CategoryGrainMock.Reset();
        FileManagerGrainMock.Reset();
        MediatorMock.Reset();
        ImageFileManagerMock.Reset();

        SetupDefaultGrainFactory();
        SetupDefaultGrains();
        SetupDefaultMediator();
        SetupDefaultFileManager();
    }

    protected virtual void SetupDefaultGrainFactory()
    {
        GrainFactoryMock
            .Setup(g => g.GetGrain<IAlbumGrain>(It.IsAny<long>(), It.IsAny<string?>()))
            .Returns(() => AlbumGrainMock.Object);
        GrainFactoryMock
            .Setup(g => g.GetGrain<IUserGrain>(It.IsAny<long>(), It.IsAny<string?>()))
            .Returns(() => UserGrainMock.Object);
        GrainFactoryMock
            .Setup(g => g.GetGrain<ICategoryGrain>(It.IsAny<long>(), It.IsAny<string?>()))
            .Returns(() => CategoryGrainMock.Object);
        GrainFactoryMock
            .Setup(g => g.GetGrain<IFileManagerGrain>(It.IsAny<Guid>(), It.IsAny<string?>()))
            .Returns(() => FileManagerGrainMock.Object);
    }

    protected virtual void SetupDefaultGrains()
    {
        AlbumGrainMock
            .Setup(a => a.Create(It.IsAny<AlbumTitle>(), It.IsAny<AlbumDescription>(),
                It.IsAny<AlbumTags>(), It.IsAny<CategoryId>()))
            .ReturnsAsync(new AlbumId(100));
        AlbumGrainMock.Setup(a => a.AddImage(It.IsAny<ImageId>(), It.IsAny<ImageTitle>(),
                It.IsAny<ImageTags>(), It.IsAny<ImageFileKey>()))
            .Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.Update(It.IsAny<AlbumTitle?>(), It.IsAny<AlbumDescription?>(),
                It.IsAny<AlbumTags?>(), It.IsAny<CategoryId?>()))
            .Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.Remove()).Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.Subscribe()).Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.Unsubscribe()).Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.RemoveImage(It.IsAny<ImageId>())).Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.LikeImage(It.IsAny<ImageId>())).Returns(ValueTask.CompletedTask);
        AlbumGrainMock.Setup(a => a.UnLikeImage(It.IsAny<ImageId>())).Returns(ValueTask.CompletedTask);

        UserGrainMock
            .Setup(u => u.Register(It.IsAny<Username>(), It.IsAny<Nickname>(), It.IsAny<Biography>()))
            .ReturnsAsync(new UserId(42));
        UserGrainMock
            .Setup(u => u.UpdateProfile(It.IsAny<Username?>(), It.IsAny<Nickname?>(), It.IsAny<Biography?>()))
            .Returns(ValueTask.CompletedTask);
        UserGrainMock
            .Setup(u => u.UpdateAvatar(It.IsAny<Immutable<byte[]>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        UserGrainMock
            .Setup(u => u.UpdateHeader(It.IsAny<Immutable<byte[]>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        CategoryGrainMock
            .Setup(c => c.Create(It.IsAny<CategoryName>(), It.IsAny<CategoryDescription>()))
            .ReturnsAsync(new CategoryId(10));
        CategoryGrainMock.Setup(c => c.Update(It.IsAny<CategoryName?>(), It.IsAny<CategoryDescription?>()))
            .Returns(ValueTask.CompletedTask);
        CategoryGrainMock.Setup(c => c.Delete()).Returns(ValueTask.CompletedTask);
    }

    protected virtual void SetupDefaultMediator()
    {
        MediatorMock
            .Setup(m => m.Send(It.IsAny<CategoriesQuery>(), It.IsAny<CancellationToken>()))
            .Returns((CategoriesQuery query, CancellationToken ct) => SendCategoriesQuery(query, ct));

        MediatorMock
            .Setup(m => m.Send(It.IsAny<AlbumsQuery>(), It.IsAny<CancellationToken>()))
            .Returns((AlbumsQuery query, CancellationToken ct) => SendAlbumsQuery(query, ct));

        MediatorMock
            .Setup(m => m.Send(It.IsAny<ImagesQuery>(), It.IsAny<CancellationToken>()))
            .Returns((ImagesQuery query, CancellationToken ct) => SendImagesQuery(query, ct));

        MediatorMock
            .Setup(m => m.Send(It.IsAny<UserProfileQuery>(), It.IsAny<CancellationToken>()))
            .Returns((UserProfileQuery query, CancellationToken ct) => SendUserProfileQuery(query, ct));

        MediatorMock
            .Setup(m => m.Send(It.IsAny<UsernameExistenceQuery>(), It.IsAny<CancellationToken>()))
            .Returns((UsernameExistenceQuery query, CancellationToken ct) => SendUsernameExistenceQuery(query, ct));

        MediatorMock
            .Setup(m => m.Send(It.IsAny<ImageFileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);
    }

    protected virtual void SetupDefaultFileManager()
    {
        FileManagerGrainMock
            .Setup(f => f.UploadAsync(It.IsAny<Immutable<byte[]>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageFileKey(1));

        ImageFileManagerMock
            .Setup(f => f.GetStream(It.IsAny<ImageId>(), It.IsAny<string?>()))
            .Returns(new MemoryStream("fake-image-data"u8.ToArray()));
        ImageFileManagerMock
            .Setup(f => f.GetStream(It.IsAny<ImageId>()))
            .Returns(new MemoryStream("fake-image-data"u8.ToArray()));
    }

    // ─── Auth helpers ─────────────────────────────────────────────

    protected string GenerateToken(long userId = 1, string username = "testuser", Role role = Role.User)
    {
        var token = JwtService.Generate(new UserId(userId), new Username(username), role);
        return token.AccessToken;
    }

    protected void SetAuth(long userId = 1, string username = "testuser", Role role = Role.User)
    {
        var token = GenerateToken(userId, username, role);
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuth()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    // ─── Read model helpers ───────────────────────────────────────

    protected async Task SeedUserAsync(
        long id,
        string username,
        string nickname = "Test User",
        string biography = "Bio")
    {
        await RunWithDbAsync(async db =>
        {
            var handler = new UserRegisteredEventHandler(db);
            await handler.Handle(new UserRegisteredEvent(
                new UserId(id),
                new Username(username),
                new Nickname(nickname),
                new Biography(biography)), CancellationToken.None);
            await db.SaveChangesAsync();
        });
    }

    protected async Task SeedCategoryAsync(
        long id,
        string name,
        string description = "Category description")
    {
        await RunWithDbAsync(async db =>
        {
            var handler = new CategoryCreatedEventHandler(db);
            await handler.Handle(new CategoryCreatedEvent(
                new CategoryId(id),
                new CategoryName(name),
                new CategoryDescription(description)), CancellationToken.None);
            await db.SaveChangesAsync();
        });
    }

    protected async Task SeedAlbumAsync(
        long id,
        string title,
        long authorId,
        long categoryId,
        string description = "Album description",
        string[]? tags = null)
    {
        await RunWithDbAsync(async db =>
        {
            var handler = new AlbumCreatedEventHandler(db);
            await handler.Handle(new AlbumCreatedEvent(
                new AlbumId(id),
                new AlbumTitle(title),
                new AlbumDescription(description),
                new AlbumTags(tags ?? []),
                new CategoryId(categoryId),
                new Actor
                {
                    Id = new UserId(authorId),
                    IsAuthenticated = true,
                    Role = Role.User,
                }), CancellationToken.None);

            var album = db.ChangeTracker
                .Entries<Query.Albums.AlbumModel>()
                .Single(e => e.Entity.Id == id)
                .Entity;
            db.Entry(album).Property(nameof(Query.Albums.AlbumModel.Tags)).CurrentValue = tags ?? [];
            await db.SaveChangesAsync();
        });
    }

    protected async Task SeedImageAsync(
        long albumId,
        long imageId,
        string title,
        long uploaderId,
        string[]? tags = null,
        long[]? likes = null)
    {
        await RunWithDbAsync(async db =>
        {
            var addHandler = new AlbumImageAddedEventHandler(db);
            await addHandler.Handle(new AlbumImageAddedEvent(
                new AlbumId(albumId),
                new ImageId(imageId),
                new ImageTitle(title),
                new ImageTags(tags ?? []),
                new ImageFileKey(checked((uint)imageId)),
                new Actor
                {
                    Id = new UserId(uploaderId),
                    IsAuthenticated = true,
                    Role = Role.User,
                }), CancellationToken.None);

            var imageEntry = db.ChangeTracker
                .Entries<ImageModel>()
                .Single(e => e.Entity.Id == imageId);
            imageEntry.State = EntityState.Added;
            var image = imageEntry.Entity;
            db.Entry(image).Property(nameof(ImageModel.Likes)).CurrentValue = likes ?? [];
            await db.SaveChangesAsync();
        });
    }

    private async ValueTask<CategoryDto[]> SendCategoriesQuery(CategoriesQuery query, CancellationToken ct)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await new CategoriesQueryHandler(db).Handle(query, ct);
    }

    private async ValueTask<AlbumDto[]> SendAlbumsQuery(AlbumsQuery query, CancellationToken ct)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await new AlbumsQueryHandler(db).Handle(query, ct);
    }

    private async ValueTask<ImageDto[]> SendImagesQuery(ImagesQuery query, CancellationToken ct)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await new ImagesQueryHandler(db).Handle(query, ct);
    }

    private async ValueTask<UserProfileDto?> SendUserProfileQuery(UserProfileQuery query, CancellationToken ct)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await new UserProfileQueryHandler(db).Handle(query, ct);
    }

    private async ValueTask<UsernameExistence> SendUsernameExistenceQuery(
        UsernameExistenceQuery query,
        CancellationToken ct)
    {
        var dbFactory = Factory.Services.GetRequiredService<IDbContextFactory<QueryDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var exists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username.ToLower() == query.Username.Value.ToLower(), ct);
        return new UsernameExistence(exists);
    }
}
