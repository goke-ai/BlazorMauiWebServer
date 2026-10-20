using Goke.Blazor.Components.Services;
using Goke.Core.Authorization;
using Goke.Core.Interfaces;
using Goke.Core.Map;
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

// Add a CORS policy for the client
builder.Services.AddCors(
    options => options.AddDefaultPolicy(
        policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? ["https://localhost:7000", "https://localhost:7002"])
            .SetIsOriginAllowed(origin =>
            {
                // Allow any HTTPS origin under *.dev.localhost
                var uris = builder.Configuration.GetSection("Cors:AllowedOriginsForDevelopment")
                        .Get<string[]>() ?? ["https://gokeweb.dev.localhost"];
                return uris.Any(uri => origin.StartsWith(uri));
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add other services
builder.Services.AddTransient<IFormFactor, FormFactorService>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<RoleAdministrationService>();
builder.Services.AddScoped<AdminActivityLog>();
builder.Services.AddScoped<AdminStatusMessageStore>();
builder.Services.AddScoped<ILocationService, BrowserLocationService>();
builder.Services.AddSingleton<TrackerEngine>();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Apply migrations & create database if needed at startup
    using (var scope = app.Services.CreateScope())
    {
        await ApplicationSeeder.SeedAllAsync(scope.ServiceProvider, deleteDatabase: true);
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

// Activate the CORS policy
app.UseCors();

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
