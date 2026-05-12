using App.Framework;
using Client.Models;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class ProfilePage(long userId, Action<string> setPage) : IComponent
{
    public VNode Render()
    {
        var client = UseContext(RootApp.ClientCtx);
        var (profile, setProfile) = UseState<UserProfileDto>(null!);
        var (loading, setLoading) = UseState(true);
        var (error, setError) = UseState("");

        UseEffect(() => { _ = Load(); return; }, [userId]);

        async Task Load()
        {
            setLoading(true);
            try
            {
                if (userId > 0)
                {
                    var p = await client.User.GetProfileAsync(userId);
                    setProfile(p);
                }
                else
                {
                    var token = await client.GetTokenAsync();
                    // Show self info if available
                }
            }
            catch (Exception ex) { setError(ex.Message); }
            finally { setLoading(false); }
        }

        return H("div", null,
            H("div", new Dictionary<string, object?> { ["class"] = "page-header" },
                H("button", new Dictionary<string, object?> {
                    ["class"] = "md-btn text",
                    ["onclick"] = (Action)(() => setPage("albums"))
                }, new VText("← Back")),
                H("h2", null, new VText("Profile"))),
            loading
                ? H("div", new Dictionary<string, object?> { ["class"] = "loading" },
                    H("span", null, new VText("Loading...")))
                : error.Length > 0
                    ? H("p", new Dictionary<string, object?> { ["class"] = "error" }, new VText(error))
                : profile is not null
                    ? H("div", new Dictionary<string, object?> { ["class"] = "md-card profile-card" },
                        H("div", new Dictionary<string, object?> { ["class"] = "avatar" },
                            new VText(profile.Nickname[..1].ToUpper())),
                        H("div", new Dictionary<string, object?> { ["class"] = "md-card-body" },
                            H("h3", null, new VText(profile.Nickname)),
                            H("p", null, new VText($"@{profile.Username}")),
                            H("p", null, new VText(profile.Biography))))
                    : H("p", new Dictionary<string, object?> { ["class"] = "empty" },
                        new VText("Profile not found.")));
    }
}
