using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class CheckboxTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickTogglesChecked(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/checkbox", mode);

            var checkbox = page.Locator("[data-testid='checkbox-default'] [role='checkbox']");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
            await Expect(checkbox).ToHaveAttributeAsync("data-unchecked", "");

            await ClickViaJsAsync(checkbox);

            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
            await Expect(checkbox).ToHaveAttributeAsync("data-checked", "");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ClickTogglesBothDirections(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/checkbox", mode);

            var checkbox = page.Locator("[data-testid='checkbox-default'] [role='checkbox']");

            await ClickViaJsAsync(checkbox);
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");

            await ClickViaJsAsync(checkbox);
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
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
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/checkbox", mode);

            var section = page.Locator("[data-testid='checkbox-form']");
            var checkbox = section.Locator("[role='checkbox']");
            var hiddenInput = section.Locator("input[type='checkbox']");

            await Expect(hiddenInput).Not.ToBeCheckedAsync();

            await ClickViaJsAsync(checkbox);

            await Expect(hiddenInput).ToBeCheckedAsync();
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
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/checkbox", mode);

            var checkbox = page.Locator("[data-testid='checkbox-disabled'] [role='checkbox']");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
            await Expect(checkbox).ToHaveAttributeAsync("data-disabled", "");

            await ClickViaJsAsync(checkbox);

            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task DefaultCheckedInitialState(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/checkbox", mode);

            var checkbox = page.Locator("[data-testid='checkbox-default-checked'] [role='checkbox']");
            await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
            await Expect(checkbox).ToHaveAttributeAsync("data-checked", "");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
