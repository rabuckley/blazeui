using BlazeUI.Headless.Core;

namespace BlazeUI.Headless.Components.Accordion;

internal sealed class AccordionItemContext
{
    public string Value { get; set; } = "";
    public bool Open { get; set; }
    public bool Disabled { get; set; }
    public string TriggerId { get; set; } = "";
    public string PanelId { get; set; } = "";
    public int Index { get; set; }
    public Orientation Orientation { get; set; } = Orientation.Vertical;
}
