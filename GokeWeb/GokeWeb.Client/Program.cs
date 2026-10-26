using Goke.Aspnet.Core;
using Goke.Blazor.Components.Services;
using Goke.Core.Authentication;
using Goke.Core.Interfaces;
using Goke.Core.Map;
using Goke.Core.Options;
using Goke.Core.Services;
using GokeSharedCore.ChatApp;
using GokeSharedCore.MapApp;
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

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddSingleton<ChatDbContext>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddTransient<IUserService, UserApiClient>();
builder.Services.Configure<HubOptions>(builder.Configuration.GetSection(HubOptions.SectionName));
builder.Services.AddScoped<IAccessTokenProvider, ClientAccessTokenProvider>();
builder.Services.AddScoped<ICookieProvider, ClientCookieProvider>();
builder.Services.AddScoped<IHubClientFactory, HubClientFactory>();
builder.Services.AddScoped<ChatHubClientService>();


// Add other services
builder.Services.AddTransient<IFormFactor, ClientFormFactorService>();
builder.Services.AddScoped<ILocationService, BrowserLocationService>();
builder.Services.AddSingleton<TrackerEngine>();


await builder.Build().RunAsync();
