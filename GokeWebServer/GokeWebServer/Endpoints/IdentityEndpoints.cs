using GokeWebServer.Data;
using GokeWebServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GokeWebServer.Endpoints
{
    public static class IdentityEndpoints
    {
        public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var identityGroup = endpoints.MapGroup("/identity");

            identityGroup.MapIdentityApi<ApplicationUser>();

            identityGroup.MapGet("me", async (
                ClaimsPrincipal principal,
                IAuthenticatedUserProfileService profileService,
                CancellationToken cancellationToken) =>
            {
                var user = await profileService.GetCurrentUserAsync(principal, cancellationToken);
                return user is null ? Results.Unauthorized() : Results.Ok(user);
            }).RequireAuthorization();

            return endpoints;
        }
    }
}