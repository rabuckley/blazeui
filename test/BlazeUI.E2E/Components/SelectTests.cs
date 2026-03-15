using BlazeUI.E2E.Fixtures;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class SelectTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickTriggerOpensPopup(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            await trigger.ClickAsync();

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
            await Expect(page.Locator("[role='listbox']").First).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task EscapeClosesPopup(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await trigger.ClickAsync();
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

            await page.Keyboard.PressAsync("Escape");

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            // Focus should return to trigger after close.
            var focusedId = await page.EvaluateAsync<string>("() => document.activeElement?.getAttribute('role')");
            Assert.Equal("combobox", focusedId);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickOutsideClosesPopup(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await trigger.ClickAsync();
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

            // Click outside the popup.
            await page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ArrowKeyNavigation(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await trigger.ClickAsync();
            await Expect(page.Locator("[role='listbox']").First).ToBeVisibleAsync();

            // The popup auto-focuses the first enabled item after open.
            await WaitForFocusedOptionAsync(page, "apple");

            // ArrowDown moves to the next item.
            await page.Keyboard.PressAsync("ArrowDown");
            await WaitForFocusedOptionAsync(page, "banana");

            // ArrowUp goes back.
            await page.Keyboard.PressAsync("ArrowUp");
            await WaitForFocusedOptionAsync(page, "apple");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task EnterSelectsHighlightedItem(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await trigger.ClickAsync();

            // The popup auto-focuses the first enabled item (apple).
            // One ArrowDown moves to banana.
            await WaitForFocusedOptionAsync(page, "apple");
            await page.Keyboard.PressAsync("ArrowDown");
            await WaitForFocusedOptionAsync(page, "banana");
            await page.Keyboard.PressAsync("Enter");

            // Popup should close.
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            // Hidden input should have the selected value.
            var hiddenInput = section.Locator("input[aria-hidden='true']");
            await Expect(hiddenInput).ToHaveValueAsync("banana");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickSelectsItem(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await trigger.ClickAsync();

            var appleItem = page.Locator("[role='option'][data-value='apple']").First;
            await appleItem.ClickAsync();

            // Popup should close.
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

            // Hidden input should reflect the selection.
            var hiddenInput = section.Locator("input[aria-hidden='true']");
            await Expect(hiddenInput).ToHaveValueAsync("apple");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task DisabledItemSkipped(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            await trigger.ClickAsync();

            // Auto-focus lands on apple. ArrowDown twice: apple → banana → (skip cherry, disabled) → date.
            await WaitForFocusedOptionAsync(page, "apple");
            await page.Keyboard.PressAsync("ArrowDown");
            await page.Keyboard.PressAsync("ArrowDown");

            await WaitForFocusedOptionAsync(page, "date");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task HiddenInputSyncsWithSelection(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");
            var hiddenInput = section.Locator("input[aria-hidden='true']");

            // Initially empty.
            await Expect(hiddenInput).ToHaveValueAsync("");

            // Select apple.
            await trigger.ClickAsync();
            await page.Locator("[role='option'][data-value='apple']").First.ClickAsync();
            await Expect(hiddenInput).ToHaveValueAsync("apple");

            // Change to banana.
            await trigger.ClickAsync();
            await page.Locator("[role='option'][data-value='banana']").First.ClickAsync();
            await Expect(hiddenInput).ToHaveValueAsync("banana");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task HiddenInputHasFormName(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-form']");
            var hiddenInput = section.Locator("input[aria-hidden='true']");

            await Expect(hiddenInput).ToHaveAttributeAsync("name", "fruit");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ControlledValueInitialState(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-controlled']");
            var boundSpan = section.Locator("[data-testid='controlled-value']");

            // Controlled value starts as "banana".
            await Expect(boundSpan).ToHaveTextAsync("banana");

            // Select a different item.
            var trigger = section.Locator("button[role='combobox']");
            await trigger.ClickAsync();
            await page.Locator("[role='option'][data-value='apple']").Last.ClickAsync();

            // Bound value updates.
            await Expect(boundSpan).ToHaveTextAsync("apple");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task PlaceholderShownWhenNoSelection(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/select", mode);

            var section = page.Locator("[data-testid='select-default']");
            var trigger = section.Locator("button[role='combobox']");

            // Before any selection, trigger should show placeholder.
            await Expect(trigger.Locator("[data-placeholder]")).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Waits until the currently focused element is an option with the given data-value.
    /// Keyboard navigation uses native focus, not data-highlighted.
    /// </summary>
    private static async Task WaitForFocusedOptionAsync(IPage page, string expectedValue)
    {
        await page.WaitForFunctionAsync(
            $$"""(expected) => document.activeElement?.getAttribute("data-value") === expected""",
            expectedValue,
            new() { Timeout = 5_000 });
    }
}
