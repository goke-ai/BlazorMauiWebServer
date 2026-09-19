using Goke.Core.Options;
using Goke.Core.Security;
using Goke.Core.Services;
using Goke.Maui.Core.Services;
using GokeHyb.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

namespace GokeHyb.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("AdminOnlyPolicy", policy => policy.RequireRole("Administrators"));
            options.AddPolicy("DepartmentITPolicy", policy => policy.RequireClaim("Department", "IT"));
            options.AddPolicy("ProfileEditPolicy", policy => policy.RequireClaim("Permission", "Profile.Edit"));
        });

        return services;
    }


    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthTokenStore, TokenStorage>();
        services.AddSingleton<IAccessTokenManager, AccessTokenManager>();
        services.AddScoped<IAuthenticatedUserPrincipalFactory, AuthenticatedUserPrincipalFactory>();

        services.AddScoped<MauiAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<MauiAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<MauiAuthenticationStateProvider>());

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