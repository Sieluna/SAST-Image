using App.Framework.Interop;
using App.Framework.Reactive;

namespace App.Framework;

/// <summary>Public entry point and VNode creation helpers.</summary>
public static class WebApp
{
    /// <summary>Mount a root component into the DOM element.</summary>
    public static void Mount<TComponent>(string cssSelector)
        where TComponent : IComponent, new()
    {
        var container = DomInterop.QuerySelector(cssSelector)
            ?? throw new InvalidOperationException($"No element found for selector: {cssSelector}");

        var rootComp = new TComponent();
        var componentFn = () => rootComp.Render();

        // The root component is treated as a VComponent inside a virtual parent.
        var parentComp = new ComponentInstance(() => new VText("__root__"), null, null);
        var vcomp = new VComponent(componentFn);

        Renderer.Reconcile(null, vcomp, container, parentComp);
    }

    // ── h() helpers ──────────────────────────────────────────────────

    /// <summary>Create an element with text content.</summary>
    public static VNode H(string tag, string text)
        => new VElement(tag, new(), new VNode[] { new VText(text) });

    /// <summary>Create an element with children.</summary>
    public static VNode H(string tag, params VNode[] children)
        => new VElement(tag, new(), children);

    /// <summary>Create an element with props and children.</summary>
    public static VNode H(string tag, object props, params VNode[] children)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var pi in props.GetType().GetProperties())
            dict[pi.Name] = pi.GetValue(props);
        return new VElement(tag, dict, children);
    }

    /// <summary>Create a component from a render function.</summary>
    public static VNode H(Func<VNode> component, string? key = null)
        => new VComponent(component, key);

    /// <summary>Create a component from its type. A new instance is created on mount.</summary>
    public static VNode H<T>(string? key = null) where T : IComponent, new()
        => new VComponent(() => new T().Render(), key);

    /// <summary>Wrap children in a fragment.</summary>
    public static VNode Fragment(params VNode[] children)
        => new VFragment(children);

    /// <summary>Create a context provider.</summary>
    public static VNode Provide<T>(Context<T> ctx, T value, VNode child) where T : class
        => new VContextProvider(typeof(Context<T>), value, child);
}

/// <summary>User component: provide a Render() method.</summary>
public interface IComponent
{
    VNode Render();
}
