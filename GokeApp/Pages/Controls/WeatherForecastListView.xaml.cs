using CommunityToolkit.Mvvm.ComponentModel;
using Goke.Core.Models;

namespace GokeApp.Pages.Controls;

public partial class WeatherForecastListView : ContentView
{
    public static readonly BindableProperty ForecastsProperty =
    BindableProperty.Create(
        nameof(Forecasts),
        typeof(IEnumerable<WeatherForecast>),
        typeof(WeatherForecastListView),
        Enumerable.Empty<WeatherForecast>(),
        propertyChanged: OnForecastsChanged);

    public WeatherForecastListView()
	{
		InitializeComponent();
	}

    public IEnumerable<WeatherForecast> Forecasts
    {
        get => (IEnumerable<WeatherForecast>)GetValue(ForecastsProperty);
        set => SetValue(ForecastsProperty, value);
    }


    private static void OnForecastsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((WeatherForecastListView)bindable).RefreshView();
    }

    private void RefreshView()
    {
    }
}