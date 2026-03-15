using System.Text.Json;
using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

/// <summary>
/// Verifies that hidden inputs rendered by form-participating components
/// (Checkbox, Radio, Switch, Select) are actually included in native form
/// submissions — the full HTTP POST round-trip that bUnit can't cover.
/// </summary>
[Collection("E2E")]
public class FormSubmissionTests(E2EFixture fixture)
{
    /// <summary>
    /// Before the interactive runtime boots, SSR-rendered hidden inputs should
    /// already participate in the browser's FormData collection. This validates
    /// the SSR codepath independently of any JS initialization.
    /// </summary>
    [Fact]
    public async Task SsrRenderedFormDataIncludesDefaults()
    {
        var page = await fixture.CreatePageAsync(RenderMode.Server);
        try
        {
            // Block Blazor's interactive JS so the DOM stays in pure SSR state.
            // Without this, the interactive handoff can re-render the page between
            // DOMContentLoaded and our FormData evaluation.
            await page.RouteAsync("**/_framework/blazor.web.js", route => route.AbortAsync());

            var baseUrl = E2EFixture.BaseUrlFor(RenderMode.Server);
            await page.GotoAsync(
                $"{baseUrl}/form-submission",
                new() { WaitUntil = WaitUntilState.DOMContentLoaded });

            // Wait for the form to be present in the DOM before evaluating FormData.
            await page.WaitForSelectorAsync("[data-testid='form-submission']");

            // Use the FormData API to collect what the browser would POST.
            // Return as JSON string and parse — EvaluateAsync<Dictionary> can lose
            // entries due to Playwright's internal serialization.
            var entries = await CollectFormDataAsync(page);

            Assert.Equal("yes", entries["agree"]);
            Assert.Equal("red", entries["color"]);
            // Switch is unchecked by default — UncheckedValue="off" provides the value.
            Assert.Equal("off", entries["notifications"]);
            Assert.Equal("banana", entries["fruit"]);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Full round-trip: submit the form via native POST and verify the echo
    /// endpoint received the correct default values.
    /// </summary>
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task NativePostCarriesDefaultValues(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/form-submission", mode);

            var formData = await SubmitAndReadEchoAsync(page);

            Assert.Equal("yes", formData["agree"]);
            Assert.Equal("red", formData["color"]);
            Assert.Equal("off", formData["notifications"]);
            Assert.Equal("banana", formData["fruit"]);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Interact with components (toggle switch, change radio, change select),
    /// then submit — the POST should reflect the interactive state, not defaults.
    /// </summary>
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task NativePostReflectsInteractiveChanges(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/form-submission", mode);

            // Toggle switch ON (was off).
            var sw = page.Locator("[data-testid='form-switch'] [role='switch']");
            await ClickViaJsAsync(sw);

            // Change radio from red → blue.
            var blueRadio = page.Locator("[data-testid='form-radio'] [role='radio']").Nth(2);
            await ClickViaJsAsync(blueRadio);

            // Change select from banana → cherry.
            var trigger = page.Locator("[data-testid='form-select'] button[role='combobox']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='listbox']").First).ToBeVisibleAsync();
            await page.Locator("[role='option'][data-value='cherry']").First.ClickAsync();

            var formData = await SubmitAndReadEchoAsync(page);

            Assert.Equal("yes", formData["agree"]);
            Assert.Equal("blue", formData["color"]);
            Assert.Equal("on", formData["notifications"]);
            Assert.Equal("cherry", formData["fruit"]);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Reads all entries from the form's FormData via JS and returns them as a dictionary.
    /// Uses JSON.stringify → C# parse to avoid Playwright's EvaluateAsync&lt;T&gt;
    /// losing entries during its internal CDP serialization.
    /// </summary>
    private static async Task<Dictionary<string, string>> CollectFormDataAsync(IPage page)
    {
        var json = await page.EvaluateAsync<string>("""
            () => {
                const form = document.querySelector("[data-testid='form-submission']");
                const fd = new FormData(form);
                const result = {};
                for (const [key, value] of fd.entries()) {
                    result[key] = value;
                }
                return JSON.stringify(result);
            }
            """);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? throw new InvalidOperationException("FormData JSON deserialized to null.");
    }

    /// <summary>
    /// Submits the form and reads the JSON echo response.
    /// </summary>
    private static async Task<Dictionary<string, string>> SubmitAndReadEchoAsync(IPage page)
    {
        var responseTask = page.WaitForResponseAsync(
            r => r.Url.Contains("/api/form-echo") && r.Request.Method == "POST");

        await page.ClickAsync("[data-testid='submit-btn']");

        var response = await responseTask;
        var body = await response.TextAsync();
        return JsonSerializer.Deserialize<Dictionary<string, string>>(body)
            ?? throw new InvalidOperationException("Echo endpoint returned null.");
    }
}
