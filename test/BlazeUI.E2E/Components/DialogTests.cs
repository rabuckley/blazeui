using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class DialogTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task TriggerOpensDialog(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/dialog", mode);

            var trigger = page.Locator("[data-testid='dialog-default'] button[aria-haspopup='dialog']");
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            await trigger.ClickAsync();

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
            await Expect(page.Locator("[role='dialog']")).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task EscapeClosesDialog(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/dialog", mode);

            var trigger = page.Locator("[data-testid='dialog-default'] button[aria-haspopup='dialog']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='dialog']")).ToBeVisibleAsync();

            await page.Keyboard.PressAsync("Escape");

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task CloseButtonClosesDialog(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/dialog", mode);

            var trigger = page.Locator("[data-testid='dialog-default'] button[aria-haspopup='dialog']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='dialog']")).ToBeVisibleAsync();

            // The inert overlay intercepts pointer events, so use JS click.
            var dialog = page.Locator("[role='dialog']");
            await ClickViaJsAsync(dialog.Locator("button", new() { HasText = "Close" }));

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task FocusReturnsToTriggerOnClose(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/dialog", mode);

            var trigger = page.Locator("[data-testid='dialog-default'] button[aria-haspopup='dialog']");
            await trigger.ClickAsync();
            await Expect(page.Locator("[role='dialog']")).ToBeVisibleAsync();

            await page.Keyboard.PressAsync("Escape");
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            // Focus should return to the trigger.
            var focusedHaspopup = await page.EvaluateAsync<string?>(
                "() => document.activeElement?.getAttribute('aria-haspopup')");
            Assert.Equal("dialog", focusedHaspopup);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory(Skip = "Known issue: DialogTitle ID not wired to aria-labelledby when rendered through Portal")]
    [MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task DialogHasAriaLabelledby(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/dialog", mode);

            var trigger = page.Locator("[data-testid='dialog-default'] button[aria-haspopup='dialog']");
            await trigger.ClickAsync();

            var dialog = page.Locator("[role='dialog']");
            await Expect(dialog).ToBeVisibleAsync();

            // aria-labelledby should point to the title element.
            var labelledBy = await dialog.GetAttributeAsync("aria-labelledby");
            Assert.NotNull(labelledBy);

            var title = page.Locator($"#{labelledBy}");
            await Expect(title).ToHaveTextAsync("Test Dialog");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task DialogIsModal(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/dialog", mode);

            var trigger = page.Locator("[data-testid='dialog-default'] button[aria-haspopup='dialog']");
            await trigger.ClickAsync();

            var dialog = page.Locator("[role='dialog']");
            await Expect(dialog).ToHaveAttributeAsync("aria-modal", "true");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
