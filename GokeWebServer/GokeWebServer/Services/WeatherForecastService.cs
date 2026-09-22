using Goke.Core.Engines;
using Goke.Core.Models;

namespace GokeWebServer.Services;

public sealed class WeatherForecastService : IWeatherForecastService
{
    public Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts()
    {
        var forecastWindowStart = WeatherForecastEngine.GetForecastWindowStart(DateTime.UtcNow);
        var forecasts = WeatherForecastEngine.BuildForecasts(forecastWindowStart, WeatherForecastEngine.ForecastPeriods);

        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetAllWeatherForecastsByCity(params string[] cities)
    {
        var forecastWindowStart = WeatherForecastEngine.GetForecastWindowStart(DateTime.UtcNow);
        var forecasts = WeatherForecastEngine.BuildForecasts(forecastWindowStart, WeatherForecastEngine.ForecastPeriods, cities);

        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateRange(DateTime start, DateTime end)
    {
        if (end < start)
        {
            return Task.FromResult<IEnumerable<WeatherForecast>>(Array.Empty<WeatherForecast>());
        }
        var utcStart = start.ToUniversalTime();
        var utcEnd = end.ToUniversalTime();
        var alignedStart = WeatherForecastEngine.AlignToForecastBoundary(utcStart);
        var alignedEnd = WeatherForecastEngine.AlignToForecastBoundary(utcEnd);
        var periods = (int)((alignedEnd - alignedStart).Ticks / WeatherForecastEngine.ForecastInterval.Ticks) + 1;
        var forecasts = WeatherForecastEngine.BuildForecasts(alignedStart, periods)
            .Where(forecast => forecast.Date >= utcStart && forecast.Date <= utcEnd)
            .ToArray();
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateRangeByCity(DateTime start, DateTime end, params string[] cities)
    {
        if (end < start)
        {
            return Task.FromResult<IEnumerable<WeatherForecast>>(Array.Empty<WeatherForecast>());
        }
        var utcStart = start.ToUniversalTime();
        var utcEnd = end.ToUniversalTime();
        var alignedStart = WeatherForecastEngine.AlignToForecastBoundary(utcStart);
        var alignedEnd = WeatherForecastEngine.AlignToForecastBoundary(utcEnd);
        var periods = (int)((alignedEnd - alignedStart).Ticks / WeatherForecastEngine.ForecastInterval.Ticks) + 1;
        var forecasts = WeatherForecastEngine.BuildForecasts(alignedStart, periods, cities)
            .Where(forecast => forecast.Date >= utcStart && forecast.Date <= utcEnd)
            .ToArray();
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDate(DateTime date)
    {
        var utcDate = date.ToUniversalTime();
        var alignedDate = WeatherForecastEngine.GetForecastWindowStart(utcDate);
        var forecasts = WeatherForecastEngine.BuildForecasts(alignedDate, 24);
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateByCity(DateTime date, params string[] cities)
    {
        var utcDate = date.ToUniversalTime();
        var alignedDate = WeatherForecastEngine.GetForecastWindowStart(utcDate);
        var forecasts = WeatherForecastEngine.BuildForecasts(alignedDate, 24, cities);
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateTime(DateTime dateTime)
    {
        var utcDateTime = dateTime.ToUniversalTime();
        var alignedDateTime = WeatherForecastEngine.AlignToForecastBoundary(utcDateTime);
        var forecasts = WeatherForecastEngine.BuildForecasts(alignedDateTime, 1);
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }
    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsForDateTimeByCity(DateTime dateTime, params string[] cities)
    {
        var utcDateTime = dateTime.ToUniversalTime();
        var alignedDateTime = WeatherForecastEngine.AlignToForecastBoundary(utcDateTime);
        var forecasts = WeatherForecastEngine.BuildForecasts(alignedDateTime, 1, cities );
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }
}