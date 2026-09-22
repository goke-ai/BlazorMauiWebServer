using Goke.Core.Models;

namespace GokeWebServer.Services
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts();
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateRange(DateTime start, DateTime end);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDate(DateTime date);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateTime(DateTime dateTime);

        Task<IEnumerable<WeatherForecast>> GetAllWeatherForecastsByCity(params string[] cities);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateRangeByCity(DateTime start, DateTime end, params string[] cities);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateByCity(DateTime date, params string[] cities);
        Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateTimeByCity(DateTime dateTime, params string[] cities);

    }
}