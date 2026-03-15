namespace BlazeUI.Headless.Core;

/// <summary>
/// Global configuration for the BlazeUI component library.
/// </summary>
public sealed class BlazeUIConfiguration
{
    /// <summary>
    /// Text direction for all components. Defaults to <see cref="TextDirection.Ltr"/>.
    /// </summary>
    public TextDirection Direction { get; set; } = TextDirection.Ltr;

    /// <summary>
    /// Whether CSS animations and transitions are enabled. When <c>false</c>,
    /// exit animation safety timeouts resolve immediately.
    /// </summary>
    public bool AnimationsEnabled { get; set; } = true;

    /// <summary>
    /// Whether to honour <c>prefers-reduced-motion</c> at the library level.
    /// </summary>
    public bool ReducedMotion { get; set; }

    /// <summary>
    /// Base path for static content. Defaults to <c>/_content/BlazeUI.Headless</c>.
    /// </summary>
    public string BasePath { get; set; } = "/_content/BlazeUI.Headless";

    /// <summary>
    /// Appended as a <c>?v=</c> query string to JS module URLs for cache busting.
    /// </summary>
    public string? JsVersionSuffix { get; set; }
}

public enum TextDirection
{
    Ltr,
    Rtl
}
