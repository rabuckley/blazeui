namespace BlazeUI.E2E.Helpers;

/// <summary>
/// Helpers for interacting with headless BlazeUI components that have no intrinsic
/// CSS dimensions. Playwright considers zero-size elements invisible/outside-viewport,
/// so standard click/focus methods fail. These use JS to bypass visibility checks.
/// </summary>
internal static class HeadlessHelpers
{
    /// <summary>
    /// Clicks a headless element via JS <c>element.click()</c>, bypassing Playwright's
    /// visibility and viewport checks.
    /// </summary>
    internal static async Task ClickViaJsAsync(ILocator locator)
    {
        await locator.EvaluateAsync("el => el.click()");
    }

    /// <summary>
    /// Focuses a headless element via JS <c>element.focus()</c>, bypassing Playwright's
    /// visibility checks.
    /// </summary>
    internal static async Task FocusViaJsAsync(ILocator locator)
    {
        await locator.EvaluateAsync("el => el.focus()");
    }
}
