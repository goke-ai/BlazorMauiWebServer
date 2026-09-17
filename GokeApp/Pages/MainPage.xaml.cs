using GokeApp.PageModels;

namespace GokeApp.Pages;

public partial class MainPage : ContentPage
{

	public MainPage(MainPageModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	
}
