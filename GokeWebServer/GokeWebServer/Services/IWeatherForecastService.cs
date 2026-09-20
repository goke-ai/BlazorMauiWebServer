using Goke.Core.Models;

namespace GokeWebServer.Services
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts();
    }
}