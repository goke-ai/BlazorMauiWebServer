using CommunityToolkit.Mvvm.ComponentModel;
using Goke.Core.Models.Map;
using GokeApp.Services;

namespace GokeApp.PageModels;

public partial class DirectionPageModel(ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
{
    private static readonly TimeSpan MovementUpdateInterval = TimeSpan.FromSeconds(1);
    private readonly Lock _routeSync = new();
    private readonly Dictionary<string, DirectionRouteSeed> _routeSeeds = new(StringComparer.Ordinal);
    private Task? _movementTask;
    private CancellationTokenSource? _cancelTokenSource;
    private bool _isCheckingLocation;

    private List<DirectionRouteMarker> _routes = [];

    public List<DirectionRouteMarker> Routes
    {
        get => _routes;
        private set
        {
            if (SetProperty(ref _routes, value))
            {
                OnPropertyChanged(nameof(HasNoRoutes));
            }
        }
    }

    public bool HasNoRoutes => _routes.Count == 0;

    protected override async Task OnInitDataAsync()
    {
        await base.OnInitDataAsync();

        var location = await GetCurrentLocationAsync();
        var centerLatitude = location?.Latitude ?? 6.5244;
        var centerLongitude = location?.Longitude ?? 3.3792;

        lock (_routeSync)
        {
            _routeSeeds.Clear();

            foreach (var seed in CreateSeeds(centerLatitude, centerLongitude))
            {
                _routeSeeds[seed.Id] = seed;
            }
        }

        EnsureMovementSimulationStarted();
    }

    protected override async Task OnLoadDataAsync()
    {
        await base.OnLoadDataAsync();
        await Task.Delay(250);
        UpdateRoutesSnapshot(DateTime.UtcNow);
    }

    private async Task<Microsoft.Maui.Devices.Sensors.Location?> GetCurrentLocationAsync()
    {
        try
        {
            _isCheckingLocation = true;
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            _cancelTokenSource = new CancellationTokenSource();
            return await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
        }
        catch
        {
            return null;
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }

    private void EnsureMovementSimulationStarted()
    {
        if (_movementTask is not null)
        {
            return;
        }

        _movementTask = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(MovementUpdateInterval);

            while (await timer.WaitForNextTickAsync())
            {
                await MainThread.InvokeOnMainThreadAsync(() => UpdateRoutesSnapshot(DateTime.UtcNow));
            }
        });
    }

    private void UpdateRoutesSnapshot(DateTime timestamp)
    {
        DirectionRouteSeed[] seeds;
        lock (_routeSync)
        {
            seeds = [.. _routeSeeds.Values];
        }

        Routes = [.. seeds.Select(seed => BuildRouteMarker(seed, timestamp))];
    }

    private static DirectionRouteMarker BuildRouteMarker(DirectionRouteSeed seed, DateTime timestamp)
    {
        var elapsedSeconds = Math.Max(0, (timestamp - seed.StartedAtUtc).TotalSeconds) + seed.StartOffsetSeconds;
        var phase = elapsedSeconds % (seed.SecondsPerLeg * 2d);
        var forward = phase <= seed.SecondsPerLeg;
        var legProgress = forward
            ? phase / seed.SecondsPerLeg
            : 1d - ((phase - seed.SecondsPerLeg) / seed.SecondsPerLeg);

        var currentLatitude = Lerp(seed.StartLatitude, seed.EndLatitude, legProgress);
        var currentLongitude = Lerp(seed.StartLongitude, seed.EndLongitude, legProgress);
        var percent = (int)Math.Round(legProgress * 100d);
        var directionText = forward ? "toward destination" : "returning";

        return new DirectionRouteMarker
        {
            Id = seed.Id,
            Name = seed.Name,
            TransportMode = seed.TransportMode,
            StartLatitude = seed.StartLatitude,
            StartLongitude = seed.StartLongitude,
            EndLatitude = seed.EndLatitude,
            EndLongitude = seed.EndLongitude,
            CurrentLatitude = currentLatitude,
            CurrentLongitude = currentLongitude,
            Progress = legProgress,
            Description = $"{seed.TransportMode} {directionText} ({percent}%)",
            Timestamp = timestamp.ToLocalTime().ToString("HH:mm:ss"),
            MarkerColor = seed.MarkerColor,
            MarkerShape = seed.MarkerShape,
            MarkerInitials = seed.MarkerInitials
        };
    }

    private static IEnumerable<DirectionRouteSeed> CreateSeeds(double latitude, double longitude)
    {
        return
        [
            CreateSeed("car-1", "Car A", "Car", latitude, longitude, 0.0000, 0.0000, 0.0180, 0.0260, 14, 0, "#2563eb", "Square", "C"),
            CreateSeed("car-2", "Car B", "Car", latitude, longitude, 0.0120, -0.0180, 0.0240, 0.0120, 16, 4, "#1d4ed8", "Square", "C"),
            CreateSeed("train-1", "Train A", "Train", latitude, longitude, -0.0300, -0.0120, 0.0320, -0.0120, 10, 2, "#7c3aed", "Diamond", "T"),
            CreateSeed("bus-1", "Bus A", "Bus", latitude, longitude, -0.0160, 0.0280, 0.0100, 0.0460, 18, 6, "#059669", "Hexagon", "B"),
            CreateSeed("bus-2", "Bus B", "Bus", latitude, longitude, 0.0080, -0.0340, -0.0180, -0.0040, 20, 9, "#0f766e", "Hexagon", "B"),
            CreateSeed("walk-1", "Walker", "Walk", latitude, longitude, -0.0060, 0.0060, 0.0040, 0.0140, 24, 12, "#ea580c", "Circle", "W")
        ];
    }

    private static DirectionRouteSeed CreateSeed(
        string id,
        string name,
        string transportMode,
        double latitude,
        double longitude,
        double startLatitudeOffset,
        double startLongitudeOffset,
        double endLatitudeOffset,
        double endLongitudeOffset,
        double secondsPerLeg,
        double startOffsetSeconds,
        string markerColor,
        string markerShape,
        string markerInitials)
    {
        return new DirectionRouteSeed(
            Id: id,
            Name: name,
            TransportMode: transportMode,
            StartLatitude: latitude + startLatitudeOffset,
            StartLongitude: longitude + startLongitudeOffset,
            EndLatitude: latitude + endLatitudeOffset,
            EndLongitude: longitude + endLongitudeOffset,
            SecondsPerLeg: secondsPerLeg,
            StartOffsetSeconds: startOffsetSeconds,
            MarkerColor: markerColor,
            MarkerShape: markerShape,
            MarkerInitials: markerInitials,
            StartedAtUtc: DateTime.UtcNow);
    }

    private static double Lerp(double start, double end, double progress)
    {
        return start + ((end - start) * progress);
    }

    private sealed record DirectionRouteSeed(
        string Id,
        string Name,
        string TransportMode,
        double StartLatitude,
        double StartLongitude,
        double EndLatitude,
        double EndLongitude,
        double SecondsPerLeg,
        double StartOffsetSeconds,
        string MarkerColor,
        string MarkerShape,
        string MarkerInitials,
        DateTime StartedAtUtc);
}
