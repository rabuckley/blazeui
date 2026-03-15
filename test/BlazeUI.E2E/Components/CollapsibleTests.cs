using BlazeUI.E2E.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class CollapsibleTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickTogglesContent(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/collapsible", mode);

            var section = page.Locator("[data-testid='collapsible-default']");
            var trigger = section.Locator("button[aria-expanded]");

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            await trigger.ClickAsync();
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

            await trigger.ClickAsync();
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
