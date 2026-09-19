using Goke.Core.Authentication;
using GokeWebServer.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GokeWebServer.Services;

internal sealed class AuthenticatedUserProfileService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IAuthenticatedUserProfileService
{
    public async Task<AuthenticatedUserResponse?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        var allClaims = new List<AuthenticatedUserClaimResponse>();

        foreach (var claim in userClaims)
        {
            AddClaim(allClaims, claim.Type, claim.Value);
        }

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            var roleClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
            {
                AddClaim(allClaims, claim.Type, claim.Value);
            }
        }

        return new AuthenticatedUserResponse
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.UserName ?? user.Email ?? string.Empty,
            Roles = [.. roles],
            Claims = [.. allClaims.DistinctBy(c => new { c.Type, c.Value })]
        };
    }

    private static void AddClaim(List<AuthenticatedUserClaimResponse> claims, string? type, string? value)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        claims.Add(new AuthenticatedUserClaimResponse
        {
            Type = type,
            Value = value
        });
    }
}