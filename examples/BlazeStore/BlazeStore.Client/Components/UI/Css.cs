namespace BlazeStore.Client.Components.UI;

using TailwindMerge;

/// <summary>
/// Utility for combining CSS class names with Tailwind-aware conflict resolution.
/// Mirrors shadcn/ui's <c>cn()</c> helper: classes are joined then passed through
/// tailwind-merge so the last conflicting utility wins.
/// </summary>
internal static class Css
{
    private static readonly TwMerge Merger = new(null);

    /// <summary>
    /// Combines class names with Tailwind-merge conflict resolution.
    /// </summary>
    internal static string Cn(params string?[] classes)
    {
        return Merger.Merge(classes) ?? string.Empty;
    }

    /// <summary>
    /// Combines conditional class names. Each tuple pairs a class name with
    /// a condition — the class is only included when the condition is <c>true</c>.
    /// </summary>
    internal static string Cn(params (string? className, bool condition)[] classes)
    {
        var filtered = classes
            .Where(c => c.condition && !string.IsNullOrWhiteSpace(c.className))
            .Select(c => c.className)
            .ToArray();

        return Merger.Merge(filtered) ?? string.Empty;
    }
}
