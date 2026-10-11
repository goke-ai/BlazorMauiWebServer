using Goke.Core.Models;
using System.Text;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public partial class WeatherForecastMapView : ContentView
{
    private const string MapCoreScriptFileName = "map-core.js";
    private const string MapTemplateFileName = "weather-map-template.html";
    private const string MapScriptFileName = "weather-map.js";

    private static string? _mapTemplateHtml;
    private static string? _weatherModuleUrl;

    private bool _isMapHtmlLoaded;
    private bool _isMapPageReady;
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
        WeatherMapWebView.Navigated += WeatherMapWebView_Navigated;
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

        if (!_isMapHtmlLoaded)
        {
            await LoadInitialMapAsync();
            OnPropertyChanged(nameof(VisibleCity));
            return;
        }

        await PushPendingUpdateAsync();

        OnPropertyChanged(nameof(VisibleCity));
    }

    private void WeatherMapWebView_Navigated(object? sender, WebNavigatedEventArgs e)
    {
        _isMapPageReady = true;
        _ = PushPendingUpdateAsync();
    }

    private async Task LoadInitialMapAsync()
    {
        var html = await BuildLeafletHtmlAsync();
        await LoadHtmlIntoWebViewAsync(html);
    }

    private async Task PushPendingUpdateAsync()
    {
        if (!_isMapHtmlLoaded || !_isMapPageReady)
        {
            return;
        }

        var script = _pendingClear
            ? BuildClearScript()
            : BuildSetForecastsScript(
                _pendingForecastsJson ?? "[]",
                _pendingScaleMinTemperature,
                _pendingScaleMaxTemperature);

        await InvokeWeatherMapScriptAsync(script);
    }

    private async Task InvokeWeatherMapScriptAsync(string script)
    {
        for (var attempt = 0; attempt < 5; attempt += 1)
        {
            var result = await MainThread.InvokeOnMainThreadAsync(
                () => WeatherMapWebView.EvaluateJavaScriptAsync(script));

            if (string.Equals(result, "ready", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(result, "\"ready\"", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await Task.Delay(100);
        }
    }

    private async Task LoadHtmlIntoWebViewAsync(string html)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _isMapPageReady = false;
            _isMapHtmlLoaded = true;
            WeatherMapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        });
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
        var template = await GetMapTemplateHtmlAsync();
        var weatherModuleUrl = await GetWeatherModuleUrlAsync();

        return template
            .Replace("__FORECASTS_JSON__", _pendingForecastsJson ?? "[]", StringComparison.Ordinal)
            .Replace("__MIN_TEMP__", _pendingScaleMinTemperature.ToString(), StringComparison.Ordinal)
            .Replace("__MAX_TEMP__", _pendingScaleMaxTemperature.ToString(), StringComparison.Ordinal)
            .Replace("__WEATHER_MODULE_URL__", weatherModuleUrl, StringComparison.Ordinal)
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

    private static async Task<string> GetMapCoreScriptAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(MapCoreScriptFileName);
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }

    private static async Task<string> GetWeatherModuleUrlAsync()
    {
        if (!string.IsNullOrWhiteSpace(_weatherModuleUrl))
        {
            return _weatherModuleUrl;
        }

        var mapCoreModuleUrl = CreateJavaScriptModuleUrl(await GetMapCoreScriptAsync());

        using var stream = await FileSystem.OpenAppPackageFileAsync(MapScriptFileName);
        using var reader = new StreamReader(stream);

        var mapScript = (await reader.ReadToEndAsync())
            .Replace("__MAP_CORE_MODULE_URL__", mapCoreModuleUrl, StringComparison.Ordinal);

        _weatherModuleUrl = CreateJavaScriptModuleUrl(mapScript);
        return _weatherModuleUrl;
    }

    private static string CreateJavaScriptModuleUrl(string script)
    {
        var bytes = Encoding.UTF8.GetBytes(script);
        return $"data:text/javascript;base64,{Convert.ToBase64String(bytes)}";
    }
}