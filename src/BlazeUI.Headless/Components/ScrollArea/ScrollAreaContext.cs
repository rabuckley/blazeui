using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.ScrollArea;

/// <summary>
/// State shared between <see cref="ScrollAreaRoot"/> and its children. All
/// scroll measurements and DOM mutations are performed by the JS module; this
/// context only carries the stable identifiers needed to wire up JS calls.
/// </summary>
internal sealed class ScrollAreaContext
{
    /// <summary>The root element ID, used to namespace data-id attributes.</summary>
    public string RootId { get; set; } = "";

    /// <summary>The viewport element ID passed to the JS module on init.</summary>
    public string ViewportId { get; set; } = "";

    /// <summary>The JS module reference, set after first render.</summary>
    public IJSObjectReference? JsModule { get; set; }

    /// <summary>The .NET callback reference, set after first render.</summary>
    public DotNetObjectReference<ScrollAreaRoot>? DotNetRef { get; set; }
}
