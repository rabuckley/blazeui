using BlazeUI.Headless.Core;
using Microsoft.JSInterop;

namespace BlazeUI.Headless.Components.Slider;

internal sealed class SliderContext
{
    // The current slider value(s). A single-value slider stores one element; a range
    // slider stores two or more. We keep this as a flat array throughout to avoid
    // branching on "is this a range?" everywhere in child components.
    public double[] Values { get; set; } = [];

    public double Min { get; set; }
    public double Max { get; set; } = 100;
    public double Step { get; set; } = 1;
    public double LargeStep { get; set; } = 10;
    public Orientation Orientation { get; set; }
    public bool Disabled { get; set; }
    public bool Dragging { get; set; }

    // Element IDs assigned by the Root and used by children to wire ARIA relationships.
    // Each thumb gets its own input id for the <output htmlFor> chain on SliderValue.
    public string RootId { get; set; } = "";
    public string ControlId { get; set; } = "";

    // The id of the SliderLabel element, set by the label component after mount so the
    // Root can add aria-labelledby. Because label registration happens post-render the
    // Root derives a stable default from its own id via GetDefaultLabelId().
    public string? LabelId { get; set; }

    // The stable label id derived from the root id, used as the default label association
    // before a SliderLabel component registers itself. Corresponds to Base UI's
    // rootLabelId / getDefaultLabelId(id).
    public string RootLabelId { get; set; } = "";

    // Per-thumb input ids, populated by each SliderThumb as it renders. SliderValue uses
    // these to build its htmlFor attribute.
    public List<string> ThumbInputIds { get; } = [];

    public Func<double, int, Task> SetValue { get; set; } = (_, _) => Task.CompletedTask;
    public IJSObjectReference? JsModule { get; set; }
    public DotNetObjectReference<SliderRoot>? DotNetRef { get; set; }

    // The measured thumb size in pixels, reported by JS after layout. SliderThumb
    // includes this in its inline style so --slider-thumb-size survives Blazor re-renders.
    public double ThumbSize { get; set; }

    // Convenience: percentage of Values[0] in [0, 1] for single-thumb styling.
    public double Percent => Max > Min ? (Values.Length > 0 ? (Values[0] - Min) / (Max - Min) : 0) : 0;

    // Whether this is a range slider (two or more thumbs).
    public bool IsRange => Values.Length > 1;
}
