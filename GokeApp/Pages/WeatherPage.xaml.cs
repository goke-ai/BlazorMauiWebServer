using System.ComponentModel;
using System.Text.Json;
using Goke.Core.Models;
using GokeApp.PageModels;
using GokeApp.Pages.Controls;

namespace GokeApp.Pages;

public partial class WeatherPage : ScrollViewPage
{
    public WeatherPage(WeatherPageModel vModel)
    {
        InitializeComponent();

        BindingContext = vModel;
    }

}