using Goke.Core.Authentication;
using Goke.Core.Security;

namespace GokeHyb.Services
{
    internal class MauiAccessTokenProvider (IAuthenticationService authProvider) : IAccessTokenProvider
    {
        public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var accessTokenInfo = await authProvider.GetAccessTokenInfoAsync(); // Replace with actual token retrieval logic
            var accessToken = accessTokenInfo?.LoginResponse.AccessToken;

            return accessToken;
        }

    }
}