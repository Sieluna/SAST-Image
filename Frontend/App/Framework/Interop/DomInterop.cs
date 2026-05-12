using System.Runtime.InteropServices.JavaScript;

namespace App.Framework.Interop;

/// <summary>[JSImport] wrappers for browser DOM APIs from main.js.</summary>
internal static partial class DomInterop
{
    [JSImport("createElement", "main.js")]
    public static partial JSObject CreateElement(string tag);

    [JSImport("createTextNode", "main.js")]
    public static partial JSObject CreateTextNode(string text);

    [JSImport("appendChild", "main.js")]
    public static partial void AppendChild(JSObject parent, JSObject child);

    [JSImport("removeChild", "main.js")]
    public static partial void RemoveChild(JSObject parent, JSObject child);

    [JSImport("replaceChild", "main.js")]
    public static partial void ReplaceChild(JSObject parent, JSObject newChild, JSObject oldChild);

    [JSImport("insertBefore", "main.js")]
    public static partial void InsertBefore(JSObject parent, JSObject child, JSObject? reference);

    [JSImport("setAttribute", "main.js")]
    public static partial void SetAttribute(JSObject element, string name, string value);

    [JSImport("removeAttribute", "main.js")]
    public static partial void RemoveAttribute(JSObject element, string name);

    [JSImport("setTextContent", "main.js")]
    public static partial void SetTextContent(JSObject element, string text);

    [JSImport("setEvent", "main.js")]
    public static partial void SetEvent(
        JSObject element,
        string eventType,
        [JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> handler);

    [JSImport("scheduleFlush", "main.js")]
    public static partial void ScheduleFlush();

    [JSImport("querySelector", "main.js")]
    public static partial JSObject? QuerySelector(string selector);

    [JSImport("setTimeout", "main.js")]
    public static partial int SetTimeout(
        [JSMarshalAs<JSType.Function<JSType.Void>>] Action callback,
        int delayMs);
}
