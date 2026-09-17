using GokeApp.Services;
using Goke.Core.Interfaces;

namespace GokeApp.Pages.Controls;

public partial class FormFactorView : ContentView
{
	public FormFactorView()
	{
		InitializeComponent();

        var viewModel = new FormFactorViewModel(ServiceHelper.GetService<IFormFactor>());
        BindingContext = viewModel;
	}
}