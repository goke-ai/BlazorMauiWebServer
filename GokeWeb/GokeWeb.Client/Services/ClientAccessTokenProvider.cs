using System.Net;
using System.Net.Http.Json;
using Goke.Core.Authentication;
using Microsoft.AspNetCore.Components;

namespace GokeWeb.Client.Services;

internal sealed class ClientAccessTokenProvider(HttpClient httpClient, NavigationManager navigationManager) : IAccessTokenProvider
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUri = new Uri(new Uri(navigationManager.BaseUri), "account/access-token");
            var payload = await httpClient.GetFromJsonAsync<AccessTokenResponse>(requestUri, cancellationToken);
            return payload?.AccessToken;
        }
        catch
        {
            return null;
        }
    }
}
