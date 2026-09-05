using GokeApp.Models;
using GokeApp.PageModels;

namespace GokeApp.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}