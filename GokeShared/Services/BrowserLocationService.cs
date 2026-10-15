using Goke.Core.Map;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace GokeShared.Services
{
    public class BrowserLocationService(IJSRuntime JS) : IAsyncDisposable
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
                var location = await _mapModule.InvokeAsync<Location?>("getCurrentLocation");
                return location;
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
    }

}
