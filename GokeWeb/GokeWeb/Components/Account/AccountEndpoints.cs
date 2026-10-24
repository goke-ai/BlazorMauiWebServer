using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace GokeWeb.Components.Account;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/login", async (
            [FromForm] BlazorLoginForm form,
            HttpContext httpContext,
            IWebAccountService accountService) =>
        {
            return await accountService.LoginAsync(form, httpContext, httpContext.RequestAborted);
        });

        endpoints.MapPost("/account/register", async (
            [FromForm] BlazorRegisterForm form,
            HttpContext httpContext,
            IWebAccountService accountService) =>
        {
            return await accountService.RegisterAsync(form, httpContext, httpContext.RequestAborted);
        });

        endpoints.MapPost("/account/logout", async (
            [FromForm] BlazorLogoutForm form,
            HttpContext httpContext,
            IWebAccountService accountService) =>
        {
            return await accountService.LogoutAsync(form, httpContext);
        }).RequireAuthorization();

        endpoints.MapGet("/account/access-token", async (HttpContext httpContext, IWebAccountService accountService) =>
        {
            var token = await accountService.GetAccessTokenAsync(httpContext);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { accessToken = token });
        }).RequireAuthorization();

        endpoints.MapGet("/account/cookies", async (HttpContext httpContext, IWebAccountService accountService) =>
        {
            var cookieContainer = await accountService.CreateAuthCookies(httpContext);
            if (cookieContainer is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { cookieContainer });
        }).RequireAuthorization();

        return endpoints;
    }
}