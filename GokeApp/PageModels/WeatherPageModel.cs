using CommunityToolkit.Mvvm.ComponentModel;
using GokeApp.Services;
using Goke.Core.Models;
using Goke.Core.Services;
using System.Net.Http.Json;

namespace GokeApp.PageModels
{
    public partial class WeatherPageModel(IHttpClientFactory httpClientFactory,
        ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient(BackendApiEndpoints.ClientName);
        public bool HasNoForecasts => Forecasts.Count == 0;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoForecasts))]

        List<WeatherForecast> _forecasts = new List<WeatherForecast>();

        List<WeatherForecast> DATA = new List<WeatherForecast>();

        DateTime current = DateTime.UtcNow;

        protected override async Task OnInitDataAsync()
        {
            await base.OnInitDataAsync();

            // Seed weather data here
            DATA = await httpClient.GetFromJsonAsync<List<WeatherForecast>>("api/weather") ?? [];
        }

        override protected async Task OnLoadDataAsync()
        {
            await base.OnLoadDataAsync();

            current = DateTime.UtcNow;

            // Simulate asynchronous loading to demonstrate a loading indicator
            await Task.Delay(500);

            Forecasts = [.. DATA.Where(f => DateTime.Now.AddMinutes(-1) < f.Date && f.Date <= DateTime.Now.AddMinutes(1))];

        }




    }

    
}
