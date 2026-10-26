using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Goke.Core.Map;
using Goke.Core.Services;
using GokeApp.Services;
using GokeSharedCore.MapApp;
using System.Net.Http.Json;

namespace GokeApp.PageModels
{
    public partial class TrackerPageModel(ModalErrorHandler errorHandler, ILocationService locationService, TrackerEngine trackerEngine) : BasePageModel(errorHandler)
    {
        private static readonly TimeSpan MovementUpdateInterval = TimeSpan.FromSeconds(1);
        private readonly Lock _movementSync = new();
        private readonly List<string> _activeTrackerNames = [];
        private Task? _movementTask;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoTrackers))]
        [NotifyPropertyChangedFor(nameof(Markers))]

        List<Goke.Core.Map.Location> _locations = [];

        List<Goke.Core.Map.Location> DATA = [];

        DateTime current = DateTime.UtcNow;

        public bool HasNoTrackers => Locations.Count == 0;

        public List<MapMarker> Markers => [.. Locations.Select(location => new MapMarker
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Name = location.Name,
            Description = $"Lat: {location.Latitude}, Lon: {location.Longitude}",
            MarkerShape = "Circle",
            MarkerColor = MapMarker.GetColor(location.Name)
        })];

        protected override async Task OnInitDataAsync()
        {
            await base.OnInitDataAsync();

            // Get the current device location
            var loc = await locationService.GetCurrentLocationAsync();

            // Seed tracker data here
            DATA =
            [
                new Goke.Core.Map.Location { Latitude = 6.530601541004869, Longitude = 3.2792890540792294, Name = "Joseph" },
                new Goke.Core.Map.Location { Latitude = 51.74989074677224, Longitude = 0.504468455212419, Name = "Foxglove" },
                // current device location can be added here if needed
                new Goke.Core.Map.Location { Latitude = loc?.Latitude ?? 0, Longitude = loc?.Longitude ?? 0, Name = "Me" },

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
            List<Goke.Core.Map.Location> trackers;

            lock (_movementSync)
            {
                trackers =
                [
                    .. _activeTrackerNames
                        .Select(GetSeedLocation)
                        .Where(location => location is not null)
                        .Select(location => trackerEngine.BuildSimulatedLocation(location!, timestamp))
                ];
            }

            Locations = trackers;
        }

        private Goke.Core.Map.Location? GetSeedLocation(string trackerName)
        {
            return DATA.FirstOrDefault(location => string.Equals(location.Name, trackerName, StringComparison.Ordinal));
        }        
    }    
}
