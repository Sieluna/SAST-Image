using App.Framework;
using App.Framework.Reactive;
using Client;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class RootApp : IComponent
{
    public static readonly Context<SastClient> ClientCtx = Context.Create<SastClient>();

    public VNode Render()
    {
        var (page, setPage) = UseState("albums");
        var (client, setClient) = UseState<SastClient>(null!);
        var (albumId, setAlbumId) = UseState(0L);
        var (userId, setUserId) = UseState(0L);

        if (client is null)
        {
            return H("div", new Dictionary<string, object?> { ["class"] = "app" },
                H(() => new LoginPage(v => setClient(v), setPage).Render(), "login"));
        }

        var nav = (string p) => (Action)(() => { setPage(p); setAlbumId(0); });

        VNode body = page switch
        {
            "album" => H(() => new AlbumDetailPage(albumId, setPage, setUserId).Render(), "album-detail"),
            "profile" => H(() => new ProfilePage(userId, setPage).Render(), "profile"),
            _ => H(() => new AlbumListPage(setPage, setAlbumId).Render(), "album-list"),
        };

        return Provide(ClientCtx, client,
            H("div", new Dictionary<string, object?> { ["class"] = "app" },
                H("header", new Dictionary<string, object?> { ["class"] = "top-bar" },
                    H("span", new Dictionary<string, object?> { ["class"] = "logo" },
                        new VText("Sast Image")),
                    H("nav", null,
                        TabBtn("Albums", page == "albums", nav("albums")),
                        TabBtn("Profile", page == "profile", (Action)(() => { setPage("profile"); setUserId(0); })))),
                H("main", new Dictionary<string, object?> { ["class"] = "main-content" }, body)));
    }

    private static VNode TabBtn(string label, bool active, Action onClick)
        => H("button", new Dictionary<string, object?> {
            ["class"] = "tab " + (active ? "active" : ""),
            ["onclick"] = onClick
        }, new VText(label));
}
