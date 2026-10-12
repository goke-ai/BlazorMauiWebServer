namespace GokeWeb.Components.Account;

internal interface IWebAccountService
{
    Task<IResult> LoginAsync(BlazorLoginForm form, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> RegisterAsync(BlazorRegisterForm form, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> LogoutAsync(BlazorLogoutForm form, HttpContext httpContext);
}