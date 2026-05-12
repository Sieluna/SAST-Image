using App.Framework;
using Client.Models;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class AlbumListPage(Action<string> setPage, Action<long> setAlbumId) : IComponent
{
    public VNode Render()
    {
        var client = UseContext(RootApp.ClientCtx);
        var (albums, setAlbums) = UseState<AlbumDto[]>([]);
        var (categories, setCategories) = UseState<CategoryDto[]>([]);
        var (loading, setLoading) = UseState(true);
        var (activeCat, setActiveCat) = UseState(0L);

        UseEffect(() => { _ = Load(); return; }, [activeCat]);

        async Task Load()
        {
            setLoading(true);
            try
            {
                var cats = await client.Category.GetAllAsync();
                setCategories(cats);
                var list = await client.Album.GetAlbumsAsync(
                    categoryId: activeCat > 0 ? activeCat : null);
                setAlbums(list);
            }
            catch { }
            finally { setLoading(false); }
        }

        var chips = new List<VNode> {
            H("button", new Dictionary<string, object?> {
                ["class"] = "chip " + (activeCat == 0 ? "active" : ""),
                ["onclick"] = (Action)(() => setActiveCat(0))
            }, new VText("All"))
        };
        foreach (var c in categories)
        {
            var id = c.Id;
            chips.Add(H("button", new Dictionary<string, object?> {
                ["class"] = "chip " + (activeCat == id ? "active" : ""),
                ["onclick"] = (Action)(() => setActiveCat(id))
            }, new VText(c.Name)));
        }

        return H("div", null,
            H("div", new Dictionary<string, object?> { ["class"] = "chip-row" }, chips.ToArray()),
            loading
                ? H("div", new Dictionary<string, object?> { ["class"] = "loading" },
                    H("span", null, new VText("Loading albums...")))
                : H("div", new Dictionary<string, object?> { ["class"] = "card-grid" },
                    albums.Length == 0
                        ? [H("p", new Dictionary<string, object?> { ["class"] = "empty" },
                            new VText("No albums yet."))]
                        : albums.Select(a => Card(a)).ToArray()));
    }

    private VNode Card(AlbumDto a)
        => H("div", new Dictionary<string, object?> {
            ["class"] = "md-card",
            ["onclick"] = (Action)(() => { setAlbumId(a.Id); setPage("album"); })
        },
            H("div", new Dictionary<string, object?> { ["class"] = "md-card-body" },
                H("h3", null, new VText(a.Title)),
                H("p", null, new VText(a.Description.Length > 120
                    ? a.Description[..120] + "..." : a.Description)),
                H("div", new Dictionary<string, object?> { ["class"] = "md-card-meta" },
                    H("span", null, new VText($"♡ {a.SubscribeCount}")),
                    H("span", null, new VText(AccessIcon(a.AccessLevel))))));
    // Subscribe not easily doable without re-render — skip action button for now.

    private static string AccessIcon(AccessLevel l) => l switch  /* static OK — no instance access */
    {
        AccessLevel.Private => "🔒",
        AccessLevel.AuthReadOnly => "👁",
        AccessLevel.AuthReadWrite => "✏️",
        AccessLevel.PublicReadOnly => "🌐",
        AccessLevel.PublicReadWrite => "🌐",
        _ => ""
    };
}
