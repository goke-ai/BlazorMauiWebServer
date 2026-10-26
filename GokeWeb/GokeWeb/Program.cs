using Goke.Aspnet.Core;
using Goke.Blazor.Components.Services;
using Goke.Core.Authentication;
using Goke.Core.Interfaces;
using Goke.Core.Map;
using Goke.Core.Options;
using GokeSharedCore.ChatApp;
using GokeSharedCore.MapApp;
using GokeWeb.Client.Services;
using GokeWeb.Components;
using GokeWeb.Components.Account;
using GokeWeb.Endpoints;
using GokeWeb.Extensions.DependencyInjection;
using GokeWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


builder.Services
           .AddBackendApi(builder.Configuration)
           .AddAuthentication(builder.Configuration)
           .AddAuthorization(builder.Configuration);

//
builder.Services.AddSingleton<ChatDbContext>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUserService, UserApiClient>();
builder.Services.Configure<HubOptions>(builder.Configuration.GetSection(HubOptions.SectionName));
builder.Services.AddScoped<IAccessTokenProvider, ServerAccessTokenProvider>();
builder.Services.AddScoped<ICookieProvider, ServerCookieProvider>();
builder.Services.AddScoped<IHubClientFactory, HubClientFactory>();
builder.Services.AddScoped<ChatHubClientService>();


// Add other services
builder.Services.AddTransient<IFormFactor, FormFactorService>();
builder.Services.AddScoped<ILocationService, BrowserLocationService>();
builder.Services.AddSingleton<TrackerEngine>();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(GokeWeb.Client._Imports).Assembly,
        typeof(GokeShared._Imports).Assembly
      );

// Needed to log in to the backend WebServer
app.MapAccountEndpoints();
// 
app.MapApiEndpoints();

app.Run();
