using App.Framework;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class RootApp : IComponent
{
    public VNode Render()
    {
        return H("div", new Dictionary<string, object?> { ["class"] = "app" },
            H("h1", null, new VText("Sast Image — Reactive Framework")),
            H<Counter>("counter-1"),
            H("footer", null, new VText("Built with a minimal hook + context reactive core."))
        );
    }
}

/// <summary>Standalone counter component: useState + event handling.</summary>
file class Counter : IComponent
{
    public VNode Render()
    {
        var (count, setCount) = UseState(0);

        return H("div", new Dictionary<string, object?> { ["class"] = "counter" },
            H("p", null, new VText($"Count: {count}")),
            H("button", new Dictionary<string, object?> { ["onclick"] = (Action)(() => setCount(count + 1)) },
                new VText("+1")),
            H("button", new Dictionary<string, object?> { ["onclick"] = (Action)(() => setCount(0)) },
                new VText("Reset"))
        );
    }
}
