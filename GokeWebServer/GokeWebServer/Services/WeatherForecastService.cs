using Goke.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GokeWebServer.Services;

public sealed class WeatherForecastService : IWeatherForecastService
{
    private const int ForecastWindowsPerDay = 4;
    private const int ForecastDays = 14;
    private const int TemperatureSeedSalt = 0;
    private const int SummarySeedSalt = 1;

    private static readonly TimeSpan ForecastInterval = TimeSpan.FromHours(24 / ForecastWindowsPerDay);
    private static readonly int ForecastPeriods = ForecastDays * ForecastWindowsPerDay;

    private static readonly CityWeatherProfile[] Cities =
    [
        new("London", 8, 18,
        [
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Windy", "💨"),
            new("Cool", "🧥"),
            new("Overcast", "🌥️")
        ]),
        new("Lagos", 25, 33,
        [
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Thunderstorms", "⛈️"),
            new("Partly Cloudy", "⛅"),
            new("Sunny", "☀️")
        ]),
        new("Paris", 10, 22,
        [
            new("Mild", "🙂"),
            new("Cloudy", "☁️"),
            new("Sunny", "☀️"),
            new("Breezy", "🍃"),
            new("Light Rain", "🌦️")
        ]),
        new("Kuala Lumpur", 24, 32,
        [
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Thunderstorms", "⛈️"),
            new("Rainy", "🌧️"),
            new("Partly Cloudy", "⛅")
        ]),
        new("New York", 6, 24,
        [
            new("Cool", "🧥"),
            new("Sunny", "☀️"),
            new("Cloudy", "☁️"),
            new("Windy", "💨"),
            new("Showers", "🚿")
        ])
    ];

    public Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts()
    {
        var forecastWindowStart = GetForecastWindowStart(DateTime.UtcNow);
        
        var forecasts = BuildForecasts(forecastWindowStart, ForecastPeriods);
        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsInRange(DateTime start, DateTime end)
    {
        if (end < start)
        {
            return Task.FromResult<IEnumerable<WeatherForecast>>([]);
        }

        var utcStart = start.ToUniversalTime();
        var utcEnd = end.ToUniversalTime();

        var alignedStart = AlignToForecastBoundary(utcStart);
        var alignedEnd = AlignToForecastBoundary(utcEnd);
        var periods = (int)((alignedEnd - alignedStart).Ticks / ForecastInterval.Ticks) + 1;

        var forecasts = BuildForecasts(alignedStart, periods)
            .Where(forecast => forecast.Date >= utcStart && forecast.Date <= utcEnd)
            .ToArray();

        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }

    private static WeatherForecast[] BuildForecasts(DateTime startDateUtc, int periods) =>
        Cities.SelectMany(city =>
                Enumerable.Range(0, periods)
                    .Select(index => CreateForecast(city, startDateUtc.AddTicks(ForecastInterval.Ticks * index))))
            .ToArray();

    private static WeatherForecast CreateForecast(CityWeatherProfile city, DateTime dateUtc)
    {
        var summary = GetDeterministicSummary(city, dateUtc);

        return new WeatherForecast
        {
            City = city.City,
            Date = dateUtc,
            TemperatureC = GetDeterministicTemperature(city, dateUtc),
            Summary = summary.Text,
            SummaryIcon = summary.Icon
        };
    }

    private static DateTime GetForecastWindowStart(DateTime currentUtc) =>
        new(currentUtc.Year, currentUtc.Month, currentUtc.Day, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime AlignToForecastBoundary(DateTime utcDateTime)
    {
        var ticks = utcDateTime.Ticks - (utcDateTime.Ticks % ForecastInterval.Ticks);
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    private static int GetDeterministicTemperature(CityWeatherProfile city, DateTime dateUtc)
    {
        var seasonalAdjustment = GetSeasonAdjustment(city.City, dateUtc.Month);
        var minimumTemperature = city.MinTempC + seasonalAdjustment;
        var exclusiveMaximumTemperature = city.MaxTempC + seasonalAdjustment + 1;

        var random = new Random(GetDeterministicSeed(city.City, dateUtc, TemperatureSeedSalt));
        return random.Next(minimumTemperature, exclusiveMaximumTemperature);
    }

    private static WeatherSummary GetDeterministicSummary(CityWeatherProfile city, DateTime dateUtc)
    {
        var random = new Random(GetDeterministicSeed(city.City, dateUtc, SummarySeedSalt));
        return city.Summaries[random.Next(city.Summaries.Length)];
    }

    private static int GetSeasonAdjustment(string city, int month) =>
        city switch
        {
            "London" => month is 12 or 1 or 2 ? -3 : month is >= 6 and <= 8 ? 4 : 0,
            "Paris" => month is 12 or 1 or 2 ? -4 : month is >= 6 and <= 8 ? 5 : 0,
            "New York" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 7 : 0,
            "Lagos" => month is >= 6 and <= 9 ? -1 : 1,
            "Kuala Lumpur" => 0,
            _ => 0
        };

    private static int GetDeterministicSeed(string city, DateTime dateUtc, int salt)
    {
        unchecked
        {
            var hash = 17;

            foreach (var character in city)
            {
                hash = (hash * 31) + character;
            }

            hash = (hash * 31) + dateUtc.Year;
            hash = (hash * 31) + dateUtc.Month;
            hash = (hash * 31) + dateUtc.Day;
            hash = (hash * 31) + dateUtc.Hour;
            hash = (hash * 31) + salt;

            return hash;
        }
    }

    private sealed record CityWeatherProfile(
        string City,
        int MinTempC,
        int MaxTempC,
        WeatherSummary[] Summaries);

    private sealed record WeatherSummary(string Text, string Icon);
}