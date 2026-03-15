namespace BlazeUI.Headless.Core;

/// <summary>
/// Converts <see cref="Side"/> and <see cref="Align"/> to Floating UI placement strings.
/// </summary>
internal static class PlacementHelper
{
    public static string ToPlacement(Side side, Align align)
    {
        var s = side switch
        {
            Side.Top => "top",
            Side.Bottom => "bottom",
            Side.Left => "left",
            Side.Right => "right",
            _ => "bottom"
        };

        return align switch
        {
            Align.Start => $"{s}-start",
            Align.End => $"{s}-end",
            _ => s
        };
    }

    public static string ToDataSide(string placement)
    {
        if (placement.StartsWith("top")) return "top";
        if (placement.StartsWith("bottom")) return "bottom";
        if (placement.StartsWith("left")) return "left";
        if (placement.StartsWith("right")) return "right";
        return "bottom";
    }

    public static string ToDataAlign(string placement)
    {
        if (placement.EndsWith("-start")) return "start";
        if (placement.EndsWith("-end")) return "end";
        return "center";
    }
}
