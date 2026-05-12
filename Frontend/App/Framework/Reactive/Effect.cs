namespace App.Framework.Reactive;

internal sealed class Effect
{
    private readonly Action _execute;
    private readonly HashSet<IStateSignal> _dependencies = new();
    private Scope _scope = new();
    private bool _isExecuting;

    [ThreadStatic]
    internal static Stack<Effect>? ContextStack;

    [ThreadStatic]
    internal static Scope? OwnerScope;

    /// <summary>The effect currently executing on this thread.</summary>
    internal static Effect? Current =>
        ContextStack is { Count: > 0 } ? ContextStack.Peek() : null;

    internal Effect(Action fn) => _execute = fn;

    internal void AddDependency(IStateSignal signal)
    {
        _dependencies.Add(signal);
    }

    internal void ClearDependencies()
    {
        foreach (var dep in _dependencies)
            dep.Unsubscribe(this);
        _dependencies.Clear();
    }

    private void SubscribeToDependencies()
    {
        foreach (var dep in _dependencies)
            dep.Subscribe(this, Execute);
    }

    /// <summary>
    /// Run (or re-run) the effect body inside a fresh tracking context.
    /// Clears old dependencies and disposes the previous child scope first.
    /// </summary>
    internal void Execute()
    {
        if (_isExecuting) return;
        _isExecuting = true;

        ContextStack ??= new();
        ContextStack.Push(this);

        ClearDependencies();

        var oldScope = _scope;
        _scope = new Scope();
        oldScope.Dispose();

        var prevOwner = OwnerScope;
        OwnerScope = _scope;

        try
        {
            _execute();
        }
        finally
        {
            ContextStack.Pop();
            OwnerScope = prevOwner;
            _isExecuting = false;
        }

        SubscribeToDependencies();
    }
}

/// <summary>
/// Owns a set of child effects and cleanup callbacks. When disposed, tears down
/// all effects (clears their dependencies) and runs all cleanup callbacks in
/// reverse order.
/// </summary>
public sealed class Scope : IDisposable
{
    private readonly List<WeakReference<Effect>> _effects = new();
    private readonly List<Action> _cleanups = new();

    internal void AddEffect(Effect effect) => _effects.Add(new(effect));

    internal void AddCleanup(Action cleanup) => _cleanups.Add(cleanup);

    public void Dispose()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            if (_effects[i].TryGetTarget(out var effect))
                effect.ClearDependencies();
        }
        _effects.Clear();

        for (int i = _cleanups.Count - 1; i >= 0; i--)
            _cleanups[i]();
        _cleanups.Clear();
    }
}

public static class Rx
{
    private static readonly Scope s_globalScope = new();

    /// <summary>Create an auto-tracking effect, owned by the current scope.</summary>
    public static void CreateEffect(Action fn)
    {
        var effect = new Effect(fn);
        (Effect.OwnerScope ?? s_globalScope).AddEffect(effect);
        effect.Execute();
    }

    /// <summary>Create a reactive root scope that owns nested effects.</summary>
    public static Scope CreateRoot(Action fn)
    {
        var prevOwner = Effect.OwnerScope;
        var scope = new Scope();

        Effect.OwnerScope = scope;
        try
        {
            fn();
        }
        finally
        {
            Effect.OwnerScope = prevOwner;
        }

        return scope;
    }

    /// <summary>Run action outside tracking context — state reads won't create deps.</summary>
    public static void Untrack(Action action)
    {
        var savedStack = Effect.ContextStack;
        Effect.ContextStack = null;

        try
        {
            action();
        }
        finally
        {
            Effect.ContextStack = savedStack;
        }
    }

    /// <summary>Run fn outside tracking context — state reads won't create deps.</summary>
    public static T Untrack<T>(Func<T> fn)
    {
        var savedStack = Effect.ContextStack;
        Effect.ContextStack = null;
        try
        {
            return fn();
        }
        finally
        {
            Effect.ContextStack = savedStack;
        }
    }

    /// <summary>Register cleanup that runs on effect re-run or scope disposal.</summary>
    public static void OnCleanup(Action fn)
    {
        Effect.OwnerScope?.AddCleanup(fn);
    }

}
