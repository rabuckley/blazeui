namespace BlazeUI.Headless.Core;

/// <summary>
/// Generates unique, sequential element IDs for component instances.
/// </summary>
internal static class IdGenerator
{
    private static int _counter;

    /// <summary>
    /// Returns a unique ID in the form <c>blazeui-{prefix}-{n}</c>.
    /// </summary>
    // TODO: Hydration-safe deterministic IDs for SSR/streaming scenarios.
    public static string Next(string prefix)
    {
        var id = Interlocked.Increment(ref _counter);
        return $"blazeui-{prefix}-{id}";
    }
}
