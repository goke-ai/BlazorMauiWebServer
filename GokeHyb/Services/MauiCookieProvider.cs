using Goke.Core.Authentication;
using System.Net;

namespace GokeHyb.Services
{
    internal class MauiCookieProvider : ICookieProvider
    {
        public Task<CookieContainer?> CreateAuthCookies(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CookieContainer?>(null);
        }
    }
}