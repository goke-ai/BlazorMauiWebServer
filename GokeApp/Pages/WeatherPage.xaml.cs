using GokeApp.Pages.Controls;
using GokeApp.PageModels;

namespace GokeApp.Pages;

public partial class WeatherPage : ScrollViewPage
{
	public WeatherPage(WeatherPageModel vModel)
	{
		InitializeComponent();
		BindingContext = vModel;
	}
}