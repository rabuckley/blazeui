using Microsoft.AspNetCore.Components;

namespace BlazeUI.Styled.Demo.Components.UI;

/// <summary>
/// Cascading state for the Sidebar component tree. Provides
/// open/collapsed state, mobile state, and toggle callbacks
/// to all sidebar sub-components.
/// </summary>
public class SidebarContext
{
    public bool Open { get; set; } = true;
    public bool OpenMobile { get; set; }
    public bool IsMobile { get; set; }
    public EventCallback ToggleSidebar { get; set; }
    public EventCallback<bool> SetOpenMobile { get; set; }
    public string State => Open ? "expanded" : "collapsed";
}
