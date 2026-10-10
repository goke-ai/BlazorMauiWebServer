using Goke.Core.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public partial class WeatherForecastMapView : ContentView
{
    private const string MapTemplateFileName = "weather-map-template.html";
    private const string MapScriptFileName = "weather.js";
    private const string GeneratedMapFileName = "leafletmap.generated.html";

    private static string? _mapTemplateHtml;
    private static string? _mapScript;
    private bool saveAndLoadHtml;

    public static readonly BindableProperty ForecastsProperty =
        BindableProperty.Create(
            nameof(Forecasts),
            typeof(IEnumerable<WeatherForecast>),
            typeof(WeatherForecastMapView),
            Enumerable.Empty<WeatherForecast>(),
            propertyChanged: OnForecastsChanged);


    public IEnumerable<WeatherForecast> Forecasts
    {
        get => (IEnumerable<WeatherForecast>)GetValue(ForecastsProperty);
        set => SetValue(ForecastsProperty, value);
    }

    private static void OnForecastsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        _ = ((WeatherForecastMapView)bindable).RefreshViewAsync();
    }

    public string VisibleCity => $"{(Forecasts ?? []).Count()} city/cities visible";



public WeatherForecastMapView()
{
    InitializeComponent();
    ConfigureMapMode();
}

private bool UseNativeMap =>
    OperatingSystem.IsAndroid() ||
    OperatingSystem.IsIOS() ||
    OperatingSystem.IsMacCatalyst();

private void ConfigureMapMode()
{
    NativeMapContainer.IsVisible = UseNativeMap;
    WebMapContainer.IsVisible = !UseNativeMap;
}

private async Task RefreshViewAsync()
{
    var filtered = (Forecasts ?? []).ToArray();

    EmptyStateLabel.IsVisible = filtered.Length == 0;
    NoMatchesLabel.IsVisible = false;

    if (filtered.Length == 0)
    {
        await ClearMapAsync();
        OnPropertyChanged(nameof(VisibleCity));
        return;
    }

    if (UseNativeMap)
    {
        await RenderNativeMapAsync(filtered);
        await ClearWebMapAsync();
    }
    else
    {
        await ClearNativeMapAsync();
        await RenderWebViewMapAsync(filtered);
    }

    OnPropertyChanged(nameof(VisibleCity));
}

    private async Task RenderNativeMapAsync(WeatherForecast[] filtered)
    {
        var minLat = filtered.Min(f => f.Latitude);
        var maxLat = filtered.Max(f => f.Latitude);
        var minLon = filtered.Min(f => f.Longitude);
        var maxLon = filtered.Max(f => f.Longitude);

        var centerLat = (minLat + maxLat) / 2d;
        var centerLon = (minLon + maxLon) / 2d;

        var latKm = Math.Max(20d, (maxLat - minLat) * 111d);
        var lonKm = Math.Max(20d, (maxLon - minLon) * 111d);
        var radiusKm = Math.Max(latKm, lonKm) * 0.75d;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            WeatherMap.Pins.Clear();

            foreach (var forecast in filtered)
            {
                WeatherMap.Pins.Add(new Pin
                {
                    Label = $"{forecast.City} - {forecast.TemperatureC}°C",
                    Address = $"{forecast.Summary} ({forecast.Date:yyyy-MM-dd HH:mm})",
                    Location = new Location(forecast.Latitude, forecast.Longitude)
                });
            }

            WeatherMap.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Location(centerLat, centerLon),
                    Distance.FromKilometers(radiusKm)));
        });
    }

    private async Task RenderWebViewMapAsync(WeatherForecast[] filtered)
    {
        var html = await BuildLeafletHtmlAsync(filtered);

        if (saveAndLoadHtml)
        {
            await SaveHtmlToFileAndLoadAsync(html);
        }
        else
        {
            await LoadHtmlIntoWebViewAsync(html);
        }

    }


private async Task ClearNativeMapAsync()
{
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        WeatherMap.Pins.Clear();
    });
}

private async Task ClearWebMapAsync()
{
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        WeatherMapWebView.Source = null;
    });
}

private async Task ClearMapAsync()
{
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        WeatherMap.Pins.Clear();
        WeatherMapWebView.Source = null;
    });
}

    private async Task SaveHtmlToFileAndLoadAsync(string html)
    {
        var filePath = Path.Combine(FileSystem.CacheDirectory, GeneratedMapFileName);

        await File.WriteAllTextAsync(filePath, html);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            WeatherMapWebView.Source = filePath;
        });
    }

    private async Task LoadHtmlIntoWebViewAsync(string html)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            WeatherMapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        });
    }

    private static async Task<string> BuildLeafletHtmlAsync(WeatherForecast[] forecasts)
    {
        var template = await GetMapTemplateHtmlAsync();
        var mapScript = await GetMapScriptAsync();

        var mapForecasts = forecasts
            .Select(f => new
            {
                city = f.City,
                latitude = f.Latitude,
                longitude = f.Longitude,
                temperatureC = f.TemperatureC,
                summary = f.Summary,
                date = f.Date.ToString("yyyy-MM-dd HH:mm")
            })
            .ToArray();

        var json = JsonSerializer.Serialize(mapForecasts);
        var minTemp = forecasts.Length == 0 ? 0 : forecasts.Min(f => f.TemperatureC);
        var maxTemp = forecasts.Length == 0 ? 0 : forecasts.Max(f => f.TemperatureC);

        return template
            .Replace("__FORECASTS_JSON__", json, StringComparison.Ordinal)
            .Replace("__MIN_TEMP__", minTemp.ToString(), StringComparison.Ordinal)
            .Replace("__MAX_TEMP__", maxTemp.ToString(), StringComparison.Ordinal)
            .Replace("__WEATHER_MAP_SCRIPT__", mapScript, StringComparison.Ordinal)
            .Replace("__EMPTY_MESSAGE__", "No forecast data available.", StringComparison.Ordinal);
    }

    private static async Task<string> GetMapTemplateHtmlAsync()
    {
        if (!string.IsNullOrWhiteSpace(_mapTemplateHtml))
        {
            return _mapTemplateHtml;
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync(MapTemplateFileName);
        using var reader = new StreamReader(stream);

        _mapTemplateHtml = await reader.ReadToEndAsync();
        return _mapTemplateHtml;
    }

    private static async Task<string> GetMapScriptAsync()
    {
        if (!string.IsNullOrWhiteSpace(_mapScript))
        {
            return _mapScript;
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync(MapScriptFileName);
        using var reader = new StreamReader(stream);

        _mapScript = await reader.ReadToEndAsync();
        return _mapScript;
    }
}