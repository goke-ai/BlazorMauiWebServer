using Goke.Core.Models.Map;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public sealed class DirectionMapView : ContentView
{
    private const string MapCoreScriptFileName = "map-core.js";
    private const string MapTemplateFileName = "map-view-template.html";
    private const string MapScriptFileName = "direction-map.js";

    private readonly Label _visibleRoutesLabel;
    private readonly Label _emptyStateLabel;
    private readonly WebView _directionMapWebView;
    private readonly MapWebViewHost _mapWebViewHost;
    private string? _pendingRoutesJson;
    private bool _pendingClear;

    public static readonly BindableProperty RoutesProperty =
        BindableProperty.Create(
            nameof(Routes),
            typeof(IEnumerable<DirectionRouteMarker>),
            typeof(DirectionMapView),
            Enumerable.Empty<DirectionRouteMarker>(),
            propertyChanged: OnRoutesChanged);

    public DirectionMapView()
    {
        _visibleRoutesLabel = new Label
        {
            FontSize = 12,
            Opacity = 0.7
        };

        _emptyStateLabel = new Label
        {
            Text = "No direction routes available.",
            IsVisible = false
        };

        _directionMapWebView = new WebView();
        _mapWebViewHost = new MapWebViewHost(_directionMapWebView, PushPendingUpdateAsync);

        var border = new Border
        {
            HeightRequest = 520,
            Stroke = new SolidColorBrush(Color.FromArgb("#DDDDDD")),
            Content = _directionMapWebView
        };

        Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                _visibleRoutesLabel,
                _emptyStateLabel,
                border
            }
        };

        UpdateVisibleRoutesText();
    }

    public IEnumerable<DirectionRouteMarker> Routes
    {
        get => (IEnumerable<DirectionRouteMarker>)GetValue(RoutesProperty);
        set => SetValue(RoutesProperty, value);
    }

    private static void OnRoutesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        _ = ((DirectionMapView)bindable).RefreshViewAsync();
    }

    private async Task RefreshViewAsync()
    {
        var routes = (Routes ?? []).ToArray();
        var routesJson = BuildRoutesJson(routes);

        _emptyStateLabel.IsVisible = routes.Length == 0;
        UpdateVisibleRoutesText();
        _pendingRoutesJson = routesJson;
        _pendingClear = routes.Length == 0;

        if (!_mapWebViewHost.IsMapHtmlLoaded)
        {
            if (routes.Length == 0)
            {
                return;
            }

            await LoadInitialMapAsync(routesJson);
            return;
        }

        await PushPendingUpdateAsync();
    }

    private void UpdateVisibleRoutesText()
    {
        _visibleRoutesLabel.Text = $"{(Routes ?? []).Count()} route marker(s) visible";
    }

    private async Task LoadInitialMapAsync(string routesJson)
    {
        var html = await BuildLeafletHtmlAsync(routesJson);
        await _mapWebViewHost.LoadHtmlAsync(html);
    }

    private async Task PushPendingUpdateAsync()
    {
        var script = _pendingClear
            ? BuildClearScript()
            : BuildSetRoutesScript(_pendingRoutesJson ?? "[]");

        await _mapWebViewHost.InvokeWhenReadyAsync(script);
    }

    private static async Task<string> BuildLeafletHtmlAsync(string routesJson)
    {
        var template = await MapWebViewAssetCache.GetTemplateHtmlAsync(MapTemplateFileName);
        var directionModuleUrl = await MapWebViewAssetCache.GetModuleUrlAsync(MapCoreScriptFileName, MapScriptFileName);

        return template
            .Replace("__MAP_CONFIG_NAME__", "directionMapConfig", StringComparison.Ordinal)
            .Replace("__MAP_BRIDGE_NAME__", "directionMapBridge", StringComparison.Ordinal)
            .Replace("__MAP_DATA_PROPERTY__", "routes", StringComparison.Ordinal)
            .Replace("__MAP_DATA_JSON__", routesJson, StringComparison.Ordinal)
            .Replace("__MAP_MODULE_URL__", directionModuleUrl, StringComparison.Ordinal)
            .Replace("__MAP_SET_EXPORT__", "setRoutesJson", StringComparison.Ordinal)
            .Replace("__MAP_CLEAR_EXPORT__", "clearRoutes", StringComparison.Ordinal)
            .Replace("__MAP_SET_METHOD__", "setRoutesJson", StringComparison.Ordinal)
            .Replace("__EMPTY_MESSAGE__", "No direction routes available.", StringComparison.Ordinal)
            .Replace("__TILE_WARNING_MESSAGE__", "Map tiles are unavailable right now. Route markers are still shown.", StringComparison.Ordinal);
    }

    private static string BuildRoutesJson(DirectionRouteMarker[] routes)
    {
        var mapRoutes = routes
            .Select(route => new
            {
                id = route.Id,
                name = route.Name,
                transportMode = route.TransportMode,
                startLatitude = route.StartLatitude,
                startLongitude = route.StartLongitude,
                endLatitude = route.EndLatitude,
                endLongitude = route.EndLongitude,
                currentLatitude = route.CurrentLatitude,
                currentLongitude = route.CurrentLongitude,
                progress = route.Progress,
                description = route.Description,
                timestamp = route.Timestamp,
                markerColor = route.MarkerColor,
                markerShape = route.MarkerShape,
                markerInitials = route.MarkerInitials,
                popupHtml = $"<strong>{route.Name}</strong><br/>{route.TransportMode}<br/>Progress: {Math.Round(route.Progress * 100d)}%<br/>Updated: {route.Timestamp}"
            })
            .ToArray();

        return JsonSerializer.Serialize(mapRoutes);
    }

    private static string BuildSetRoutesScript(string routesJson)
    {
        var quotedRoutesJson = JsonSerializer.Serialize(routesJson);
        return $"(() => {{ const bridge = window.directionMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.setRoutesJson({quotedRoutesJson}, window.directionMapConfig ?? {{}}); return 'ready'; }})()";
    }

    private static string BuildClearScript()
    {
        return "(() => { const bridge = window.directionMapBridge; if (!bridge || bridge.isReady !== true) return 'not-ready'; bridge.clear(); return 'ready'; })()";
    }
}
