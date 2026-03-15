using BlazeStore.Api;
using BlazeStore.Client.Services;
using BlazeStore.Components;
using BlazeStore.Data;
using BlazeStore.Services;
using BlazeUI.Headless.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazeUI();

// Data layer — in-memory stores acting as a pretend database.
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IProductService>(sp => sp.GetRequiredService<ProductRepository>());
builder.Services.AddSingleton<CartRepository>();

// Stub ICartService for SSR prerendering — WASM islands fetch real data after hydration.
builder.Services.AddScoped<ICartService, SsrCartService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();

// API endpoints for WASM islands to call.
app.MapProductApi();
app.MapCartApi();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazeStore.Client._Imports).Assembly);

app.Run();
