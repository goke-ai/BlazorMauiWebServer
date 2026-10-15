using Goke.Core.Map;
using Goke.Core.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Text.Json;

namespace GokeApp.Pages.Controls;

public partial class WeatherForecastMapListView : ContentView
{
    public static readonly BindableProperty ForecastsProperty =
        BindableProperty.Create(
            nameof(Forecasts),
            typeof(IEnumerable<WeatherForecast>),
            typeof(WeatherForecastMapListView),
            Enumerable.Empty<WeatherForecast>(),
            propertyChanged: OnForecastsChanged);

    public static readonly BindableProperty CanEditProperty =
        BindableProperty.Create(
            nameof(CanEdit),
            typeof(bool),
            typeof(WeatherForecastMapListView),
            false);

    private WeatherForecast[] _cityForecasts = [];
    private WeatherForecast[] _filteredForecasts = [];
    private CancellationTokenSource? _filterDebounceCts;
    private bool _suspendFilterRefresh;

    private int _absoluteMinTemperature;
    public int AbsoluteMinTemperature
    {
        get => _absoluteMinTemperature;
        set
        {
            if (_absoluteMinTemperature == value) return;
            _absoluteMinTemperature = value;
            OnPropertyChanged(nameof(AbsoluteMinTemperature));
            OnPropertyChanged(nameof(TemperatureRangeText));
        }
    }

    private int _absoluteMaxTemperature;
    public int AbsoluteMaxTemperature
    {
        get => _absoluteMaxTemperature;
        set
        {
            if (_absoluteMaxTemperature == value) return;
            _absoluteMaxTemperature = value;
            OnPropertyChanged(nameof(AbsoluteMaxTemperature));
            OnPropertyChanged(nameof(TemperatureRangeText));
        }
    }

    private int _selectedMaxTemperature;
    public int SelectedMaxTemperature
    {
        get => _selectedMaxTemperature;
        set
        {
            var clamped = Math.Clamp(value, AbsoluteMinTemperature, AbsoluteMaxTemperature);
            if (_selectedMaxTemperature == clamped) return;

            _selectedMaxTemperature = clamped;
            OnPropertyChanged(nameof(SelectedMaxTemperature));
            OnPropertyChanged(nameof(MaxTemperatureText));

            QueueFilterRefresh();
        }
    }

    private int _selectedRangeMinTemperature;
    public int SelectedRangeMinTemperature
    {
        get => _selectedRangeMinTemperature;
        set
        {
            var clamped = Math.Clamp(value, AbsoluteMinTemperature, SelectedRangeMaxTemperature);
            if (_selectedRangeMinTemperature == clamped) return;

            _selectedRangeMinTemperature = clamped;
            OnPropertyChanged(nameof(SelectedRangeMinTemperature));
            OnPropertyChanged(nameof(SelectedRangeText));

            QueueFilterRefresh();
        }
    }

    private int _selectedRangeMaxTemperature;
    public int SelectedRangeMaxTemperature
    {
        get => _selectedRangeMaxTemperature;
        set
        {
            var clamped = Math.Clamp(value, SelectedRangeMinTemperature, AbsoluteMaxTemperature);
            if (_selectedRangeMaxTemperature == clamped) return;

            _selectedRangeMaxTemperature = clamped;
            OnPropertyChanged(nameof(SelectedRangeMaxTemperature));
            OnPropertyChanged(nameof(SelectedRangeText));

            QueueFilterRefresh();
        }
    }

    private bool _filtersInitialized;

    public string MaxTemperatureText => $"Show temperatures up to: {SelectedMaxTemperature}°C";
    public string TemperatureRangeText => $"Temperature range: {AbsoluteMinTemperature}°C to {AbsoluteMaxTemperature}°C";
    public string SelectedRangeText => $"Selected range: {SelectedRangeMinTemperature}°C to {SelectedRangeMaxTemperature}°C";
    public string VisibleCitiesText => $"{_filteredForecasts.Length} city / {_cityForecasts.Length} cities visible";

    public WeatherForecastMapListView()
    {
        InitializeComponent();
    }

    public IEnumerable<WeatherForecast> Forecasts
    {
        get => (IEnumerable<WeatherForecast>)GetValue(ForecastsProperty);
        set => SetValue(ForecastsProperty, value);
    }

    public bool CanEdit
    {
        get => (bool)GetValue(CanEditProperty);
        set => SetValue(CanEditProperty, value);
    }

    public WeatherForecast[] FilteredForecasts => _filteredForecasts;
    public MapMarker[] FilteredMarkers => BuildMarkersJson(_filteredForecasts);
    private static MapMarker[] BuildMarkersJson(WeatherForecast[] forecasts)
    {
        var mapMarkers = forecasts
            .Select((forecast, index) => new MapMarker
            {
                Id = index.ToString(),
                Name = $"{forecast.TemperatureC}&deg; C",
                Latitude = forecast.Latitude,
                Longitude = forecast.Longitude,
                Description = $"{forecast.City} | {forecast.TemperatureC}&deg; C",
                //Timestamp = forecast.Timestamp,
                PopupHtml = $"<strong>{forecast.City}</strong><br />" +
                            $"Temperature: {forecast.TemperatureC}&deg;C<br />" +
                            $"Summary: {forecast.Summary}<br />" +
                            $"Time: {forecast.Date}",
                //MarkerColor = "#1f2937",
                //MarkerShape = forecast.MarkerShape,
                //MarkerInitials = forecast.MarkerInitials,
                //MarkerOpacity = 0.55,
                MarkerValue = forecast.TemperatureC

            })
            .ToArray();

        return mapMarkers;
    }

    private static void OnForecastsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((WeatherForecastMapListView)bindable).RefreshView();
    }

    private void RefreshView()
    {
        _cityForecasts = (Forecasts ?? [])
            .GroupBy(f => f.City, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(f => f.Date).First())
            .OrderBy(f => f.City)
            .ToArray();

        if (_cityForecasts.Length == 0)
        {
            _filteredForecasts = [];
            OnPropertyChanged(nameof(FilteredForecasts));
            OnPropertyChanged(nameof(VisibleCitiesText));
            return;
        }

        var minTemperature = _cityForecasts.Min(f => f.TemperatureC);
        var maxTemperature = _cityForecasts.Max(f => f.TemperatureC);

        _suspendFilterRefresh = true;
        try
        {
            if (!_filtersInitialized ||
                minTemperature != AbsoluteMinTemperature ||
                maxTemperature != AbsoluteMaxTemperature)
            {
                AbsoluteMinTemperature = minTemperature;
                AbsoluteMaxTemperature = maxTemperature;
                SelectedMaxTemperature = maxTemperature;
                SelectedRangeMinTemperature = minTemperature;
                SelectedRangeMaxTemperature = maxTemperature;
                _filtersInitialized = true;
            }

            SelectedMaxTemperature = Math.Clamp(
                SelectedMaxTemperature,
                AbsoluteMinTemperature,
                AbsoluteMaxTemperature);

            SelectedRangeMinTemperature = Math.Clamp(
                SelectedRangeMinTemperature,
                AbsoluteMinTemperature,
                SelectedRangeMaxTemperature);

            SelectedRangeMaxTemperature = Math.Clamp(
                SelectedRangeMaxTemperature,
                SelectedRangeMinTemperature,
                AbsoluteMaxTemperature);
        }
        finally
        {
            _suspendFilterRefresh = false;
        }

        ApplyFilters();
    }

    private void QueueFilterRefresh()
    {
        if (_suspendFilterRefresh)
        {
            return;
        }

        _filterDebounceCts?.Cancel();
        _filterDebounceCts?.Dispose();
        _filterDebounceCts = new CancellationTokenSource();
        var token = _filterDebounceCts.Token;

        _ = DebounceApplyFiltersAsync(token);
    }

    private async Task DebounceApplyFiltersAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(125, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(ApplyFilters);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ApplyFilters()
    {
        _filteredForecasts = _cityForecasts
            .Where(f =>
                f.TemperatureC <= SelectedMaxTemperature &&
                f.TemperatureC >= SelectedRangeMinTemperature &&
                f.TemperatureC <= SelectedRangeMaxTemperature)
            .ToArray();


        OnPropertyChanged(nameof(FilteredForecasts));
        OnPropertyChanged(nameof(FilteredMarkers));
        OnPropertyChanged(nameof(VisibleCitiesText));
    }

    private void Slider_DragCompleted(object sender, EventArgs e)
    {
        //QueueFilterRefresh();
    }
}