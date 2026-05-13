using System.Net;
using Domain;
using Domain.Album;
using Domain.Album.Image;
using Domain.Api;
using Domain.Category;
using Domain.File;
using Domain.User;
using Orleans.Concurrency;
using Storage.Images;
using Storage.Images.Queries;

namespace Interface.Tests;

[TestClass]
public sealed class InterfaceTests : TestBase
{
    [TestMethod]
    public async Task AuthScenario_RegisterLoginAndRefresh_UsesReadModel()
    {
        var register = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest("alice", "Alice", "Camera ready"));

        register.StatusCode.ShouldBe(HttpStatusCode.OK);
        var registeredToken = await register.Content.ReadFromJsonAsync<JwtTokenResponse>();
        registeredToken.ShouldNotBeNull();
        registeredToken!.AccessToken.ShouldNotBeNullOrEmpty();
        registeredToken.RefreshToken.ShouldNotBeNullOrEmpty();
        UserGrainMock.Verify(g => g.Register(
            It.Is<Username>(u => u.Value == "alice"),
            It.Is<Nickname>(n => n.Value == "Alice"),
            It.Is<Biography>(b => b.Value == "Camera ready")), Times.Once);

        await SeedUserAsync(42, "alice", "Alice", "Camera ready");

        var login = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("alice"));

        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loginToken = await login.Content.ReadFromJsonAsync<JwtTokenResponse>();
        loginToken.ShouldNotBeNull();
        loginToken!.AccessToken.ShouldNotBeNullOrEmpty();

        var refresh = await Client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest(loginToken.RefreshToken));

        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        var refreshedToken = await refresh.Content.ReadFromJsonAsync<JwtTokenResponse>();
        refreshedToken.ShouldNotBeNull();
        refreshedToken!.AccessToken.ShouldNotBeNullOrEmpty();

        var unknownLogin = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("missing"));
        unknownLogin.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var invalidRefresh = await Client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest("invalid-refresh-token"));
        invalidRefresh.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task ReadModelScenario_CatalogProfileAndImages_AreServedThroughHttp()
    {
        await SeedUserAsync(42, "alice", "Alice", "Camera ready");
        await SeedCategoryAsync(7, "Nature", "Outdoor scenes");
        await SeedAlbumAsync(100, "Summer", 42, 7, "Warm light", ["sun", "travel"]);
        await SeedImageAsync(100, 501, "Sunset", 42, ["gold"], [42]);

        SetAuth(42, "alice");

        var categoriesResponse = await Client.GetAsync("/api/v1/categories");
        categoriesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<CategoryResponse[]>();
        categories.ShouldNotBeNull();
        categories!.Single().Name.ShouldBe("Nature");

        var albumsResponse = await Client.GetAsync("/api/v1/albums?categoryId=7");
        albumsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var albums = await albumsResponse.Content.ReadFromJsonAsync<AlbumListResponse>();
        albums.ShouldNotBeNull();
        albums!.Albums.Single().ShouldSatisfyAllConditions(
            album => album.Id.ShouldBe(100),
            album => album.Title.ShouldBe("Summer"),
            album => album.AuthorName.ShouldBe("alice"),
            album => album.CategoryName.ShouldBe("Nature"));

        var albumResponse = await Client.GetAsync("/api/v1/albums/100");
        albumResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var album = await albumResponse.Content.ReadFromJsonAsync<AlbumResponse>();
        album.ShouldNotBeNull();
        album!.Tags.ShouldBe(["sun", "travel"]);

        var profileResponse = await Client.GetAsync("/api/v1/users/profile");
        profileResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        profile.ShouldNotBeNull();
        profile!.Username.ShouldBe("alice");
        profile.Nickname.ShouldBe("Alice");

        var imagesResponse = await Client.GetAsync("/api/v1/albums/100/images");
        imagesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var images = await imagesResponse.Content.ReadFromJsonAsync<ImageResponse[]>();
        images.ShouldNotBeNull();
        images!.Single().ShouldSatisfyAllConditions(
            image => image.Id.ShouldBe(501),
            image => image.Title.ShouldBe("Sunset"),
            image => image.Likes.ShouldBe(1),
            image => image.Liked.ShouldBeTrue(),
            image => image.ThumbnailUrl.ShouldBe("/images/501"));
    }

    [TestMethod]
    public async Task CommandScenario_ProtectedMutationsDispatchExpectedGrains()
    {
        SetAuth(42, "alice");

        var category = await Client.PostAsJsonAsync(
            "/api/v1/categories",
            new CreateCategoryRequest("Portrait", "People"));
        category.StatusCode.ShouldBe(HttpStatusCode.OK);

        var album = await Client.PostAsJsonAsync(
            "/api/v1/albums",
            new CreateAlbumRequest("New Album", "Description", ["tag1"], 5));
        album.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updateAlbum = await Client.PutAsJsonAsync(
            "/api/v1/albums/10",
            new UpdateAlbumRequest("Updated", null, null, null));
        updateAlbum.StatusCode.ShouldBe(HttpStatusCode.OK);

        var imageBytes = new byte[] { 1, 2, 3, 4, 5 };
        var addImage = await Client.PostAsJsonAsync(
            "/api/v1/albums/10/images",
            new AddImageRequest("New Image", ["test"], imageBytes));
        addImage.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await Client.PostAsync("/api/v1/albums/10/images/5/like", null))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Client.DeleteAsync("/api/v1/albums/10/images/5/like"))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Client.DeleteAsync("/api/v1/albums/10/images/5"))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Client.PostAsync("/api/v1/albums/10/subscribe", null))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Client.DeleteAsync("/api/v1/albums/10/subscribe"))
            .StatusCode.ShouldBe(HttpStatusCode.OK);

        var updateProfile = await Client.PutAsJsonAsync(
            "/api/v1/users/profile",
            new UpdateProfileRequest("alice2", "Alice Two", "Updated bio"));
        updateProfile.StatusCode.ShouldBe(HttpStatusCode.OK);

        var avatarBytes = new byte[] { 10, 20, 30 };
        var avatar = await Client.PutAsync("/api/v1/users/avatar", new ByteArrayContent(avatarBytes));
        avatar.StatusCode.ShouldBe(HttpStatusCode.OK);

        CategoryGrainMock.Verify(g => g.Create(
            It.Is<CategoryName>(n => n.Value == "Portrait"),
            It.Is<CategoryDescription>(d => d.Value == "People")), Times.Once);
        AlbumGrainMock.Verify(g => g.Create(
            It.Is<AlbumTitle>(t => t.Value == "New Album"),
            It.Is<AlbumDescription>(d => d.Value == "Description"),
            It.Is<AlbumTags>(t => t.Value.SequenceEqual(new[] { "tag1" })),
            It.Is<CategoryId>(c => c.Value == 5)), Times.Once);
        AlbumGrainMock.Verify(g => g.Update(
            It.Is<AlbumTitle>(t => t.Value == "Updated"),
            null, null, null), Times.Once);
        FileManagerGrainMock.Verify(g => g.UploadAsync(
            It.Is<Immutable<byte[]>>(b => b.Value.SequenceEqual(imageBytes)),
            It.IsAny<CancellationToken>()), Times.Once);
        AlbumGrainMock.Verify(g => g.AddImage(
            It.IsAny<ImageId>(),
            It.Is<ImageTitle>(t => t.Value == "New Image"),
            It.Is<ImageTags>(t => t.Value.SequenceEqual(new[] { "test" })),
            It.Is<ImageFileKey>(k => k.Value == 1)), Times.Once);
        AlbumGrainMock.Verify(g => g.LikeImage(It.Is<ImageId>(id => id.Value == 5)), Times.Once);
        AlbumGrainMock.Verify(g => g.UnLikeImage(It.Is<ImageId>(id => id.Value == 5)), Times.Once);
        AlbumGrainMock.Verify(g => g.RemoveImage(It.Is<ImageId>(id => id.Value == 5)), Times.Once);
        AlbumGrainMock.Verify(g => g.Subscribe(), Times.Once);
        AlbumGrainMock.Verify(g => g.Unsubscribe(), Times.Once);
        UserGrainMock.Verify(g => g.UpdateProfile(
            It.Is<Username>(u => u.Value == "alice2"),
            It.Is<Nickname>(n => n.Value == "Alice Two"),
            It.Is<Biography>(b => b.Value == "Updated bio")), Times.Once);
        UserGrainMock.Verify(g => g.UpdateAvatar(
            It.Is<Immutable<byte[]>>(b => b.Value.SequenceEqual(avatarBytes)),
            It.IsAny<CancellationToken>()), Times.Once);

        GrainFactoryMock.Verify(g => g.GetGrain<IAlbumGrain>(10, It.IsAny<string?>()), Times.AtLeast(6));
        GrainFactoryMock.Verify(g => g.GetGrain<IUserGrain>(42, It.IsAny<string?>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task AuthorizationScenario_ProtectedEndpointsRejectAnonymousRequests()
    {
        ClearAuth();

        var protectedRequests = new[]
        {
            new HttpRequestMessage(HttpMethod.Get, "/api/v1/albums"),
            new HttpRequestMessage(HttpMethod.Get, "/api/v1/albums/1"),
            new HttpRequestMessage(HttpMethod.Post, "/api/v1/categories")
            {
                Content = JsonContent.Create(new CreateCategoryRequest("X", "Y")),
            },
            new HttpRequestMessage(HttpMethod.Post, "/api/v1/albums")
            {
                Content = JsonContent.Create(new CreateAlbumRequest("X", "Y", [], 1)),
            },
            new HttpRequestMessage(HttpMethod.Get, "/api/v1/albums/1/images"),
            new HttpRequestMessage(HttpMethod.Put, "/api/v1/users/profile")
            {
                Content = JsonContent.Create(new UpdateProfileRequest("x", null, null)),
            },
        };

        foreach (var request in protectedRequests)
        {
            var response = await Client.SendAsync(request);
            response.StatusCode.ShouldBe(
                HttpStatusCode.Unauthorized,
                $"{request.Method} {request.RequestUri} should require authentication");
        }

        var publicCategories = await Client.GetAsync("/api/v1/categories");
        publicCategories.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task FileServingScenario_UsesAnonymousOrAuthenticatedActor()
    {
        var anonymousBytes = "anonymous-image"u8.ToArray();
        MediatorMock
            .Setup(m => m.Send(
                It.Is<ImageFileQuery>(q => q.Image.Value == 1 && !q.Actor.IsAuthenticated),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(anonymousBytes));

        ClearAuth();
        var anonymous = await Client.GetAsync("/images/1");
        anonymous.StatusCode.ShouldBe(HttpStatusCode.OK);
        anonymous.Content.Headers.ContentType!.MediaType.ShouldBe("image/webp");
        (await anonymous.Content.ReadAsByteArrayAsync()).ShouldBe(anonymousBytes);

        var authenticatedBytes = "authenticated-image"u8.ToArray();
        MediatorMock
            .Setup(m => m.Send(
                It.Is<ImageFileQuery>(q =>
                    q.Image.Value == 2 &&
                    q.Kind == ImageKind.Original &&
                    q.Actor.IsAuthenticated &&
                    q.Actor.Id.Value == 42),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(authenticatedBytes));

        SetAuth(42, "alice");
        var authenticated = await Client.GetAsync("/images/2");
        authenticated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await authenticated.Content.ReadAsByteArrayAsync()).ShouldBe(authenticatedBytes);

        var missing = await Client.GetAsync("/images/404");
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
