using BlazeUI.E2E.Host.Wasm.Components;
using BlazeUI.Headless.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazeUI();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseStaticFiles();
app.UseAntiforgery();

// Echo endpoint for form submission E2E tests — returns posted form data as JSON
// so Playwright can verify hidden inputs participate in native form submission.
app.MapPost("/api/form-echo", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    return Results.Json(form.ToDictionary(k => k.Key, k => k.Value.ToString()));
}).DisableAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(BlazeUI.E2E.Host.Wasm.Client._Imports).Assembly,
        typeof(BlazeUI.E2E.Pages._Imports).Assembly);

app.Run();
