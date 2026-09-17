using GokeApp.Services;
using Goke.Core.Security;

namespace GokeApp.PageModels;

public partial class LogoutPageModel : BasePageModel
{
    private readonly IAuthenticationService authService;
    private readonly ModalErrorHandler errorHandler;

    public LogoutPageModel(IAuthenticationService authService, ModalErrorHandler errorHandler) : base(errorHandler)
    {
        this.authService = authService;
        this.errorHandler = errorHandler;
    }

    protected override async Task OnInitDataAsync()
    {
        await base.OnInitDataAsync();
        await LogoutAsync();
    }

    private async Task LogoutAsync()
    {
        try
        {
            authService.Logout();

            if (Shell.Current is AppShell shell)
            {
                await shell.RefreshMenuAsync();
            }

            await Shell.Current.GoToAsync("//Main", true);
        }
        catch (Exception e)
        {
            errorHandler.HandleError(e);
        }
    }
}
