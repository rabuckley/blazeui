using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class SwitchTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickTogglesSwitch(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/switch", mode);

            var sw = page.Locator("[data-testid='switch-default'] [role='switch']");
            await Expect(sw).ToHaveAttributeAsync("aria-checked", "false");
            await Expect(sw).ToHaveAttributeAsync("data-unchecked", "");

            await ClickViaJsAsync(sw);

            await Expect(sw).ToHaveAttributeAsync("aria-checked", "true");
            await Expect(sw).ToHaveAttributeAsync("data-checked", "");
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
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/switch", mode);

            var section = page.Locator("[data-testid='switch-form']");
            var sw = section.Locator("[role='switch']");
            var hiddenCheckbox = section.Locator("input[type='checkbox']");

            await Expect(hiddenCheckbox).Not.ToBeCheckedAsync();

            await ClickViaJsAsync(sw);

            await Expect(hiddenCheckbox).ToBeCheckedAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task UncheckedValueRendersHiddenInput(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/switch", mode);

            var section = page.Locator("[data-testid='switch-form']");

            // When unchecked, a hidden input with UncheckedValue should be present.
            var uncheckedInput = section.Locator("input[type='hidden'][value='no']");
            await Expect(uncheckedInput).ToHaveCountAsync(1);

            // Toggle on — the unchecked hidden input should disappear.
            await ClickViaJsAsync(section.Locator("[role='switch']"));
            await Expect(uncheckedInput).ToHaveCountAsync(0);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task DisabledPreventsToggle(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/switch", mode);

            var sw = page.Locator("[data-testid='switch-disabled'] [role='switch']");
            await Expect(sw).ToHaveAttributeAsync("aria-checked", "false");

            await ClickViaJsAsync(sw);

            await Expect(sw).ToHaveAttributeAsync("aria-checked", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
