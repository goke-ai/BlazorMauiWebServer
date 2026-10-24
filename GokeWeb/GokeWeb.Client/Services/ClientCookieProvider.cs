using System.Net;
using System.Net.Http.Json;
using Goke.Core.Authentication;
using Microsoft.AspNetCore.Components;

namespace GokeWeb.Client.Services;

internal sealed class ClientCookieProvider(HttpClient httpClient, NavigationManager navigationManager) : ICookieProvider
{
    public async Task<CookieContainer?> CreateAuthCookies(CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUri = new Uri(new Uri(navigationManager.BaseUri), "account/cookies");
            var payload = await httpClient.GetFromJsonAsync<CookieContainerResponse>(requestUri, cancellationToken);
            if (payload is null || payload.CookieContainer is null)
            {
                return null;
            }
            return payload.CookieContainer;

            //var cookies = new CookieContainer();
            //foreach (var cookie in payload.Cookies)
            //{
            //    cookies.Add(new Cookie(cookie.Name, cookie.Value, "/", cookie.Domain));
            //}
            //return cookies;
        }
        catch
        {
            return null;
        }

    }


}
