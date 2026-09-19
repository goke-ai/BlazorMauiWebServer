using Goke.Core.Authentication;
using Goke.Core.Enums;
using Goke.Core.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GokeHyb.Services;

public sealed class MauiAuthenticationStateProvider(
    AuthApiClient authApiClient,
    IAccessTokenManager tokenManager,
    IAuthenticatedUserPrincipalFactory claimsPrincipalFactory,
    IAuthorizationService authorizationService,
    ILogger<MauiAuthenticationStateProvider> logger) : AuthenticationStateProvider, IAuthenticationService
{
    private const string AuthenticationType = "Custom authentication";

    private static readonly ClaimsPrincipal DefaultUser = new(new ClaimsIdentity());
    private static readonly Task<AuthenticationState> DefaultAuthState =
        Task.FromResult(new AuthenticationState(DefaultUser));

    private readonly AuthApiClient client = authApiClient;
    private Task<AuthenticationState> currentAuthState = DefaultAuthState;

    public LoginStatus LoginStatus { get; private set; } = LoginStatus.None;
    public string LoginFailureMessage { get; private set; } = string.Empty;
    public string? StatusMessage { get; private set; }
    public string? CurrentEmail => tokenManager.CurrentEmail;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (currentAuthState != DefaultAuthState)
        {
            return currentAuthState;
        }

        currentAuthState = CreateAuthenticationStateFromStoredTokenAsync();
        NotifyAuthenticationStateChanged(currentAuthState);

        return currentAuthState;
    }

    public async Task<bool> IsAuthenticatedAsync()
        => await tokenManager.GetValidTokenAsync() is not null;

    public async Task<bool> IsInRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        var user = (await GetAuthenticationStateAsync()).User;
        return user.Identity?.IsAuthenticated == true && user.IsInRole(role);
    }

    public async Task<bool> HasClaimAsync(string claimType, string? claimValue = null)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return false;
        }

        var user = (await GetAuthenticationStateAsync()).User;
        return user.Identity?.IsAuthenticated == true
            && user.HasClaim(c =>
                c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)
                && (claimValue is null || c.Value.Equals(claimValue, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task<bool> HasClaimAsync(Predicate<Claim> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var user = (await GetAuthenticationStateAsync()).User;
        return user.Identity?.IsAuthenticated == true && user.Claims.Any(c => predicate(c));
    }

    public async Task<bool> AuthorizePolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
        {
            return false;
        }

        var user = (await GetAuthenticationStateAsync()).User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var authResult = await authorizationService.AuthorizeAsync(user, policyName);
        return authResult.Succeeded;
    }

    public async Task<AccessTokenInfo?> GetAccessTokenInfoAsync()
    {
        var token = await tokenManager.GetValidTokenAsync();
        if (token is not null)
        {
            return token;
        }

        SetAnonymousState("Your session expired. Sign in again.");
        return null;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest loginRequest)
    {
        ResetMessages();
        var email = loginRequest.Email?.Trim() ?? string.Empty;

        try
        {
            var loginResponse = await client.LoginAsync(email, loginRequest.Password);
            if (loginResponse is null)
            {
                return Fail("Invalid Email or Password. Please try again.");
            }

            var token = await tokenManager.SetTokenAsync(loginResponse, email, loginRequest.RememberMe);
            if (token is null)
            {
                tokenManager.Clear();
                return Fail("Authentication response was invalid.");
            }

            var userInfo = await GetAuthenticatedUserAsync(token.LoginResponse.AccessToken);
            if (userInfo is null)
            {
                tokenManager.Clear();
                return Fail("Unable to load the signed-in user.");
            }

            SetAuthenticatedState(userInfo, email);
            return AuthenticationResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging in to the remote identity endpoint.");
            tokenManager.Clear();
            return Fail("Server error.");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest registerRequest)
    {
        ResetMessages();

        try
        {
            var error = await client.RegisterAsync(registerRequest.Email, registerRequest.Password);
            if (string.IsNullOrWhiteSpace(error))
            {
                StatusMessage = "Registration succeeded. Check your email to confirm your account before signing in.";
                return AuthenticationResult.Success();
            }

            return Fail(error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering against the remote identity endpoint.");
            return Fail("Server error.");
        }
    }

    public void Logout()
    {
        SetAnonymousState();
    }

    private async Task<AuthenticationState> CreateAuthenticationStateFromStoredTokenAsync()
    {
        ResetMessages();

        var token = await tokenManager.GetValidTokenAsync();
        if (token is null)
        {
            return new AuthenticationState(DefaultUser);
        }

        var userInfo = await GetAuthenticatedUserAsync(token.LoginResponse.AccessToken);
        if (userInfo is null)
        {
            SetAnonymousState("Your session expired. Sign in again.");
            return new AuthenticationState(DefaultUser);
        }

        LoginStatus = LoginStatus.Success;
        var principal = claimsPrincipalFactory.Create(userInfo, token.Email, AuthenticationType);
        return new AuthenticationState(principal);
    }

    private async Task<AuthenticatedUserResponse?> GetAuthenticatedUserAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        return await client.GetCurrentUserAsync(accessToken);
    }

    private void SetAuthenticatedState(AuthenticatedUserResponse userInfo, string fallbackEmail)
    {
        ResetMessages();
        LoginStatus = LoginStatus.Success;

        var principal = claimsPrincipalFactory.Create(userInfo, fallbackEmail, AuthenticationType);
        currentAuthState = Task.FromResult(new AuthenticationState(principal));
        NotifyAuthenticationStateChanged(currentAuthState);
    }

    private AuthenticationResult Fail(string? message)
    {
        LoginStatus = LoginStatus.Failed;
        LoginFailureMessage = string.IsNullOrWhiteSpace(message)
            ? "Authentication failed."
            : message;

        currentAuthState = DefaultAuthState;
        NotifyAuthenticationStateChanged(currentAuthState);

        return AuthenticationResult.Failed(LoginFailureMessage);
    }

    private void SetAnonymousState(string? statusMessage = null)
    {
        LoginStatus = LoginStatus.None;
        LoginFailureMessage = string.Empty;
        StatusMessage = statusMessage;
        currentAuthState = DefaultAuthState;
        tokenManager.Clear();

        NotifyAuthenticationStateChanged(currentAuthState);
    }

    private void ResetMessages()
    {
        LoginStatus = LoginStatus.None;
        LoginFailureMessage = string.Empty;
        StatusMessage = null;
    }
}