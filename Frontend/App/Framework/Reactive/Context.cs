namespace App.Framework.Reactive;

/// <summary>
/// A context value that flows down the component tree. Components consume it via
/// <c>useContext</c>, and providers override it for their subtree.
/// </summary>
public sealed class Context<T>
    where T : class
{
    public T DefaultValue { get; }

    public Context(T defaultValue) => DefaultValue = defaultValue;
}

public static class Context
{
    /// <summary>Create a new context with the given default value.</summary>
    public static Context<T> Create<T>(T defaultValue) where T : class => new(defaultValue);
}
