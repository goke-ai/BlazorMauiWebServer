using GokeApp.Models;
using GokeApp.PageModels;

namespace GokeApp.Pages;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}