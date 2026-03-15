using BlazeUI.E2E.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class AccordionTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickExpandsPanel(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/accordion", mode);

            var section = page.Locator("[data-testid='accordion-default']");
            var triggers = section.Locator("button[aria-expanded]");

            // Initially all collapsed.
            await Expect(triggers.Nth(0)).ToHaveAttributeAsync("aria-expanded", "false");

            await triggers.Nth(0).ClickAsync();

            await Expect(triggers.Nth(0)).ToHaveAttributeAsync("aria-expanded", "true");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task SingleModeClosesPrevious(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/accordion", mode);

            var section = page.Locator("[data-testid='accordion-default']");
            var triggers = section.Locator("button[aria-expanded]");

            // Open first.
            await triggers.Nth(0).ClickAsync();
            await Expect(triggers.Nth(0)).ToHaveAttributeAsync("aria-expanded", "true");

            // Open second — first should close (single-mode default).
            await triggers.Nth(1).ClickAsync();
            await Expect(triggers.Nth(1)).ToHaveAttributeAsync("aria-expanded", "true");
            await Expect(triggers.Nth(0)).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
