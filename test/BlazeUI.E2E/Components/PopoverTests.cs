using BlazeUI.E2E.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class PopoverTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task TriggerOpensPopover(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/popover", mode);

            var trigger = page.Locator("[data-testid='popover-default'] button[aria-expanded]");
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            await trigger.ClickAsync();

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task EscapeClosesPopover(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/popover", mode);

            var trigger = page.Locator("[data-testid='popover-default'] button[aria-expanded]");
            await trigger.ClickAsync();
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

            await page.Keyboard.PressAsync("Escape");

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickOutsideClosesPopover(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/popover", mode);

            var trigger = page.Locator("[data-testid='popover-default'] button[aria-expanded]");
            await trigger.ClickAsync();
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

            await page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
