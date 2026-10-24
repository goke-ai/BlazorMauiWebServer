using Goke.Core.Authentication;
using Goke.Core.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Security.Claims;

namespace GokeWeb.Components.Account;

internal sealed class WebAccountService(AuthApiClient client) : IWebAccountService
{
    public async Task<IResult> LoginAsync(
        BlazorLoginForm form,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var returnUrl = AccountRedirects.NormalizeReturnUrl(form.ReturnUrl);

        var loginResponse = await client.LoginAsync(form.Email, form.Password, cancellationToken);
        if (loginResponse is null)
        {
            return AccountRedirects.LoginError("Invalid login attempt.", returnUrl);
        }

        var currentUser = await client.GetCurrentUserAsync(loginResponse.AccessToken, cancellationToken);
        if (currentUser is null)
        {
            return AccountRedirects.LoginError("Unable to load user profile.", returnUrl);
        }

        var claims = ClaimBuilder.BuildClaimFomUserInfo(currentUser, form.Email);
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            CreateAuthenticationProperties(form.RememberMe, loginResponse));

        return Results.LocalRedirect(returnUrl);
    }

    public async Task<IResult> RegisterAsync(
        BlazorRegisterForm form,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var returnUrl = AccountRedirects.NormalizeReturnUrl(form.ReturnUrl);

        if (string.IsNullOrWhiteSpace(form.Email) || string.IsNullOrWhiteSpace(form.Password))
        {
            return AccountRedirects.RegisterError("Email and password are required.", returnUrl);
        }

        if (!string.Equals(form.Password, form.ConfirmPassword, StringComparison.Ordinal))
        {
            return AccountRedirects.RegisterError("The password and confirmation password do not match.", returnUrl);
        }

        var registrationError = await client.RegisterAsync(form.Email, form.Password, cancellationToken);
        if (!string.IsNullOrWhiteSpace(registrationError))
        {
            return AccountRedirects.RegisterError(registrationError, returnUrl);
        }

        return AccountRedirects.LoginStatus(
            "Registration succeeded. Check your email to confirm your account before signing in.",
            returnUrl);
    }

    public async Task<IResult> LogoutAsync(BlazorLogoutForm form, HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var returnUrl = AccountRedirects.NormalizeReturnUrl(form.ReturnUrl);
        return Results.LocalRedirect(returnUrl);
    }

    private static AuthenticationProperties CreateAuthenticationProperties(bool rememberMe, LoginResponse loginResponse)
    {
        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(loginResponse.ExpiresIn)
        };

        properties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = loginResponse.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = loginResponse.RefreshToken },
            new AuthenticationToken { Name = "token_type", Value = loginResponse.TokenType }
        ]);

        return properties;
    }

    public Task<string?> GetAccessTokenAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            return Task.FromResult<string?>(null);
        }

        return httpContext.GetTokenAsync("access_token");        
    }

    public Task<CookieContainer?> CreateAuthCookies(HttpContext httpContext)
    {
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

    //public Task<List<Cookie>?> CreateAuthCookies(HttpContext httpContext)
    //{
    //    if (httpContext is null || httpContext.Request.Cookies.Count == 0)
    //    {
    //        return Task.FromResult<List<Cookie>?>(null);
    //    }

    //    var request = httpContext.Request;
    //    var baseUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/", UriKind.Absolute);
    //    var cookies = new List<Cookie>();
    //    var domain = request.Host.Host;

    //    foreach (var cookie in request.Cookies)
    //    {
    //        cookies.Add(new Cookie(cookie.Key, cookie.Value, "/", domain));
    //    }

    //    return Task.FromResult<List<Cookie>?>(cookies);
    //}
}