using Goke.Core.Interfaces;
using GokeWeb.Components;
using GokeWeb.Components.Account;
using GokeWeb.Extensions.DependencyInjection;
using GokeWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services
           .AddBackendApi(builder.Configuration)
           .AddAuthentication(builder.Configuration)
           .AddAuthorization(builder.Configuration);

// Add other services
builder.Services.AddTransient<IFormFactor, FormFactorService>();


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

app.Run();
