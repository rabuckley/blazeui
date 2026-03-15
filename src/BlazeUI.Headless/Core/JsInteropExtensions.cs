namespace BlazeUI.Headless.Core;

/// <summary>
/// Extension methods for JS interop URL formatting.
/// </summary>
internal static class JsInteropExtensions
{
    /// <summary>
    /// Appends a cache-busting <c>?v={suffix}</c> query string to a static content URL
    /// when <see cref="BlazeUIConfiguration.JsVersionSuffix"/> is set.
    /// </summary>
    public static string FormatUrl(this string path, BlazeUIConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration.JsVersionSuffix))
        {
            return path;
        }

        return $"{path}?v={configuration.JsVersionSuffix}";
    }
}
