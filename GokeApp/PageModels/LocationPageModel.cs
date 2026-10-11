using CommunityToolkit.Mvvm.ComponentModel;
using GokeApp.Services;
using Goke.Core.Models;
using Goke.Core.Services;
using System.Net.Http.Json;
using GokeApp.Models;

namespace GokeApp.PageModels
{
    public partial class LocationPageModel(ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
    {

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoLocations))]
        [NotifyPropertyChangedFor(nameof(Markers))]

        List<Goke.Core.Models.Location> _locations = [];

        List<Goke.Core.Models.Location> DATA = [];

        DateTime current = DateTime.UtcNow;

        public bool HasNoLocations => Locations.Count == 0;

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

            // Seed weather data here
            DATA =
            [
                new Goke.Core.Models.Location { Latitude = 6.530601541004869, Longitude = 3.2792890540792294, Name = "Femi Osobu" },
                new Goke.Core.Models.Location { Latitude = 1.5630831374667737, Longitude = 103.6168111428407, Name = "Jalan Pulai Perdana" },
                new Goke.Core.Models.Location { Latitude = 1.5671234137643146, Longitude = 103.61329262550467, Name = "Jalan Enau" },
                new Goke.Core.Models.Location { Latitude = 1.555975049606735, Longitude = 103.56783852970288, Name = "Jalan Pulasan" },
                new Goke.Core.Models.Location { Latitude = 11.813208059803452, Longitude = 13.205843552095427, Name = "UNIMAID" },
                new Goke.Core.Models.Location { Latitude = 51.74989074677224, Longitude = 0.504468455212419, Name = "Foxglove" },
                // current device location can be added here if needed
                new Goke.Core.Models.Location { Latitude = loc?.Latitude ?? 0, Longitude = loc?.Longitude ?? 0, Name = "Current Location" }

            ];
        }

        override protected async Task OnLoadDataAsync()
        {
            await base.OnLoadDataAsync();

            current = DateTime.UtcNow;

            // Simulate asynchronous loading to demonstrate a loading indicator
            await Task.Delay(500);

            //Locations = [.. DATA.Where(f => DateTime.Now.AddDays(-1) < f.Date && f.Date <= DateTime.Now.AddDays(1))];
            Locations = [.. DATA.OrderBy(f => f.Latitude)];

        }
    }

    
}
