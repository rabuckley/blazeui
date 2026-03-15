using BlazeUI.E2E.Fixtures;
using static BlazeUI.E2E.Helpers.HeadlessHelpers;
using static Microsoft.Playwright.Assertions;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class SliderTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task ArrowKeyChangesValue(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/slider", mode);

            var rangeInput = page.Locator("[data-testid='slider-default'] input[type='range']");
            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "50");

            await FocusViaJsAsync(rangeInput);
            await page.Keyboard.PressAsync("ArrowRight");

            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "51");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task HomeGoesToMin(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/slider", mode);

            var rangeInput = page.Locator("[data-testid='slider-default'] input[type='range']");
            await FocusViaJsAsync(rangeInput);
            await page.Keyboard.PressAsync("Home");

            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "0");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task EndGoesToMax(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/slider", mode);

            var rangeInput = page.Locator("[data-testid='slider-default'] input[type='range']");
            await FocusViaJsAsync(rangeInput);
            await page.Keyboard.PressAsync("End");

            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "100");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task CustomStepIncrements(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/slider", mode);

            var rangeInput = page.Locator("[data-testid='slider-custom-step'] input[type='range']");
            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "0");

            await FocusViaJsAsync(rangeInput);
            await page.Keyboard.PressAsync("ArrowRight");

            // Step is 2, so 0 + 2 = 2.
            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "2");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task DisabledSliderHasCorrectAttributes(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/slider", mode);

            var section = page.Locator("[data-testid='slider-disabled']");
            var rangeInput = section.Locator("input[type='range']");

            await Expect(rangeInput).ToHaveAttributeAsync("aria-valuenow", "50");
            await Expect(rangeInput).ToHaveAttributeAsync("disabled", "");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task InitialValueRenderedCorrectly(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/slider", mode);

            var defaultInput = page.Locator("[data-testid='slider-default'] input[type='range']");
            await Expect(defaultInput).ToHaveAttributeAsync("aria-valuenow", "50");
            await Expect(defaultInput).ToHaveAttributeAsync("aria-valuemin", "0");
            await Expect(defaultInput).ToHaveAttributeAsync("aria-valuemax", "100");

            var customInput = page.Locator("[data-testid='slider-custom-step'] input[type='range']");
            await Expect(customInput).ToHaveAttributeAsync("aria-valuenow", "0");
            await Expect(customInput).ToHaveAttributeAsync("min", "0");
            await Expect(customInput).ToHaveAttributeAsync("max", "10");
            await Expect(customInput).ToHaveAttributeAsync("step", "2");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
