using System.Runtime.InteropServices.JavaScript;
using App.Framework;
using Client;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class LoginPage(Action<SastClient> onLogin, Action<string> setPage) : IComponent
{
    public VNode Render()
    {
        var (username, setUsername) = UseState("");
        var (password, setPassword) = UseState("");
        var (error, setError) = UseState("");
        var (loading, setLoading) = UseState(false);

        var field = (string placeholder, string type, string value, Action<string> set) =>
            H("input", new Dictionary<string, object?> {
                ["class"] = "md-field",
                ["type"] = type,
                ["placeholder"] = placeholder,
                ["value"] = value,
                ["oninput"] = (Action<JSObject>)(e =>
                    set(e.GetPropertyAsJSObject("target")!.GetPropertyAsString("value") ?? ""))
            });

        return H("div", new Dictionary<string, object?> { ["class"] = "login-page" },
            H("div", new Dictionary<string, object?> { ["class"] = "login-card" },
                H("div", new Dictionary<string, object?> { ["class"] = "login-hero" },
                    H("h1", null, new VText("Sast Image")),
                    H("p", null, new VText("Sign in to explore albums"))),

                field("Username", "text", username, setUsername),
                field("Password", "password", password, setPassword),

                error.Length > 0
                    ? H("p", new Dictionary<string, object?> { ["class"] = "error" }, new VText(error))
                    : new VText(""),

                H("button", new Dictionary<string, object?> {
                    ["class"] = "md-btn primary full",
                    ["onclick"] = (Action)(async () =>
                    {
                        if (loading) return;
                        setLoading(true);
                        setError("");
                        try
                        {
                            var client = new SastClient(
                                "http://localhost:5078",
                                new BrowserStorage());
                            await client.SignalR().LoginAsync(username, password);
                            onLogin(client);
                        }
                        catch (Exception ex) { setError(ex.Message); }
                        finally { setLoading(false); }
                    })
                }, new VText(loading ? "Signing in..." : "Sign In"))
            ));
    }
}
