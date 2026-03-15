namespace BlazeUI.Headless.Core;

/// <summary>
/// Utility for combining CSS class names, filtering out nulls and empty strings.
/// </summary>
public static class Css
{
    /// <summary>
    /// Combines class names, ignoring null and whitespace-only values.
    /// </summary>
    public static string Cn(params string?[] classes)
    {
        return string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    /// <summary>
    /// Combines conditional class names. Each tuple pairs a class name with
    /// a condition — the class is only included when the condition is <c>true</c>.
    /// </summary>
    public static string Cn(params (string? className, bool condition)[] classes)
    {
        return string.Join(' ', classes
            .Where(c => c.condition && !string.IsNullOrWhiteSpace(c.className))
            .Select(c => c.className));
    }
}
