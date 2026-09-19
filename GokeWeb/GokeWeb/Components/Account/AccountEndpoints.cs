using Microsoft.AspNetCore.Mvc;

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

        return endpoints;
    }
}