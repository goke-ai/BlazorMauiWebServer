using Goke.Core.Interfaces;
using Goke.Core.Options;
using Goke.Core.Security;
using Goke.Core.Services;
using GokeWeb.Client.Pages;
using GokeWeb.Components;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// Add authentication and authorization services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

//
builder.Services.AddHttpContextAccessor();

// Add configuration for the backend API options
builder.Services.Configure<BackendApiOptions>(builder.Configuration.GetSection(BackendApiOptions.SectionName));
builder.Services.AddSingleton<BackendApiEndpoints>();

// Add httpclient service for API calls
builder.Services.AddHttpClient(BackendApiEndpoints.ClientName, (sp, client) => {
    var endpoint = sp.GetRequiredService<BackendApiEndpoints>();
    client.BaseAddress = endpoint.BaseUri ?? throw new InvalidOperationException("API base URL is not configured");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());

// Add httpclient service for API calls
builder.Services.AddHttpClient<AuthApiClient>((sp, client) => {
    var endpoint = sp.GetRequiredService<BackendApiEndpoints>();
    client.BaseAddress = endpoint.BaseUri ?? throw new InvalidOperationException("API base URL is not configured");
});

// Add other services
builder.Services.AddTransient<IFormFactor, GokeWeb.Services.FormFactorService>();


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
        typeof(GokeWebShared._Imports).Assembly,
        typeof(GokeShared._Imports).Assembly
      );

app.Run();
