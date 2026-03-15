using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Components.Toolbar;

internal sealed class ToolbarContext
{
    public bool Disabled { get; set; }
    public Orientation Orientation { get; set; } = Orientation.Horizontal;
}
