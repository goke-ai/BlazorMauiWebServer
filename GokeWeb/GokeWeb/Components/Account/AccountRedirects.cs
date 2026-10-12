namespace GokeWeb.Components.Account;

internal static class AccountRedirects
{
    public static string NormalizeReturnUrl(string? returnUrl)
    {
        return string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    }

    public static IResult LoginError(string message, string returnUrl)
    {
        return Results.LocalRedirect(
            $"/login?error={Uri.EscapeDataString(message)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    public static IResult RegisterError(string message, string returnUrl)
    {
        return Results.LocalRedirect(
            $"/register?error={Uri.EscapeDataString(message)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    public static IResult LoginStatus(string message, string returnUrl)
    {
        return Results.LocalRedirect(
            $"/login?status={Uri.EscapeDataString(message)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }
}