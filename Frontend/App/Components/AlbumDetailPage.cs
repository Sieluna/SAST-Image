using App.Framework;
using App.Models;
using Client.SignalR;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class AlbumDetailPage(long albumId, Action<string> setPage, Action<long> setUserId) : IComponent
{
    public VNode Render()
    {
        var sastClient = UseContext(RootApp.ClientCtx);
        var signalR = sastClient.SignalR();
        var baseUrl = sastClient.Options.BaseUrl.TrimEnd('/');
        var (images, setImages) = UseState<ImageModel[]>([]);
        var (loading, setLoading) = UseState(true);

        UseEffect(() => { _ = Load(); return; }, [albumId]);

        async Task Load()
        {
            setLoading(true);
            try
            {
                var list = await signalR.GetImagesAsync(albumId: albumId);
                setImages(list.Select(i => (ImageModel)i).ToArray());
            }
            catch { }
            finally { setLoading(false); }
        }

        return H("div", null,
            H("div", new Dictionary<string, object?> { ["class"] = "page-header" },
                H("button", new Dictionary<string, object?> {
                    ["class"] = "md-btn text",
                    ["onclick"] = (Action)(() => setPage("albums"))
                }, new VText("← Albums")),
                H("h2", null, new VText($"{images.Length} images"))),
            loading
                ? H("div", new Dictionary<string, object?> { ["class"] = "loading" },
                    H("span", null, new VText("Loading...")))
                : H("div", new Dictionary<string, object?> { ["class"] = "image-grid" },
                    images.Select(img => ImageCard(img, baseUrl, setUserId, setPage, signalR)).ToArray()));
    }

    private static VNode ImageCard(
        ImageModel img, string baseUrl, Action<long> setUserId,
        Action<string> setPage, SignalRClient signalR)
    {
        var src = img.ThumbnailUrl is not null ? $"{baseUrl}{img.ThumbnailUrl}" : null;
        return H("div", new Dictionary<string, object?> { ["class"] = "md-card image-card" },
            src is not null
                ? H("img", new Dictionary<string, object?> { ["src"] = src, ["alt"] = img.Title })
                : H("div", new Dictionary<string, object?> { ["class"] = "img-fallback" },
                    new VText("🖼")),
            H("div", new Dictionary<string, object?> { ["class"] = "md-card-body" },
                H("h4", null, new VText(img.Title)),
                H("div", new Dictionary<string, object?> { ["class"] = "md-card-meta" },
                    H("span", null, new VText($"♥ {img.Likes}")),
                    H("button", new Dictionary<string, object?> {
                        ["class"] = "md-btn text sm",
                        ["onclick"] = (Action)(async () =>
                        {
                            try
                            {
                                if (img.Liked)
                                    await signalR.UnlikeImageAsync(img.AlbumId, img.Id);
                                else
                                    await signalR.LikeImageAsync(img.AlbumId, img.Id);
                            }
                            catch { }
                        })
                    }, new VText(img.Liked ? "Unlike" : "Like")),
                    H("button", new Dictionary<string, object?> {
                        ["class"] = "md-btn text sm",
                        ["onclick"] = (Action)(() =>
                        {
                            setUserId(img.UploaderId);
                            setPage("profile");
                        })
                    }, new VText("Uploader")))));
    }
}
