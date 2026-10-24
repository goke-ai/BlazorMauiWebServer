using Goke.Core.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace GokeWebServer.Services;

internal sealed class ServerAccessTokenProvider(IHttpContextAccessor httpContextAccessor) : IAccessTokenProvider
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        var authenticationResult = await httpContext.AuthenticateAsync();
        return authenticationResult.Properties?.GetTokenValue("access_token");
    }
}
