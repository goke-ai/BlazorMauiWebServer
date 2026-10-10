using Goke.Core.Models;

namespace GokeWebServer.Services
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts();
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateRange(DateTime start, DateTime end);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDate(DateTime date);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateTime(DateTime dateTime);

        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForCities(params string[] cities);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateRangeAndCities(DateTime start, DateTime end, params string[] cities);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateAndCities(DateTime date, params string[] cities);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateTimeAndCities(DateTime dateTime, params string[] cities);

    }
}