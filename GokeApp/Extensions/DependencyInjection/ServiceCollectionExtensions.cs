using Goke.Core.Authorization;
using Goke.Core.Options;
using Goke.Core.Security;
using Goke.Core.Services;
using Goke.Maui.Core.Services;
using Microsoft.Extensions.Configuration;

namespace GokeApp.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnlyPolicy", policy => policy.RequireRole("Administrators"));
            options.AddPolicy("DepartmentITPolicy", policy => policy.RequireClaim("Department", "IT"));
            options.AddPolicy("ProfileEditPolicy", policy => policy.RequirePermission("Profile.Edit"));
        });

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAuthTokenStore, TokenStorage>();
        services.AddSingleton<IAccessTokenManager, AccessTokenManager>();
        services.AddSingleton<IAuthenticatedUserPrincipalFactory, AuthenticatedUserPrincipalFactory>();

        services.AddSingleton<MauiAuthenticationStateProvider>();
        services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<MauiAuthenticationStateProvider>());

        return services;
    }

    public static IServiceCollection AddBackendApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackendApiOptions>(configuration.GetSection(BackendApiOptions.SectionName));
        services.AddSingleton<IBackendApiBaseUrlResolver, BackendApiBaseUrlResolver>();
        services.AddSingleton<BackendApiEndpoints>();

        services.AddHttpClient(BackendApiEndpoints.ClientName, static (sp, client) =>
        {
            client.BaseAddress = sp.GetRequiredService<BackendApiEndpoints>().BaseUri;
        })
        .ConfigurePrimaryHttpMessageHandler(HttpClientHelper.CreatePlatformMessageHandler);

        services.AddHttpClient<AuthApiClient>(static (sp, client) =>
        {
            client.BaseAddress = sp.GetRequiredService<BackendApiEndpoints>().BaseUri;
        })
        .ConfigurePrimaryHttpMessageHandler(HttpClientHelper.CreatePlatformMessageHandler);

        return services;
    }
}