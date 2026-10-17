using Goke.Core.Interfaces;
using Goke.Core.Map;
using Goke.Core.Options;
using Goke.Core.Services;
using GokeShared.Services;
using GokeWeb.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.Configure<BackendApiOptions>(builder.Configuration.GetSection(BackendApiOptions.SectionName));
builder.Services.AddSingleton<BackendApiEndpoints>();

builder.Services.AddHttpClient(BackendApiEndpoints.ClientName, static (sp, client) =>
{
    client.BaseAddress = sp.GetRequiredService<BackendApiEndpoints>().BaseUri;
});

// Add other services
builder.Services.AddTransient<IFormFactor, ClientFormFactorService>();
builder.Services.AddScoped<ILocationService, BrowserLocationService>();
builder.Services.AddSingleton<TrackerEngine>();


await builder.Build().RunAsync();
