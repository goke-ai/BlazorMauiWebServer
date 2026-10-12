using Goke.Core.Models;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public partial class WeatherForecastMapView : ContentView
{
    private const string MapCoreScriptFileName = "map-core.js";
    private const string MapTemplateFileName = "weather-map-template.html";
    private const string MapScriptFileName = "weather-forecast-map.js";

    private readonly MapWebViewHost _mapWebViewHost;
    private string? _pendingForecastsJson;
    private int _pendingScaleMinTemperature;
    private int _pendingScaleMaxTemperature;
    private bool _pendingClear;

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
        _mapWebViewHost = new MapWebViewHost(WeatherMapWebView, PushPendingUpdateAsync);
    }

    private async Task LoadInitialMapAsync()
    {
        var html = await BuildLeafletHtmlAsync();
        await _mapWebViewHost.LoadHtmlAsync(html);
    }

    private async Task PushPendingUpdateAsync()
    {
        var script = _pendingClear
            ? BuildClearScript()
            : BuildSetForecastsScript(
                _pendingForecastsJson ?? "[]",
                _pendingScaleMinTemperature,
                _pendingScaleMaxTemperature);

        await _mapWebViewHost.InvokeWhenReadyAsync(script);
    }

    private async Task RefreshViewAsync()
    {
        var filtered = (Forecasts ?? []).ToArray();
        var forecastsJson = BuildForecastsJson(filtered);

        EmptyStateLabel.IsVisible = filtered.Length == 0;
        NoMatchesLabel.IsVisible = false;
        _pendingForecastsJson = forecastsJson;
        _pendingScaleMinTemperature = filtered.Length == 0 ? 0 : filtered.Min(f => f.TemperatureC);
        _pendingScaleMaxTemperature = filtered.Length == 0 ? 0 : filtered.Max(f => f.TemperatureC);
        _pendingClear = filtered.Length == 0;

        if (!_mapWebViewHost.IsMapHtmlLoaded)
        {
            await LoadInitialMapAsync();
            OnPropertyChanged(nameof(VisibleCity));
            return;
        }

        await PushPendingUpdateAsync();

        OnPropertyChanged(nameof(VisibleCity));
    }

    private static string BuildForecastsJson(WeatherForecast[] forecasts)
    {
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

        return JsonSerializer.Serialize(mapForecasts);
    }

    private string BuildSetForecastsScript(string forecastsJson, int minTemp, int maxTemp)
    {
        var quotedForecastsJson = JsonSerializer.Serialize(forecastsJson);

        return $"(() => {{ const bridge = window.weatherMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.setForecastsJson({quotedForecastsJson}, {minTemp}, {maxTemp}); return 'ready'; }})()";
    }

    private static string BuildClearScript()
    {
        return "(() => { const bridge = window.weatherMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.clear(); return 'ready'; })()";
    }

    private async Task<string> BuildLeafletHtmlAsync()
    {
        var template = await MapWebViewAssetCache.GetTemplateHtmlAsync(MapTemplateFileName);
        var weatherModuleUrl = await MapWebViewAssetCache.GetModuleUrlAsync(MapCoreScriptFileName, MapScriptFileName);

        return template
            .Replace("__FORECASTS_JSON__", _pendingForecastsJson ?? "[]", StringComparison.Ordinal)
            .Replace("__MIN_TEMP__", _pendingScaleMinTemperature.ToString(), StringComparison.Ordinal)
            .Replace("__MAX_TEMP__", _pendingScaleMaxTemperature.ToString(), StringComparison.Ordinal)
            .Replace("__WEATHER_MODULE_URL__", weatherModuleUrl, StringComparison.Ordinal)
            .Replace("__EMPTY_MESSAGE__", "No forecast data available.", StringComparison.Ordinal);
    }
}