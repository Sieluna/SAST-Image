using App.Framework;
using App.Framework.Reactive;
using Client;
using static App.Framework.WebApp;
using static App.Framework.Hooks;
using static App.Framework.Tags;

namespace App.Components;

public partial class App : IComponent
{
    // TODO: Use URL-based routing so browser back/forward and deep-links work
    public static readonly Context<SastClient> ClientCtx = Context.Create<SastClient>();

    public VNode Render()
    {
        var (page, setPage) = UseState("albums");
        var (client, setClient) = UseState<SastClient>(null!);
        var (albumId, setAlbumId) = UseState(0L);
        var (userId, setUserId) = UseState(0L);

        if (client is null)
        {
            return div("app",
                H(() => new LoginPage(v => setClient(v), setPage).Render(), "login"));
        }

        var goTo = (string p) => (Action)(() => { setPage(p); setAlbumId(0); });

        VNode body = page switch
        {
            "album" => H(() => new AlbumDetailPage(albumId, setPage, setUserId).Render(), "album-detail"),
            "profile" => H(() => new ProfilePage(userId, setPage).Render(), "profile"),
            _ => H(() => new AlbumListPage(setPage, setAlbumId).Render(), "album-list"),
        };

        return Provide(ClientCtx, client,
            div("app",
                header(Css.top_bar,
                    span(Css.logo, txt("Sast Image")),
                    nav(
                        TabBtn("Albums", page == "albums", goTo("albums")),
                        TabBtn("Profile", page == "profile",
                            (Action)(() => { setPage("profile"); setUserId(0); })))),
                main("main-content", body)));
    }

    private static VNode TabBtn(string label, bool active, Action onClick)
        => button(Css.tab + (active ? " " + Css.active : ""), onClick, txt(label));
}
