using BlazeUI.E2E.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class MenuTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task TriggerOpensMenu(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/menu", mode);

            var trigger = page.Locator("[data-testid='menu-default'] button[aria-haspopup='menu']");
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            await trigger.ClickAsync();

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
            await Expect(page.Locator("[role='menu']")).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task EscapeClosesMenu(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/menu", mode);

            var trigger = page.Locator("[data-testid='menu-default'] button[aria-haspopup='menu']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='menu']")).ToBeVisibleAsync();

            await page.Keyboard.PressAsync("Escape");

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickOutsideClosesMenu(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/menu", mode);

            var trigger = page.Locator("[data-testid='menu-default'] button[aria-haspopup='menu']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='menu']")).ToBeVisibleAsync();

            await page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ArrowKeyNavigatesItems(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/menu", mode);

            var trigger = page.Locator("[data-testid='menu-default'] button[aria-haspopup='menu']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='menu']")).ToBeVisibleAsync();

            // Pointer-opened menus focus the popup via rAF. Wait for focus to
            // land on the menu (or a child) before sending keyboard events.
            await page.WaitForFunctionAsync(
                """() => document.activeElement?.closest("[role='menu']") !== null""",
                null, new() { Timeout = 5_000 });

            // ArrowDown should focus the first item.
            await page.Keyboard.PressAsync("ArrowDown");

            await page.WaitForFunctionAsync(
                """() => document.activeElement?.getAttribute("role") === "menuitem" """,
                null, new() { Timeout = 5_000 });

            var firstText = await page.EvaluateAsync<string>("() => document.activeElement?.textContent?.trim()");
            Assert.Equal("Cut", firstText);

            // ArrowDown again moves to the next.
            await page.Keyboard.PressAsync("ArrowDown");
            await page.WaitForFunctionAsync(
                """() => document.activeElement?.textContent?.trim() === "Copy" """,
                null, new() { Timeout = 5_000 });
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task FocusReturnsOnClose(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/menu", mode);

            var trigger = page.Locator("[data-testid='menu-default'] button[aria-haspopup='menu']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='menu']")).ToBeVisibleAsync();

            await page.Keyboard.PressAsync("Escape");
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            await page.WaitForFunctionAsync(
                "() => document.activeElement?.getAttribute('aria-haspopup') === 'menu'",
                null,
                new() { Timeout = 5_000 });
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
