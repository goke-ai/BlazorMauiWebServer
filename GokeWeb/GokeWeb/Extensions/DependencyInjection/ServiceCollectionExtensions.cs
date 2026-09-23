using Goke.Core.Options;
using Goke.Core.Security;
using Goke.Core.Services;
using GokeWeb.Components.Account;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GokeWeb.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
    {        
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnlyPolicy", policy => policy.RequireRole("Administrators"))
            .AddPolicy("AdminCanDeletePolicy", policy => policy.RequireClaim("Permission", "Admin.CanDelete"))
            .AddPolicy("WeatherMapEditPolicy", policy => policy.RequireClaim("Permission", "WeatherMap.Edit"));

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCascadingAuthenticationState();

        services.AddHttpContextAccessor();

        var cookieOptions = configuration
            .GetSection(CookieAuthOptions.SectionName)
            .Get<CookieAuthOptions>() ?? new CookieAuthOptions();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = cookieOptions.LoginPath;
                options.AccessDeniedPath = cookieOptions.AccessDeniedPath;
                options.SlidingExpiration = cookieOptions.SlidingExpiration;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieOptions.ExpireTimeSpanMinutes);
            });

        return services;
    }

    public static IServiceCollection AddBackendApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackendApiOptions>(configuration.GetSection(BackendApiOptions.SectionName));
        services.AddSingleton<BackendApiEndpoints>();

        services.AddHttpClient(BackendApiEndpoints.ClientName, static (sp, client) =>
        {
            client.BaseAddress = sp.GetRequiredService<BackendApiEndpoints>().BaseUri;
        });

        services.AddHttpClient<AuthApiClient>(static (sp, client) =>
        {
            client.BaseAddress = sp.GetRequiredService<BackendApiEndpoints>().BaseUri;
        });

        services.AddScoped<IWebAccountService, WebAccountService>();

        return services;
    }
}