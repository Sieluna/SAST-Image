using App.Framework;
using App.Framework.Interop;
using Client.Models;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class AlbumDetailPage(long albumId, Action<string> setPage, Action<long> setUserId) : IComponent
{
    public VNode Render()
    {
        var client = UseContext(RootApp.ClientCtx);
        var (images, setImages) = UseState<ImageDto[]>([]);
        var (loading, setLoading) = UseState(true);
        var (thumbUrls, setThumbUrls) = UseState<Dictionary<long, string>>(new());

        UseEffect(() => { _ = Load(); return; }, [albumId]);

        async Task Load()
        {
            setLoading(true);
            try
            {
                var list = await client.Image.GetImagesAsync(albumId: albumId);
                setImages(list);
                var urls = new Dictionary<long, string>();
                foreach (var img in list)
                {
                    try
                    {
                        using var s = await client.Image.GetImageFileAsync(img.Id, ImageKind.Thumbnail);
                        if (s is null) continue;
                        using var ms = new MemoryStream();
                        await s.CopyToAsync(ms);
                        urls[img.Id] = DomInterop.CreateObjectURL(ms.ToArray(), "image/webp");
                    }
                    catch { }
                }
                setThumbUrls(urls);
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
                    images.Select(img => ImageCard(img, thumbUrls, setUserId, setPage, client)).ToArray()));
    }

    private static VNode ImageCard(
        ImageDto img, Dictionary<long, string> urls, Action<long> setUserId,
        Action<string> setPage, Client.Client client)
    {
        var src = urls.GetValueOrDefault(img.Id, "");
        return H("div", new Dictionary<string, object?> { ["class"] = "md-card image-card" },
            src.Length > 0
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
                                if (img.Requester.Liked)
                                    await client.Image.UnlikeAsync(img.AlbumId, img.Id);
                                else
                                    await client.Image.LikeAsync(img.AlbumId, img.Id);
                            }
                            catch { }
                        })
                    }, new VText(img.Requester.Liked ? "Unlike" : "Like")),
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
