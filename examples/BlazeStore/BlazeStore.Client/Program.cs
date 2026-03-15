using BlazeStore.Client.Services;
using BlazeUI.Headless.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazeUI();

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IProductService, ApiProductService>();
builder.Services.AddScoped<ICartService, ApiCartService>();

await builder.Build().RunAsync();
