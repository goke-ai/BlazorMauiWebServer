using Goke.Core.Interfaces;
using Goke.Core.Options;
using Goke.Core.Services;
using GokeWebServer.Client.Services;
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


await builder.Build().RunAsync();
