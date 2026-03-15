using BlazeUI.Headless.Core;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Accordion;

internal sealed class AccordionContext
{
    public IReadOnlyList<string> OpenItems { get; set; } = Array.Empty<string>();
    public bool Multiple { get; set; }
    public bool Disabled { get; set; }
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public bool LoopFocus { get; set; } = true;
    public bool KeepMounted { get; set; }
    public bool HiddenUntilFound { get; set; }
    public Func<string, Task> Toggle { get; set; } = _ => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<AccordionRoot>? DotNetRef { get; set; }

    /// <summary>
    /// Panel IDs whose close animation is in progress. While a panel
    /// is animating closed, it must remain visible (no <c>hidden</c>
    /// attribute) so the CSS exit animation can play.
    /// </summary>
    public HashSet<string> ClosingPanelIds { get; } = [];

    /// <summary>
    /// Maps item values to their panel element IDs so that
    /// <see cref="AccordionRoot.ToggleItemAsync"/> can animate panels
    /// that close implicitly (e.g. when a different item opens in
    /// single-selection mode).
    /// </summary>
    public Dictionary<string, string> PanelIdsByValue { get; } = new();

    private int _nextIndex;
    public int GetNextIndex() => _nextIndex++;
}
