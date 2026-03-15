namespace BlazeUI.Sonner;

internal static class PositionExtensions
{
    /// <summary>
    /// Splits a <see cref="Position"/> into its Y and X data-attribute values
    /// (e.g. <c>BottomRight</c> → <c>("bottom", "right")</c>).
    /// </summary>
    internal static (string Y, string X) ToDataValues(this Position position)
    {
        return position switch
        {
            Position.TopLeft => ("top", "left"),
            Position.TopCenter => ("top", "center"),
            Position.TopRight => ("top", "right"),
            Position.BottomLeft => ("bottom", "left"),
            Position.BottomCenter => ("bottom", "center"),
            Position.BottomRight => ("bottom", "right"),
            _ => ("bottom", "right"),
        };
    }
}
