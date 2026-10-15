using Goke.Core.Map;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace GokeShared.Services
{
    public class BrowserLocationService(IJSRuntime JS) : IAsyncDisposable, ILocationService
    {
        private IJSObjectReference? _mapModule;

        private async Task InitializeAsync()
        {
            _mapModule ??= await JS.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/GokeShared/js/location-service.js");
        }

        public async Task<Location?> GetCurrentLocationAsync()
        {
            await InitializeAsync();

            if (_mapModule is null)
            {
                return null;
            }

            try
            {
                BrowserLocationSnapshot location = await _mapModule.InvokeAsync<BrowserLocationSnapshot>("getCurrentLocation");
                return new Location
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Altitude = location.Altitude,
                    Accuracy = location.Accuracy,
                    VerticalAccuracy = location.VerticalAccuracy,
                    ReducedAccuracy = location.ReducedAccuracy,
                    Speed = location.Speed,
                    Heading = location.Heading,
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(location.Timestamp)
                };
            }
            catch (JSException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_mapModule is null)
            {
                return;
            }

            try
            {
                await _mapModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        private class BrowserLocationSnapshot
        {
            public double Latitude { get; set; }

            public double Longitude { get; set; }

            public double? Altitude { get; set; }

            public double? Accuracy { get; set; }

            public double? VerticalAccuracy { get; set; }

            public bool ReducedAccuracy { get; set; }

            public double? Speed { get; set; }

            public double? Heading { get; set; }

            public long Timestamp { get; set; }

        }
    }

}
