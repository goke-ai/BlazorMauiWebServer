using Goke.Blazor.Components.Services;
using Goke.Core.Authentication;
using Goke.Core.Authorization;
using Goke.Core.Interfaces;
using Goke.Core.Map;
using Goke.Core.Options;
using Goke.Core.Services;
using GokeWebServer.Components;
using GokeWebServer.Components.Account;
using GokeWebServer.Data;
using GokeWebServer.Endpoints;
using GokeWebServer.Extensions.DependencyInjection;
using GokeWebServer.Hubs;
using GokeWebServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAccessTokenProvider, ServerAccessTokenProvider>();
builder.Services.AddScoped<ICookieProvider, ServerCookieProvider>();

builder.Services
    .AddAuthentication(builder.Configuration)
    .AddAuthorization(builder.Configuration);

builder.Services.Configure<BackendApiOptions>(builder.Configuration.GetSection(BackendApiOptions.SectionName));
builder.Services.AddSingleton<BackendApiEndpoints>();

// Add a CORS policy for the client
builder.Services.AddCors(
    options => options.AddDefaultPolicy(
        policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? ["https://localhost:7000", "https://localhost:7002"])
            .SetIsOriginAllowed(origin =>
            {
                // Allow any HTTPS origin under *.dev.localhost
                var uris = builder.Configuration.GetSection("Cors:AllowedOriginsForDevelopment")
                        .Get<string[]>() ?? ["https://gokewebserver.dev.localhost", "https://gokeweb.dev.localhost"];
                return uris.Any(uri => origin.StartsWith(uri));
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// Add a no-op email sender for development purposes
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add rate limiting to prevent brute force attacks on the Identity endpoints
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var bucket = httpContext.Request.Path.StartsWithSegments("/identity", StringComparison.OrdinalIgnoreCase)
            ? "identity"
            : "default";

        return RateLimitPartition.GetFixedWindowLimiter(
            bucket,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = bucket == "identity" ? 5 : int.MaxValue,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
    options.OnRejected = static (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

// +signalr
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});
// -signalr

// Add other services
builder.Services.AddTransient<IFormFactor, FormFactorService>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<RoleAdministrationService>();
builder.Services.AddScoped<AdminActivityLog>();
builder.Services.AddScoped<AdminStatusMessageStore>();
builder.Services.AddScoped<ILocationService, BrowserLocationService>();
builder.Services.AddSingleton<TrackerEngine>();
builder.Services.AddTransient<IUserConnectionManager, UserConnectionManager>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddSingleton<ChatDbContext>();


builder.Services.AddOpenApi();

var app = builder.Build();

// signalr hub requires response compression to be enabled
app.UseResponseCompression();

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

    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
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

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

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

// Add endpoints for the WeatherForecast service    
app.MapWeatherForecastEndpoints();

// Chat API endpoints
app.MapChatEndpoints();

// ChatHub endpoint for SignalR
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
