using Goke.Core.Authentication;
using System.Net;

namespace GokeWeb.Services;

internal sealed class ServerCookieProvider(IHttpContextAccessor httpContextAccessor) : ICookieProvider
{
    public Task<CookieContainer?> CreateAuthCookies(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.Request.Cookies.Count == 0)
        {
            return Task.FromResult<CookieContainer?>(null);
        }

        var request = httpContext.Request;
        var baseUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/", UriKind.Absolute);
        var cookies = new CookieContainer();

        foreach (var cookie in request.Cookies)
        {
            cookies.Add(baseUri, new Cookie(cookie.Key, cookie.Value, "/"));
        }

        return Task.FromResult<CookieContainer?>(cookies);
    }

}
