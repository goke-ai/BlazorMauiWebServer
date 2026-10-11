using GokeApp.Models;
using System.Text;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public sealed partial class TrackingMapView : ContentView
{
    private const string MapCoreScriptFileName = "map-core.js";
    private const string MapTemplateFileName = "tracking-map-template.html";
    private const string MapScriptFileName = "tracking.map.js";

    private static string? _mapTemplateHtml;
    private static string? _trackingModuleUrl;

    private readonly Label _visibleMarkersLabel;
    private readonly Label _emptyStateLabel;
    private readonly WebView _trackingMapWebView;
    private bool _isMapHtmlLoaded;
    private bool _isMapPageReady;
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
        _trackingMapWebView.Navigated += TrackingMapWebView_Navigated;

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

        if (!_isMapHtmlLoaded)
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

    private void TrackingMapWebView_Navigated(object? sender, WebNavigatedEventArgs e)
    {
        _isMapPageReady = true;
        _ = PushPendingUpdateAsync();
    }

    private async Task LoadInitialMapAsync(string markersJson)
    {
        var html = await BuildLeafletHtmlAsync(markersJson);
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
            : BuildSetMarkersScript(_pendingMarkersJson ?? "[]");

        await InvokeTrackingMapScriptAsync(script);
    }

    private async Task InvokeTrackingMapScriptAsync(string script)
    {
        for (var attempt = 0; attempt < 5; attempt += 1)
        {
            var result = await MainThread.InvokeOnMainThreadAsync(
                () => _trackingMapWebView.EvaluateJavaScriptAsync(script));

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
            _trackingMapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        });
    }

    private static async Task<string> BuildLeafletHtmlAsync(string markersJson)
    {
        var template = await GetMapTemplateHtmlAsync();
        var trackingModuleUrl = await GetTrackingModuleUrlAsync();

        return template
            .Replace("__MARKERS_JSON__", markersJson, StringComparison.Ordinal)
            .Replace("__TRACKING_MAP_MODULE_URL__", trackingModuleUrl, StringComparison.Ordinal)
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

        return $"(() => {{ const bridge = window.trackingMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.setMarkersJson({quotedMarkersJson}); return 'ready'; }})()";
    }

    private static string BuildClearScript()
    {
        return "(() => { const bridge = window.trackingMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.clear(); return 'ready'; })()";
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

    private static async Task<string> GetTrackingModuleUrlAsync()
    {
        if (!string.IsNullOrWhiteSpace(_trackingModuleUrl))
        {
            return _trackingModuleUrl;
        }

        var mapCoreModuleUrl = CreateJavaScriptModuleUrl(await GetMapCoreScriptAsync());

        using var stream = await FileSystem.OpenAppPackageFileAsync(MapScriptFileName);
        using var reader = new StreamReader(stream);

        var mapScript = (await reader.ReadToEndAsync())
            .Replace("__MAP_CORE_MODULE_URL__", mapCoreModuleUrl, StringComparison.Ordinal);

        _trackingModuleUrl = CreateJavaScriptModuleUrl(mapScript);
        return _trackingModuleUrl;
    }

    private static string CreateJavaScriptModuleUrl(string script)
    {
        var bytes = Encoding.UTF8.GetBytes(script);
        return $"data:text/javascript;base64,{Convert.ToBase64String(bytes)}";
    }
}
