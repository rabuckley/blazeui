using BlazeUI.E2E.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

/// <summary>
/// Validates the InteractiveWebAssembly SSR→interactive handoff. The page is
/// non-interactive static HTML until the WASM runtime downloads and boots.
/// These tests verify components work correctly after the handoff.
/// </summary>
[Collection("E2E")]
public class SsrHandoffTests(E2EFixture fixture)
{
    [Fact]
    public async Task ComponentWorksAfterWasmHandoff()
    {
        var page = await fixture.CreatePageAsync(RenderMode.WebAssembly);
        try
        {
            // Navigate and wait for the full SSR→WASM handoff.
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/checkbox", RenderMode.WebAssembly);

            // Verify a component works after the handoff — the headless checkbox
            // requires the interactive runtime to handle click events.
            var checkbox = page.Locator("[data-testid='checkbox-default'] [role='checkbox']");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

            await checkbox.EvaluateAsync("el => el.click()");

            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task SsrRendersStaticHtmlBeforeInteractive()
    {
        var page = await fixture.CreatePageAsync(RenderMode.WebAssembly);
        try
        {
            // Navigate but DON'T wait for interactive. The SSR should have
            // already rendered the checkbox with correct ARIA attributes.
            var baseUrl = E2EFixture.BaseUrlFor(RenderMode.WebAssembly);
            await page.GotoAsync(
                $"{baseUrl}/checkbox",
                new() { WaitUntil = WaitUntilState.DOMContentLoaded });

            // Even during SSR, the checkbox should render with correct attributes.
            var checkbox = page.Locator("[data-testid='checkbox-default'] [role='checkbox']");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

            // Clicking during SSR should have no effect — the element has no event handlers.
            await checkbox.EvaluateAsync("el => el.click()");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

            // Now wait for interactive and verify it works.
            await page.WaitForFunctionAsync(
                "() => window.__blazeInteractive === true",
                null, new() { Timeout = 30_000 });

            await checkbox.EvaluateAsync("el => el.click()");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
