using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Goke.Core.Models.Map;
using Goke.Core.Services;
using GokeApp.Services;
using System.Net.Http.Json;

namespace GokeApp.PageModels
{
    public partial class TrackerPageModel(ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
    {
        private const double LatitudeDegreesPerMeter = 1d / 111_320d;
        private static readonly TimeSpan MovementUpdateInterval = TimeSpan.FromSeconds(1);
        private readonly Lock _movementSync = new();
        private readonly Dictionary<string, TrackerMotionState> _motionStates = new(StringComparer.Ordinal);
        private readonly List<string> _activeTrackerNames = [];
        private Task? _movementTask;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoTrackers))]
        [NotifyPropertyChangedFor(nameof(Markers))]

        List<Goke.Core.Models.Map.Location> _locations = [];

        List<Goke.Core.Models.Map.Location> DATA = [];

        DateTime current = DateTime.UtcNow;

        public bool HasNoTrackers => Locations.Count == 0;

        public List<MapMarker> Markers => [.. Locations.Select(location => new MapMarker
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Name = location.Name,
            MarkerShape = "Circle",
        })];

        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;

        public async Task<Microsoft.Maui.Devices.Sensors.Location?> GetCurrentLocation()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                _cancelTokenSource = new CancellationTokenSource();

                var location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                return location;
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                // Unable to get location
            }
            finally
            {
                _isCheckingLocation = false;
            }
            return null;
        }

        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }

        protected override async Task OnInitDataAsync()
        {
            await base.OnInitDataAsync();

            // Get the current device location
            var loc = await GetCurrentLocation();

            // Seed tracker data here
            DATA =
            [
                new Goke.Core.Models.Location { Latitude = 6.530601541004869, Longitude = 3.2792890540792294, Name = "Joseph" },
                new Goke.Core.Models.Location { Latitude = 51.74989074677224, Longitude = 0.504468455212419, Name = "Foxglove" },
                // current device location can be added here if needed
                new Goke.Core.Models.Location { Latitude = loc?.Latitude ?? 0, Longitude = loc?.Longitude ?? 0, Name = "Me" },

            ];

            EnsureMovementSimulationStarted();
        }

        override protected async Task OnLoadDataAsync()
        {
            await base.OnLoadDataAsync();

            current = DateTime.UtcNow;

            // Simulate asynchronous loading to demonstrate a loading indicator
            await Task.Delay(500);

            //Locations = [.. DATA.Where(f => DateTime.Now.AddDays(-1) < f.Date && f.Date <= DateTime.Now.AddDays(1))];
            lock (_movementSync)
            {
                _activeTrackerNames.Clear();
                _activeTrackerNames.AddRange(DATA.Skip(Random.Shared.Next(DATA.Count)).Take(1).Select(location => location.Name));
            }

            UpdateLocationsSnapshot(current);

        }


        [RelayCommand]
        private void Add()
        {
            string[] activeTrackerNames;
            lock (_movementSync)
            {
                activeTrackerNames = [.. _activeTrackerNames];
            }

            var availables = DATA.Where(f => activeTrackerNames.Contains(f.Name) == false).ToArray();

            if (availables.Length != 0)
            {
                var selectedTracker = availables[Random.Shared.Next(availables.Length)];

                lock (_movementSync)
                {
                    if (_activeTrackerNames.Contains(selectedTracker.Name) == false)
                    {
                        _activeTrackerNames.Add(selectedTracker.Name);
                    }
                }

                UpdateLocationsSnapshot(DateTime.UtcNow);
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
                    await MainThread.InvokeOnMainThreadAsync(() => UpdateLocationsSnapshot(DateTime.UtcNow));
                }
            });
        }

        private void UpdateLocationsSnapshot(DateTime timestamp)
        {
            List<Goke.Core.Models.Map.Location> trackers;

            lock (_movementSync)
            {
                trackers =
                [
                    .. _activeTrackerNames
                        .Select(GetSeedLocation)
                        .Where(location => location is not null)
                        .Select(location => BuildSimulatedLocation(location!, timestamp))
                ];
            }

            Locations = trackers;
        }

        private Goke.Core.Models.Map.Location? GetSeedLocation(string trackerName)
        {
            return DATA.FirstOrDefault(location => string.Equals(location.Name, trackerName, StringComparison.Ordinal));
        }

        private Goke.Core.Models.Map.Location BuildSimulatedLocation(Goke.Core.Models.Map.Location seedLocation, DateTime timestamp)
        {
            var motionState = GetOrCreateMotionState(seedLocation, timestamp);
            var elapsedSeconds = Math.Max(0, (timestamp - motionState.StartedAtUtc).TotalSeconds);
            var routePosition = elapsedSeconds / motionState.SecondsPerSegment;
            var currentSegment = ((int)Math.Floor(routePosition)) % motionState.Waypoints.Length;
            var nextSegment = (currentSegment + 1) % motionState.Waypoints.Length;
            var segmentProgress = routePosition - Math.Floor(routePosition);
            var currentWaypoint = motionState.Waypoints[currentSegment];
            var nextWaypoint = motionState.Waypoints[nextSegment];
            var latitudeOffsetMeters = Lerp(currentWaypoint.LatitudeOffsetMeters, nextWaypoint.LatitudeOffsetMeters, segmentProgress);
            var longitudeOffsetMeters = Lerp(currentWaypoint.LongitudeOffsetMeters, nextWaypoint.LongitudeOffsetMeters, segmentProgress);
            var latitudeDegrees = latitudeOffsetMeters * LatitudeDegreesPerMeter;
            var longitudeDegrees = longitudeOffsetMeters * GetLongitudeDegreesPerMeter(seedLocation.Latitude);

            return new Goke.Core.Models.Location
            {
                Name = seedLocation.Name,
                Latitude = seedLocation.Latitude + latitudeDegrees,
                Longitude = seedLocation.Longitude + longitudeDegrees
            };
        }

        private TrackerMotionState GetOrCreateMotionState(Goke.Core.Models.Map.Location seedLocation, DateTime startedAtUtc)
        {
            if (_motionStates.TryGetValue(seedLocation.Name, out var existingState))
            {
                return existingState;
            }

            var hash = Math.Abs(seedLocation.Name.GetHashCode(StringComparison.Ordinal));
            var movementState = new TrackerMotionState(
                startedAtUtc,
                SecondsPerSegment: 4 + (hash % 3),
                Waypoints: CreateRouteWaypoints(hash));

            _motionStates[seedLocation.Name] = movementState;
            return movementState;
        }

        private static RouteWaypoint[] CreateRouteWaypoints(int hash)
        {
            var heading = DegreesToRadians(hash % 360);
            var latitudeOffsetMeters = 0d;
            var longitudeOffsetMeters = 0d;
            var waypoints = new List<RouteWaypoint>
            {
                new(0, 0)
            };

            for (var index = 0; index < 5; index += 1)
            {
                var turnDirection = index % 2 == 0 ? 1 : -1;
                var turnDegrees = 35 + ((hash / ((index + 1) * 11)) % 55);
                var segmentLengthMeters = 18 + ((hash / ((index + 1) * 5)) % 45);

                heading += DegreesToRadians(turnDegrees * turnDirection);
                latitudeOffsetMeters += Math.Sin(heading) * segmentLengthMeters;
                longitudeOffsetMeters += Math.Cos(heading) * segmentLengthMeters;
                waypoints.Add(new(latitudeOffsetMeters, longitudeOffsetMeters));
            }

            return [.. waypoints];
        }

        private static double Lerp(double start, double end, double progress)
        {
            return start + ((end - start) * progress);
        }

        private static double DegreesToRadians(int degrees) => degrees * (Math.PI / 180d);

        private static double GetLongitudeDegreesPerMeter(double latitude)
        {
            var latitudeRadians = latitude * (Math.PI / 180d);
            var cosine = Math.Cos(latitudeRadians);

            if (Math.Abs(cosine) < 0.01)
            {
                cosine = cosine < 0 ? -0.01 : 0.01;
            }

            return LatitudeDegreesPerMeter / cosine;
        }

        private sealed record TrackerMotionState(
            DateTime StartedAtUtc,
            double SecondsPerSegment,
            RouteWaypoint[] Waypoints);

        private sealed record RouteWaypoint(
            double LatitudeOffsetMeters,
            double LongitudeOffsetMeters);
    }

    
}
