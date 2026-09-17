using GokeApp.Services;
using Goke.Core.Authorization;
using Goke.Core.Security;
using System.Windows.Input;

namespace GokeApp.Pages.Controls;

public partial class AuthorizePage : ContentPage
{
    private bool _isAuthorizing;
    private bool _attributeResolved;
    private AuthorizeAttribute? _authorizeAttribute;

    protected bool IsAuthorized { get; private set; }


    public static readonly BindableProperty AppearingCommandProperty =
        BindableProperty.Create(
            nameof(AppearingCommand),
            typeof(ICommand),
            typeof(AuthorizePage));

    public static readonly BindableProperty NavigatedToCommandProperty =
        BindableProperty.Create(
            nameof(NavigatedToCommand),
            typeof(ICommand),
            typeof(AuthorizePage));

    public static readonly BindableProperty NavigatedFromCommandProperty =
        BindableProperty.Create(
            nameof(NavigatedFromCommand),
            typeof(ICommand),
            typeof(AuthorizePage));


    public ICommand AppearingCommand
    {
        get => (ICommand)GetValue(AppearingCommandProperty);
        set => SetValue(AppearingCommandProperty, value);
    }

    public ICommand NavigatedToCommand
    {
        get => (ICommand)GetValue(NavigatedToCommandProperty);
        set => SetValue(NavigatedToCommandProperty, value);
    }

    public ICommand NavigatedFromCommand
    {
        get => (ICommand)GetValue(NavigatedFromCommandProperty);
        set => SetValue(NavigatedFromCommandProperty, value);
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isAuthorizing)
        {
            return;
        }

        _isAuthorizing = true;

        try
        {
            IsAuthorized = await EnsureAuthorizedAsync();

            if (IsAuthorized)
            {
                await OnAuthorizedAppearingAsync();
            }
        }
        finally
        {
            _isAuthorizing = false;
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (NavigatedToCommand?.CanExecute(null) == true)
        {
            NavigatedToCommand.Execute(null);
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        if (NavigatedFromCommand?.CanExecute(null) == true)
        {
            NavigatedFromCommand.Execute(null);
        }
    }


    protected virtual Task OnAuthorizedAppearingAsync()
    {
        if (AppearingCommand?.CanExecute(null) == true)
        {
            AppearingCommand.Execute(null);
        }

        return Task.CompletedTask;
    }

    protected virtual async Task<bool> EnsureAuthorizedAsync()
    {
        var attribute = GetAuthorizeAttribute();
        if (attribute is null)
        {
            return true;
        }

        var authService = ServiceHelper.GetService<IAuthenticationService>();
        if (authService is null)
        {
            return false;
        }

        if (!await authService.IsAuthenticatedAsync())
        {
            await RedirectToLoginAsync();
            return false;
        }

        if (!await AreRolesSatisfiedAsync(authService, attribute))
        {
            await DenyAccessAsync();
            return false;
        }

        if (!await ArePoliciesSatisfiedAsync(authService, attribute))
        {
            await DenyAccessAsync();
            return false;
        }

        if (!await AreClaimsSatisfiedAsync(authService, attribute))
        {
            await DenyAccessAsync();
            return false;
        }

        return true;
    }

    private static async Task<bool> AreRolesSatisfiedAsync(
        IAuthenticationService authService,
        AuthorizeAttribute attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute.Roles))
        {
            return true;
        }

        var requiredRoles = attribute.Roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var role in requiredRoles)
        {
            if (await authService.IsInRoleAsync(role))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> ArePoliciesSatisfiedAsync(
        IAuthenticationService authService,
        AuthorizeAttribute attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute.Policies))
        {
            return true;
        }

        var requiredPolicies = attribute.Policies
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var policyName in requiredPolicies)
        {
            if (!await authService.AuthorizePolicyAsync(policyName))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<bool> AreClaimsSatisfiedAsync(
        IAuthenticationService authService,
        AuthorizeAttribute attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute.Claims))
        {
            return true;
        }

        var requiredClaims = attribute.Claims
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var claim in requiredClaims)
        {
            var (type, value) = ParseClaimRequirement(claim);

            if (!await authService.HasClaimAsync(type, value))
            {
                return false;
            }
        }

        return true;
    }

    private static (string Type, string? Value) ParseClaimRequirement(string input)
    {
        var parts = input.Split('=', 2, StringSplitOptions.TrimEntries);

        return parts.Length == 2
            ? (parts[0], parts[1])
            : (input.Trim(), null);
    }

    private static async Task RedirectToLoginAsync()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }

    private static async Task DenyAccessAsync()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlertAsync("Access Denied", "You do not have permission.", "OK");
            await Shell.Current.GoToAsync("//Main");
        }
    }

    private AuthorizeAttribute? GetAuthorizeAttribute()
    {
        if (_attributeResolved)
        {
            return _authorizeAttribute;
        }

        _authorizeAttribute = GetType()
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .OfType<AuthorizeAttribute>()
            .FirstOrDefault();

        _attributeResolved = true;
        return _authorizeAttribute;
    }
}