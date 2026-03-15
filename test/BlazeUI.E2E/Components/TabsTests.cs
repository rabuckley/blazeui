using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class TabsTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickActivatesTab(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/tabs", mode);

            var section = page.Locator("[data-testid='tabs-default']");
            var tabs = section.Locator("[role='tab']");

            // First tab is selected by default.
            await Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-selected", "true");
            await Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "false");

            // Click second tab.
            await ClickViaJsAsync(tabs.Nth(1));

            await Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
            await Expect(tabs.Nth(0)).ToHaveAttributeAsync("aria-selected", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task PanelContentSwitches(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/tabs", mode);

            var section = page.Locator("[data-testid='tabs-default']");
            var tabs = section.Locator("[role='tab']");
            var panels = section.Locator("[role='tabpanel']");

            // First panel visible.
            await Expect(panels.First).ToContainTextAsync("Content for tab 1");

            // Click tab 2, panel 2 should appear.
            await ClickViaJsAsync(tabs.Nth(1));
            await Expect(panels.First).ToContainTextAsync("Content for tab 2");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
