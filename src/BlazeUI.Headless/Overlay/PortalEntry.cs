using Microsoft.AspNetCore.Components;

namespace BlazeUI.Headless.Overlay;

/// <summary>
/// Represents a single piece of content mounted into the portal host.
/// </summary>
internal sealed record PortalEntry(string Id, RenderFragment Content, int ZIndex)
{
    /// <summary>
    /// When false, the PortalHost renders the entry wrapper with <c>display: none</c>.
    /// Overlay portals set this to false after exit animations complete so closed
    /// overlay content doesn't leave empty containers in the DOM.
    /// </summary>
    public bool Active { get; set; } = true;
}
