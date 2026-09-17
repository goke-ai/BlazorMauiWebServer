using GokeApp.Pages.Controls;

namespace GokeApp.Pages;

public partial class CounterPage : AuthorizePage
{
	public CounterPage(CounterPageModel vModel)
	{
		InitializeComponent();
		BindingContext = vModel;
	}
}