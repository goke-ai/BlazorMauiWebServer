using GokeApp.Pages.Controls;
using GokeApp.PageModels;

namespace GokeApp.Pages.Account;

public partial class LoginPage : ScrollViewPage
{

    public LoginPage(LoginPageModel model)
	{
		InitializeComponent();
        BindingContext = model;

    }


}