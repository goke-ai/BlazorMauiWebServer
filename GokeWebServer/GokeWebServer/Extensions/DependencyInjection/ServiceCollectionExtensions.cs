using GokeWebServer.Components.Account;
using GokeWebServer.Data;
using GokeWebServer.Extensions.DependencyInjection;
using GokeWebServer.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GokeWebServer.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnlyPolicy", policy => policy.RequireRole("Administrators"))
            .AddPolicy("DepartmentITPolicy", policy => policy.RequireClaim("Department", "IT"))
            .AddPolicy("ProfileEditPolicy", policy => policy.RequireClaim("Permission", "Profile.Edit"));

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        // Ensure unauthenticated web clients redirect to login rather than receive 401.
        // Only DefaultChallengeScheme is set here; AddIdentityApiEndpoints sets DefaultScheme
        // to BearerAndApplicationScheme which handles both bearer tokens (MAUI) and cookies (web).
        services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        // Needed for external clients to log in
        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IAuthenticatedUserProfileService, AuthenticatedUserProfileService>();

        return services;
    }
}