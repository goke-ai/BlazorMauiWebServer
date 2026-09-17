using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GokeApp.Services;
using Goke.Core.Security;
using Goke.Core.Authentication;

namespace GokeApp.PageModels;

public partial class LoginPageModel(IAuthenticationService authService, ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
{
	
	[ObservableProperty]
	private string _email =  string.Empty;

	[ObservableProperty]
	private string _password = string.Empty;

	[ObservableProperty]
	private bool _rememberMe;

	[ObservableProperty]
	private string _message = string.Empty;

	[ObservableProperty]
    bool _isMessageVisible;

    [RelayCommand]
    private async Task Login()
    {
        var loginRequest = new LoginRequest
        {
            Email = Email?.Trim() ?? string.Empty,
            Password = Password ?? string.Empty,
            RememberMe = RememberMe,
        };

        var result = await authService.AuthenticateAsync(loginRequest);

        if (!result.Succeeded)
        {
            Message = result.ErrorMessage ?? authService.LoginFailureMessage;
            IsMessageVisible = true;
            //await RefreshUiAsync();
            return;
        }

        Password = string.Empty;

        if (Shell.Current is AppShell shell)
        {
            await shell.RefreshMenuAsync();
        }

        await Shell.Current.GoToAsync("//Main");

    }

    [RelayCommand]
    private async Task Register()
    {
        await Shell.Current.GoToAsync("//Register");
    }


    

	
}