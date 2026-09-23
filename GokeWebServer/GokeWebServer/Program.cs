using Goke.Core.Authorization;
using Goke.Core.Interfaces;
using GokeWebServer.Components;
using GokeWebServer.Components.Account;
using GokeWebServer.Data;
using GokeWebServer.Endpoints;
using GokeWebServer.Extensions.DependencyInjection;
using GokeWebServer.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services
    .AddAuthentication(builder.Configuration)
    .AddAuthorization(builder.Configuration);


builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add other services
builder.Services.AddTransient<IFormFactor, FormFactorService>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<RoleAdministrationService>();
builder.Services.AddScoped<AdminActivityLog>();
builder.Services.AddScoped<AdminStatusMessageStore>();  


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Apply migrations & create database if needed at startup
    using (var scope = app.Services.CreateScope())
    {
        await ApplicationSeeder.SeedAllAsync(scope.ServiceProvider);
    }

    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(GokeWebServer.Client._Imports).Assembly,
        typeof(GokeShared._Imports).Assembly
    );

// Needed for external clients to log in
app.MapIdentityEndpoints();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapWeatherForecastEndpoints();  

app.Run();
