using BlazeUI.E2E.Fixtures;

namespace BlazeUI.E2E.Components;

[Collection("E2E")]
public class SmokeTests(E2EFixture fixture)
{
    [Theory, MemberData(nameof(RenderModes.All), MemberType = typeof(RenderModes))]
    public async Task HostBecomesInteractive(RenderMode mode)
    {
        var page = await fixture.CreatePageAsync(mode);
        try
        {
            await E2EFixture.NavigateAndWaitForInteractiveAsync(page, "/", mode);

            var isInteractive = await page.EvaluateAsync<bool>("() => window.__blazeInteractive === true");
            Assert.True(isInteractive);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
