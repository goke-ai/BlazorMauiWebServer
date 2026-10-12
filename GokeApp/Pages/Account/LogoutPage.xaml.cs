using GokeApp.Pages.Controls;
using GokeApp.PageModels;
using Goke.Core.Authorization;

namespace GokeApp.Pages.Account;

[Authorize]
public partial class LogoutPage : AuthorizePage
{

    public LogoutPage(LogoutPageModel model)
	{
		InitializeComponent();
        BindingContext = model;

    }


}