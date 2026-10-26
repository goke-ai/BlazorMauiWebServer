using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Goke.Core.Map;
using Goke.Core.Services;
using GokeApp.Services;
using GokeSharedCore.MapApp;
using System.Net.Http.Json;

namespace GokeApp.PageModels
{
    public partial class LocationPageModel(ModalErrorHandler errorHandler, ILocationService locationService) : BasePageModel(errorHandler)
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoLocations))]
        [NotifyPropertyChangedFor(nameof(Markers))]

        List<Goke.Core.Map.Location> _locations = [];

        List<Goke.Core.Map.Location> DATA = [];

        DateTime current = DateTime.UtcNow;

        public bool HasNoLocations => Locations.Count == 0;

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
            var location = await locationService.GetCurrentLocationAsync();

            // Seed weather data here
            DATA =
            [
                new Goke.Core.Map.Location { Latitude = 6.530601541004869, Longitude = 3.2792890540792294, Name = "Femi Osobu" },
                new Goke.Core.Map.Location { Latitude = 1.5630831374667737, Longitude = 103.6168111428407, Name = "Pulai Perdana" },
                new Goke.Core.Map.Location { Latitude = 1.5671234137643146, Longitude = 103.61329262550467, Name = "Enau" },
                new Goke.Core.Map.Location { Latitude = 1.555975049606735, Longitude = 103.56783852970288, Name = "Pulasan" },
                new Goke.Core.Map.Location { Latitude = 11.813208059803452, Longitude = 13.205843552095427, Name = "UNIMAID" },
                new Goke.Core.Map.Location { Latitude = 51.74989074677224, Longitude = 0.504468455212419, Name = "Foxglove" },
                // current device location can be added here if needed
                new Goke.Core.Map.Location { Latitude = location?.Latitude ?? 0, Longitude = location?.Longitude ?? 0, Name = "Current Location" },
                .. MapEngines.LOCATIONS
            ];
        }

        override protected async Task OnLoadDataAsync()
        {
            await base.OnLoadDataAsync();

            current = DateTime.UtcNow;

            // Simulate asynchronous loading to demonstrate a loading indicator
            await Task.Delay(500);

            Locations = [.. DATA.Skip(Random.Shared.Next(DATA.Count)).Take(1)];
        }

        [RelayCommand]
        private void Add()
        {
            var availables = DATA.Where(f => Locations.Any(l => l.Name == f.Name) == false).ToArray();

            if (availables.Length != 0)
            {
                Locations = [.. Locations, availables[Random.Shared.Next(availables.Length)]];
            }
        }       
    }
}
