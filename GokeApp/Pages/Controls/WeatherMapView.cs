using Goke.Core.Map;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public sealed partial class WeatherMapView : ContentView
{
    private const string MapCoreScriptFileName = "map-core.js";
    private const string MapTemplateFileName = "map-view-template.html";
    private const string MapScriptFileName = "weather-map.js";

    private readonly Label _visibleMarkersLabel;
    private readonly Label _emptyStateLabel;
    private readonly WebView _currentLocationMapWebView;
    private readonly MapWebViewHost _mapWebViewHost;
    private string? _pendingMarkersJson;
    private bool _pendingClear;

    public static readonly BindableProperty MarkersProperty =
        BindableProperty.Create(
            nameof(Markers),
            typeof(IEnumerable<MapMarker>),
            typeof(CurrentLocationMapView),
            Enumerable.Empty<MapMarker>(),
            propertyChanged: OnMarkersChanged);

    public WeatherMapView()
    {
        _visibleMarkersLabel = new Label
        {
            FontSize = 12,
            Opacity = 0.7
        };

        _emptyStateLabel = new Label
        {
            Text = "Weather forecast is unavailable.",
            IsVisible = false
        };

        _currentLocationMapWebView = new WebView();
        _mapWebViewHost = new MapWebViewHost(_currentLocationMapWebView, PushPendingUpdateAsync);

        var border = new Border
        {
            HeightRequest = 520,
            Stroke = new SolidColorBrush(Color.FromArgb("#DDDDDD")),
            Content = _currentLocationMapWebView
        };

        Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                _visibleMarkersLabel,
                _emptyStateLabel,
                border
            }
        };

        UpdateVisibleMarkersText();
    }

    public IEnumerable<MapMarker> Markers
    {
        get => (IEnumerable<MapMarker>)GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    private static void OnMarkersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        _ = ((WeatherMapView)bindable).RefreshViewAsync();
    }

    private async Task RefreshViewAsync()
    {
        var markers = (Markers ?? []).ToArray();
        var markersJson = BuildMarkersJson(markers);

        _emptyStateLabel.IsVisible = markers.Length == 0;
        UpdateVisibleMarkersText();
        _pendingMarkersJson = markersJson;
        _pendingClear = markers.Length == 0;

        if (!_mapWebViewHost.IsMapHtmlLoaded)
        {
            if (markers.Length == 0)
            {
                return;
            }

            await LoadInitialMapAsync(markersJson);
            return;
        }

        await PushPendingUpdateAsync();
    }

    private void UpdateVisibleMarkersText()
    {
        _visibleMarkersLabel.Text = $"{(Markers ?? []).Count()} marker(s) visible";
    }

    private async Task LoadInitialMapAsync(string markersJson)
    {
        var html = await BuildLeafletHtmlAsync(markersJson);
        await _mapWebViewHost.LoadHtmlAsync(html);
    }

    private async Task PushPendingUpdateAsync()
    {
        var script = _pendingClear
            ? BuildClearScript()
            : BuildSetMarkersScript(_pendingMarkersJson ?? "[]");

        await _mapWebViewHost.InvokeWhenReadyAsync(script);
    }

    private static async Task<string> BuildLeafletHtmlAsync(string markersJson)
    {
        var template = await MapWebViewAssetCache.GetTemplateHtmlAsync(MapTemplateFileName);
        var currentLocationModuleUrl = await MapWebViewAssetCache.GetModuleUrlAsync(MapCoreScriptFileName, MapScriptFileName);

        return template
            .Replace("__MAP_CONFIG_NAME__", "weatherMapConfig", StringComparison.Ordinal)
            .Replace("__MAP_BRIDGE_NAME__", "weatherMapBridge", StringComparison.Ordinal)
            .Replace("__MAP_DATA_PROPERTY__", "markers", StringComparison.Ordinal)
            .Replace("__MAP_DATA_JSON__", markersJson, StringComparison.Ordinal)
            .Replace("__MAP_MODULE_URL__", currentLocationModuleUrl, StringComparison.Ordinal)
            .Replace("__MAP_SET_EXPORT__", "setMarkersJson", StringComparison.Ordinal)
            .Replace("__MAP_CLEAR_EXPORT__", "clearMarkers", StringComparison.Ordinal)
            .Replace("__MAP_SET_METHOD__", "setMarkersJson", StringComparison.Ordinal)
            .Replace("__EMPTY_MESSAGE__", "Weather forecast is unavailable.", StringComparison.Ordinal)
            .Replace("__TILE_WARNING_MESSAGE__", "Map tiles are unavailable right now. Weather forecast markers are still shown.", StringComparison.Ordinal);
    }

    private static string BuildMarkersJson(MapMarker[] markers)
    {
        var mapMarkers = markers
            .Select(marker => new
            {
                id = marker.Id,
                name = marker.Name,
                latitude = marker.Latitude,
                longitude = marker.Longitude,
                description = marker.Description,
                timestamp = marker.Timestamp,
                popupHtml = marker.PopupHtml,
                markerColor = marker.MarkerColor,
                markerShape = marker.MarkerShape,
                markerInitials = marker.MarkerInitials,
                markerOpacity = marker.MarkerOpacity,
                markerValue = marker.MarkerValue
            })
            .ToArray();

        return JsonSerializer.Serialize(mapMarkers);
    }

    private static string BuildSetMarkersScript(string markersJson)
    {
        var quotedMarkersJson = JsonSerializer.Serialize(markersJson);

        return $"(() => {{ const bridge = window.weatherMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.setMarkersJson({quotedMarkersJson}, {{ ...(window.weatherMapConfig ?? {{}}), fitBounds: true }}); return 'ready'; }})()";
    }

    private static string BuildClearScript()
    {
        return "(() => { const bridge = window.weatherMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.clear(); return 'ready'; })()";
    }
}
