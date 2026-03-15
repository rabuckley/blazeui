using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class RadioTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickSelectsRadio(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/radio", mode);

            var section = page.Locator("[data-testid='radio-default']");
            var radios = section.Locator("[role='radio']");

            // Default value is "banana".
            await Expect(radios.Nth(0)).ToHaveAttributeAsync("aria-checked", "false");
            await Expect(radios.Nth(1)).ToHaveAttributeAsync("aria-checked", "true");
            await Expect(radios.Nth(2)).ToHaveAttributeAsync("aria-checked", "false");

            // Click apple.
            await ClickViaJsAsync(radios.Nth(0));

            await Expect(radios.Nth(0)).ToHaveAttributeAsync("aria-checked", "true");
            await Expect(radios.Nth(1)).ToHaveAttributeAsync("aria-checked", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ArrowKeyMovesSelection(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/radio", mode);

            var section = page.Locator("[data-testid='radio-default']");
            var radios = section.Locator("[role='radio']");

            // "banana" is checked and has tabindex="0" (roving focus).
            await Expect(radios.Nth(1)).ToHaveAttributeAsync("tabindex", "0");

            // Focus the checked radio and press ArrowDown to move to cherry.
            await FocusViaJsAsync(radios.Nth(1));
            await page.Keyboard.PressAsync("ArrowDown");

            // Arrow key activates on focus — cherry should now be checked.
            await Expect(radios.Nth(2)).ToHaveAttributeAsync("aria-checked", "true");
            await Expect(radios.Nth(1)).ToHaveAttributeAsync("aria-checked", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task HiddenInputSyncs(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/radio", mode);

            var section = page.Locator("[data-testid='radio-form']");

            // Default is "apple" — its hidden radio should be checked.
            var appleInput = section.Locator("input[type='radio'][value='apple']");
            var bananaInput = section.Locator("input[type='radio'][value='banana']");

            await Expect(appleInput).ToBeCheckedAsync();
            await Expect(bananaInput).Not.ToBeCheckedAsync();

            // Click banana radio.
            await ClickViaJsAsync(section.Locator("[role='radio']").Nth(1));

            await Expect(bananaInput).ToBeCheckedAsync();
            await Expect(appleInput).Not.ToBeCheckedAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task HiddenInputsShareFormName(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/radio", mode);

            var section = page.Locator("[data-testid='radio-form']");
            var radioInputs = section.Locator("input[type='radio']");

            // All hidden radio inputs should share the group's name.
            var count = await radioInputs.CountAsync();
            for (var i = 0; i < count; i++)
            {
                await Expect(radioInputs.Nth(i)).ToHaveAttributeAsync("name", "fruit");
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
