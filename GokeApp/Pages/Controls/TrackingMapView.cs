using Goke.Core.Models.Map;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public sealed partial class TrackingMapView : ContentView
{
    private const string MapCoreScriptFileName = "map-core.js";
    private const string MapTemplateFileName = "map-view-template.html";
    private const string MapScriptFileName = "tracking-map.js";

    private readonly Label _visibleMarkersLabel;
    private readonly Label _emptyStateLabel;
    private readonly WebView _trackingMapWebView;
    private readonly MapWebViewHost _mapWebViewHost;
    private string? _pendingMarkersJson;
    private bool _pendingClear;

    public static readonly BindableProperty MarkersProperty =
        BindableProperty.Create(
            nameof(Markers),
            typeof(IEnumerable<MapMarker>),
            typeof(TrackingMapView),
            Enumerable.Empty<MapMarker>(),
            propertyChanged: OnMarkersChanged);

    public TrackingMapView()
    {
        _visibleMarkersLabel = new Label
        {
            FontSize = 12,
            Opacity = 0.7
        };

        _emptyStateLabel = new Label
        {
            Text = "No tracking markers available.",
            IsVisible = false
        };

        _trackingMapWebView = new WebView();
        _mapWebViewHost = new MapWebViewHost(_trackingMapWebView, PushPendingUpdateAsync);

        var border = new Border
        {
            HeightRequest = 520,
            Stroke = new SolidColorBrush(Color.FromArgb("#DDDDDD")),
            Content = _trackingMapWebView
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
        _ = ((TrackingMapView)bindable).RefreshViewAsync();
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
        var trackingModuleUrl = await MapWebViewAssetCache.GetModuleUrlAsync(MapCoreScriptFileName, MapScriptFileName);

        return template
            .Replace("__MAP_CONFIG_NAME__", "trackingMapConfig", StringComparison.Ordinal)
            .Replace("__MAP_BRIDGE_NAME__", "trackingMapBridge", StringComparison.Ordinal)
            .Replace("__MAP_DATA_PROPERTY__", "markers", StringComparison.Ordinal)
            .Replace("__MAP_DATA_JSON__", markersJson, StringComparison.Ordinal)
            .Replace("__MAP_MODULE_URL__", trackingModuleUrl, StringComparison.Ordinal)
            .Replace("__MAP_SET_EXPORT__", "setMarkersJson", StringComparison.Ordinal)
            .Replace("__MAP_CLEAR_EXPORT__", "clearMarkers", StringComparison.Ordinal)
            .Replace("__MAP_SET_METHOD__", "setMarkersJson", StringComparison.Ordinal)
            .Replace("__EMPTY_MESSAGE__", "No tracking markers available.", StringComparison.Ordinal)
            .Replace("__TILE_WARNING_MESSAGE__", "Map tiles are unavailable right now. Tracking markers are still shown.", StringComparison.Ordinal);
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
                markerOpacity = marker.MarkerOpacity
            })
            .ToArray();

        return JsonSerializer.Serialize(mapMarkers);
    }

    private static string BuildSetMarkersScript(string markersJson)
    {
        var quotedMarkersJson = JsonSerializer.Serialize(markersJson);

        return $"(() => {{ const bridge = window.trackingMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.setMarkersJson({quotedMarkersJson}, window.trackingMapConfig ?? {{}}); return 'ready'; }})()";
    }

    private static string BuildClearScript()
    {
        return "(() => { const bridge = window.trackingMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.clear(); return 'ready'; })()";
    }
}
