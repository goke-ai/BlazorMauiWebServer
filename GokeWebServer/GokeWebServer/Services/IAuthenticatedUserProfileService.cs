using Goke.Core.Authentication;
using System.Security.Claims;

namespace GokeWebServer.Services;

internal interface IAuthenticatedUserProfileService
{
    Task<AuthenticatedUserResponse?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
