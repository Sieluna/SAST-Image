using App.Framework;
using static App.Framework.WebApp;
using static App.Framework.Hooks;

namespace App.Components;

public class RootApp : IComponent
{
    public VNode Render()
    {
        return H("div", new { @class = "app" },
            H("h1", "Sast Image — Reactive Framework"),
            H<Counter>("counter-1"),
            H("footer", "Built with a minimal hook + context reactive core.")
        );
    }
}

/// <summary>Standalone counter component: useState + event handling.</summary>
file class Counter : IComponent
{
    public VNode Render()
    {
        var (count, setCount) = UseState(0);

        return H("div", new { @class = "counter" },
            H("p", $"Count: {count}"),
            H("button", new { onclick = (Action)(() => setCount(count + 1)) }, H("text", "+1")),
            H("button", new { onclick = (Action)(() => setCount(0)) }, H("text", "Reset"))
        );
    }
}
