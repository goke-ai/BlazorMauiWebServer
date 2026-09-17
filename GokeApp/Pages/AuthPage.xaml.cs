using GokeApp.Pages.Controls;
using Goke.Core.Authorization;

namespace GokeApp.Pages;

[Authorize]
public partial class AuthPage : ScrollViewPage
{
	public AuthPage()
	{
		InitializeComponent();
	}
}