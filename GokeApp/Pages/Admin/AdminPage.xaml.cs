using GokeApp.Pages.Controls;
using Goke.Core.Authorization;

namespace GokeApp.Pages.Admin;

[Authorize(Roles = "Administrators")]
public partial class AdminPage : AuthorizePage
{
	public AdminPage()
	{
		InitializeComponent();
	}
}