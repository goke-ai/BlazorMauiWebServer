using Goke.Core.Models;

namespace GokeWebServer.Services;

public sealed class WeatherForecastService : IWeatherForecastService
{
    private const int ForecastWindowsPerDay = 6;
    private const int ForecastDays = 14;

    private const int TemperatureSeedSalt = 0;
    private const int SummarySeedSalt = 1;
    private const int HumiditySeedSalt = 2;
    private const int PressureSeedSalt = 3;
    private const int VisibilitySeedSalt = 4;
    private const int AirQualitySeedSalt = 5;
    private const int WindSeedSalt = 6;
    private const int UvSeedSalt = 7;
    private const int TemperatureRangeSeedSalt = 8;
    private const int RainProbabilitySeedSalt = 9;

    private static readonly TimeSpan ForecastInterval = TimeSpan.FromHours(24d / ForecastWindowsPerDay);
    private static readonly int ForecastPeriods = ForecastDays * ForecastWindowsPerDay;

    private static readonly CityWeatherProfile[] Cities =
    [
        new("London", 51.5074, -0.1278, 8, 18, 68, 90, 8, 22, 1008, 1026, 10, 28, 20, 55,
        [
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Windy", "💨"),
            new("Cool", "🧥"),
            new("Overcast", "🌥️")
        ]),
        new("Lagos", 6.5244, 3.3792, 25, 33, 72, 95, 6, 16, 1007, 1018, 8, 24, 55, 120,
        [
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Thunderstorms", "⛈️"),
            new("Partly Cloudy", "⛅"),
            new("Sunny", "☀️")
        ]),
        new("Abuja", 9.0765, 7.3986, 22, 34, 45, 82, 7, 22, 1006, 1018, 7, 20, 35, 95,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Thunderstorms", "⛈️"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Maiduguri", 11.8311, 13.1510, 24, 39, 20, 55, 8, 28, 1004, 1016, 8, 24, 40, 110,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Dry", "🏜️"),
            new("Windy", "💨"),
            new("Clear", "🌤️")
        ]),
        new("Paris", 48.8566, 2.3522, 10, 22, 60, 86, 8, 24, 1009, 1025, 7, 22, 18, 48,
        [
            new("Mild", "🙂"),
            new("Cloudy", "☁️"),
            new("Sunny", "☀️"),
            new("Breezy", "🍃"),
            new("Light Rain", "🌦️")
        ]),
        new("Kuala Lumpur", 3.1390, 101.6869, 24, 32, 78, 96, 5, 14, 1006, 1016, 6, 18, 35, 85,
        [
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Thunderstorms", "⛈️"),
            new("Rainy", "🌧️"),
            new("Partly Cloudy", "⛅")
        ]),
        new("New York", 40.7128, -74.0060, 6, 24, 48, 82, 8, 26, 1005, 1024, 9, 30, 15, 60,
        [
            new("Cool", "🧥"),
            new("Sunny", "☀️"),
            new("Cloudy", "☁️"),
            new("Windy", "💨"),
            new("Showers", "🚿")
        ]),
        new("Oslo", 59.9139, 10.7522, -6, 18, 55, 88, 6, 24, 1002, 1024, 8, 26, 12, 42,
        [
            new("Cold", "🥶"),
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Snow Showers", "🌨️"),
            new("Breezy", "🍃")
        ]),
        new("Rio de Janeiro", -22.9068, -43.1729, 22, 34, 65, 92, 7, 24, 1006, 1018, 7, 20, 28, 78,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Showers", "🚿"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Cairo", 30.0444, 31.2357, 14, 38, 30, 60, 9, 30, 1007, 1020, 8, 24, 35, 110,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Dry", "🏜️"),
            new("Breezy", "🍃"),
            new("Clear", "🌤️")
        ]),
        new("Sydney", -33.8688, 151.2093, 10, 27, 55, 85, 8, 26, 1008, 1024, 9, 28, 12, 45,
        [
            new("Sunny", "☀️"),
            new("Partly Cloudy", "⛅"),
            new("Showers", "🚿"),
            new("Breezy", "🍃"),
            new("Mild", "🙂")
        ]),
        new("Cape Town", -33.9249, 18.4241, 9, 28, 50, 82, 8, 26, 1008, 1025, 10, 30, 10, 40,
        [
            new("Sunny", "☀️"),
            new("Windy", "💨"),
            new("Partly Cloudy", "⛅"),
            new("Light Rain", "🌦️"),
            new("Mild", "🙂")
        ]),
        new("Mexico City", 19.4326, -99.1332, 8, 27, 40, 75, 8, 24, 1010, 1025, 7, 22, 18, 65,
        [
            new("Sunny", "☀️"),
            new("Mild", "🙂"),
            new("Partly Cloudy", "⛅"),
            new("Showers", "🚿"),
            new("Cloudy", "☁️")
        ]),
        new("Kingston", 17.9712, -76.7936, 24, 32, 68, 92, 7, 22, 1007, 1017, 8, 24, 22, 70,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Showers", "🚿"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Madrid", 40.4168, -3.7038, 5, 34, 35, 70, 8, 28, 1009, 1025, 7, 22, 14, 52,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Dry", "🏜️"),
            new("Breezy", "🍃"),
            new("Clear", "🌤️")
        ]),
        new("Cardiff", 51.4816, -3.1791, 6, 21, 65, 90, 7, 22, 1004, 1023, 8, 24, 10, 38,
        [
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Windy", "💨"),
            new("Cool", "🧥"),
            new("Overcast", "🌥️")
        ]),
        new("Edinburgh", 55.9533, -3.1883, 3, 19, 60, 88, 7, 22, 1003, 1022, 9, 25, 10, 35,
        [
            new("Cool", "🧥"),
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Windy", "💨"),
            new("Overcast", "🌥️")
        ]),
        new("Dublin", 53.3498, -6.2603, 5, 20, 65, 90, 7, 23, 1004, 1023, 8, 24, 10, 36,
        [
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Breezy", "🍃"),
            new("Cool", "🧥"),
            new("Overcast", "🌥️")
        ]),
        new("Belfast", 54.5973, -5.9301, 4, 19, 65, 90, 7, 22, 1003, 1022, 8, 25, 10, 36,
        [
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Windy", "💨"),
            new("Cool", "🧥"),
            new("Overcast", "🌥️")
        ]),
        new("Beijing", 39.9042, 116.4074, -4, 32, 35, 75, 7, 24, 1008, 1026, 7, 22, 25, 90,
        [
            new("Sunny", "☀️"),
            new("Dry", "🏜️"),
            new("Windy", "💨"),
            new("Cloudy", "☁️"),
            new("Clear", "🌤️")
        ]),
        new("Moscow", 55.7558, 37.6173, -10, 20, 55, 88, 6, 22, 1000, 1023, 8, 24, 12, 45,
        [
            new("Cold", "🥶"),
            new("Snow Showers", "🌨️"),
            new("Cloudy", "☁️"),
            new("Cool", "🧥"),
            new("Overcast", "🌥️")
        ]),
        new("Mumbai", 19.0760, 72.8777, 24, 34, 68, 95, 5, 18, 1002, 1014, 6, 18, 40, 120,
        [
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Rainy", "🌧️"),
            new("Thunderstorms", "⛈️"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Dubai", 25.2048, 55.2708, 18, 42, 35, 75, 8, 30, 1004, 1018, 7, 24, 30, 95,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Dry", "🏜️"),
            new("Clear", "🌤️"),
            new("Breezy", "🍃")
        ]),
        new("Riyadh", 24.7136, 46.6753, 10, 43, 20, 55, 8, 30, 1003, 1018, 8, 24, 28, 100,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Dry", "🏜️"),
            new("Clear", "🌤️"),
            new("Windy", "💨")
        ]),
        new("Tehran", 35.6892, 51.3890, 2, 36, 25, 60, 8, 28, 1007, 1023, 7, 22, 22, 78,
        [
            new("Sunny", "☀️"),
            new("Dry", "🏜️"),
            new("Breezy", "🍃"),
            new("Clear", "🌤️"),
            new("Cloudy", "☁️")
        ]),
        new("Jerusalem", 31.7683, 35.2137, 7, 31, 35, 70, 8, 28, 1008, 1024, 7, 20, 16, 58,
        [
            new("Sunny", "☀️"),
            new("Clear", "🌤️"),
            new("Mild", "🙂"),
            new("Breezy", "🍃"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Buenos Aires", -34.6037, -58.3816, 8, 31, 55, 85, 8, 25, 1006, 1023, 8, 24, 16, 52,
        [
            new("Sunny", "☀️"),
            new("Mild", "🙂"),
            new("Showers", "🚿"),
            new("Partly Cloudy", "⛅"),
            new("Windy", "💨")
        ]),
        new("Bogota", 4.7110, -74.0721, 7, 20, 60, 88, 6, 18, 1010, 1025, 6, 18, 12, 42,
        [
            new("Cool", "🧥"),
            new("Cloudy", "☁️"),
            new("Light Rain", "🌦️"),
            new("Mild", "🙂"),
            new("Overcast", "🌥️")
        ]),
        new("Caracas", 10.4806, -66.9036, 20, 31, 60, 90, 7, 22, 1008, 1018, 6, 18, 18, 62,
        [
            new("Warm", "🌤️"),
            new("Humid", "💧"),
            new("Showers", "🚿"),
            new("Partly Cloudy", "⛅"),
            new("Sunny", "☀️")
        ]),
        new("Toronto", 43.6532, -79.3832, -8, 27, 45, 82, 7, 25, 1002, 1024, 8, 26, 10, 42,
        [
            new("Cold", "🥶"),
            new("Sunny", "☀️"),
            new("Cloudy", "☁️"),
            new("Snow Showers", "🌨️"),
            new("Breezy", "🍃")
        ]),
        new("Montreal", 45.5017, -73.5673, -12, 25, 50, 85, 6, 24, 1001, 1023, 8, 26, 10, 40,
        [
            new("Cold", "🥶"),
            new("Cloudy", "☁️"),
            new("Snow Showers", "🌨️"),
            new("Sunny", "☀️"),
            new("Windy", "💨")
        ]),
        new("Los Angeles", 34.0522, -118.2437, 12, 31, 40, 70, 9, 28, 1009, 1022, 6, 18, 18, 60,
        [
            new("Sunny", "☀️"),
            new("Warm", "🌤️"),
            new("Clear", "🌤️"),
            new("Dry", "🏜️"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Dallas", 32.7767, -96.7970, 8, 37, 45, 80, 8, 26, 1004, 1020, 8, 24, 20, 78,
        [
            new("Sunny", "☀️"),
            new("Hot", "🔥"),
            new("Windy", "💨"),
            new("Thunderstorms", "⛈️"),
            new("Partly Cloudy", "⛅")
        ]),
        new("Miami", 25.7617, -80.1918, 21, 34, 65, 92, 7, 22, 1007, 1018, 8, 22, 28, 85,
        [
            new("Hot", "🔥"),
            new("Humid", "💧"),
            new("Sunny", "☀️"),
            new("Showers", "🚿"),
            new("Thunderstorms", "⛈️")
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
        var temperatureC = GetDeterministicTemperature(city, dateUtc);
        var (minimumTemperatureC, maximumTemperatureC) = GetDeterministicTemperatureRange(city, dateUtc, temperatureC);
        var humidityPercent = GetDeterministicHumidity(city, dateUtc, summary.Text);
        var pressureHpa = GetDeterministicPressure(city, dateUtc);
        var visibilityKm = GetDeterministicVisibility(city, dateUtc, humidityPercent, summary.Text);
        var airQualityIndex = GetDeterministicAirQualityIndex(city, dateUtc);
        var daylightHours = GetDeterministicDaylightHours(city, dateUtc);
        var uvIndex = GetDeterministicUvIndex(city, dateUtc, daylightHours, summary.Text);
        var rainProbabilityPercent = GetDeterministicRainProbability(city, dateUtc, humidityPercent, summary.Text);
        var wind = GetDeterministicWind(city, dateUtc);
        var feelsLikeTemperatureC = CalculateFeelsLikeTemperatureC(temperatureC, humidityPercent, wind.SpeedKph);
        var dewPointC = CalculateDewPointC(temperatureC, humidityPercent);

        return new WeatherForecast
        {
            City = city.City,
            Date = dateUtc,
            Latitude = city.Latitude,
            Longitude = city.Longitude,
            TemperatureC = temperatureC,
            FeelsLikeTemperatureC = feelsLikeTemperatureC,
            MinimumTemperatureC = minimumTemperatureC,
            MaximumTemperatureC = maximumTemperatureC,
            HumidityPercent = humidityPercent,
            DewPointC = dewPointC,
            PressureHpa = pressureHpa,
            VisibilityKm = visibilityKm,
            AirQualityIndex = airQualityIndex,
            UVIndex = uvIndex,
            RainProbabilityPercent = rainProbabilityPercent,
            WindSpeedKph = wind.SpeedKph,
            WindGustKph = wind.GustKph,
            WindDirectionDegrees = wind.DirectionDegrees,
            WindDirection = wind.DirectionText,
            DaylightHours = daylightHours,
            ActivityRecommendation = GetActivityRecommendation(
                summary.Text,
                rainProbabilityPercent,
                airQualityIndex,
                uvIndex,
                feelsLikeTemperatureC,
                wind.SpeedKph),
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

        return CreateRandom(city.City, dateUtc, TemperatureSeedSalt)
            .Next(minimumTemperature, exclusiveMaximumTemperature);
    }

    private static (int MinimumTemperatureC, int MaximumTemperatureC) GetDeterministicTemperatureRange(
        CityWeatherProfile city,
        DateTime dateUtc,
        int currentTemperatureC)
    {
        var seasonalAdjustment = GetSeasonAdjustment(city.City, dateUtc.Month);
        var seasonalMin = city.MinTempC + seasonalAdjustment - 2;
        var seasonalMax = city.MaxTempC + seasonalAdjustment + 4;

        var random = CreateRandom(city.City, dateUtc, TemperatureRangeSeedSalt);
        var min = Math.Clamp(currentTemperatureC - random.Next(1, 5), seasonalMin, seasonalMax - 1);
        var max = Math.Clamp(currentTemperatureC + random.Next(2, 7), min + 1, seasonalMax);

        return (min, max);
    }

    private static WeatherSummary GetDeterministicSummary(CityWeatherProfile city, DateTime dateUtc)
    {
        var random = CreateRandom(city.City, dateUtc, SummarySeedSalt);
        return city.Summaries[random.Next(city.Summaries.Length)];
    }

    private static int GetDeterministicHumidity(CityWeatherProfile city, DateTime dateUtc, string summary)
    {
        var random = CreateRandom(city.City, dateUtc, HumiditySeedSalt);
        var humidity = random.Next(city.MinHumidityPercent, city.MaxHumidityPercent + 1);

        humidity += summary switch
        {
            "Humid" => 5,
            "Thunderstorms" or "Rainy" or "Light Rain" or "Showers" => 7,
            "Sunny" => -8,
            "Windy" or "Breezy" => -4,
            _ => 0
        };

        return Math.Clamp(humidity, 35, 100);
    }

    private static int GetDeterministicPressure(CityWeatherProfile city, DateTime dateUtc) =>
        CreateRandom(city.City, dateUtc, PressureSeedSalt)
            .Next(city.MinPressureHpa, city.MaxPressureHpa + 1);

    private static double GetDeterministicVisibility(
        CityWeatherProfile city,
        DateTime dateUtc,
        int humidityPercent,
        string summary)
    {
        var random = CreateRandom(city.City, dateUtc, VisibilitySeedSalt);
        var visibility = city.MinVisibilityKm + (random.NextDouble() * (city.MaxVisibilityKm - city.MinVisibilityKm));

        visibility -= summary switch
        {
            "Thunderstorms" => 4.0,
            "Rainy" or "Light Rain" or "Showers" => 2.5,
            "Cloudy" or "Overcast" => 1.0,
            _ => 0.0
        };

        visibility -= Math.Max(0, humidityPercent - 75) / 25.0;
        return Math.Round(Math.Clamp(visibility, 2.0, city.MaxVisibilityKm), 1);
    }

    private static int GetDeterministicAirQualityIndex(CityWeatherProfile city, DateTime dateUtc) =>
        CreateRandom(city.City, dateUtc, AirQualitySeedSalt)
            .Next(city.MinAirQualityIndex, city.MaxAirQualityIndex + 1);

    private static int GetDeterministicRainProbability(
        CityWeatherProfile city,
        DateTime dateUtc,
        int humidityPercent,
        string summary)
    {
        var random = CreateRandom(city.City, dateUtc, RainProbabilitySeedSalt);
        var baseProbability = random.Next(5, 45) + ((humidityPercent - 50) / 2);

        baseProbability += summary switch
        {
            "Thunderstorms" => 45,
            "Rainy" => 35,
            "Light Rain" or "Showers" => 30,
            "Cloudy" or "Overcast" => 15,
            "Partly Cloudy" => 8,
            "Sunny" => -15,
            _ => 0
        };

        return Math.Clamp(baseProbability, 0, 100);
    }

    private static double GetDeterministicDaylightHours(CityWeatherProfile city, DateTime dateUtc)
    {
        var dayOfYear = dateUtc.DayOfYear;
        var latitudeFactor = Math.Min(Math.Abs(city.Latitude) / 90d, 1d);
        var amplitude = 1.2 + (latitudeFactor * 4.8);
        var seasonalWave = Math.Sin((2d * Math.PI * (dayOfYear - 80)) / 365.25);

        var daylightHours = 12d + (amplitude * seasonalWave);
        return Math.Round(Math.Clamp(daylightHours, 10d, 18d), 1);
    }

    private static int GetDeterministicUvIndex(
        CityWeatherProfile city,
        DateTime dateUtc,
        double daylightHours,
        string summary)
    {
        var random = CreateRandom(city.City, dateUtc, UvSeedSalt);
        var localTime = dateUtc + TimeSpan.FromHours(city.Longitude / 15d);
        var localHour = localTime.TimeOfDay.TotalHours;

        if (localHour < 6d || localHour > 18d)
        {
            return 0;
        }

        var solarFactor = Math.Sin(Math.PI * ((localHour - 6d) / 12d));
        var daylightFactor = daylightHours / 12d;
        var cloudModifier = summary switch
        {
            "Thunderstorms" or "Rainy" or "Light Rain" or "Showers" => 0.45,
            "Cloudy" or "Overcast" => 0.65,
            "Partly Cloudy" => 0.8,
            _ => 1.0
        };

        var uv = (7d * daylightFactor * solarFactor * cloudModifier) + (random.NextDouble() * 2d);
        return Math.Clamp((int)Math.Round(uv), 0, 11);
    }

    private static WindData GetDeterministicWind(CityWeatherProfile city, DateTime dateUtc)
    {
        var random = CreateRandom(city.City, dateUtc, WindSeedSalt);
        var speedKph = city.MinWindSpeedKph + (random.NextDouble() * (city.MaxWindSpeedKph - city.MinWindSpeedKph));
        var gustKph = speedKph + 3d + (random.NextDouble() * 12d);
        var directionDegrees = random.Next(0, 360);

        return new WindData(
            Math.Round(speedKph, 1),
            Math.Round(gustKph, 1),
            directionDegrees,
            GetCardinalDirection(directionDegrees));
    }

    private static int CalculateFeelsLikeTemperatureC(int temperatureC, int humidityPercent, double windSpeedKph)
    {
        if (temperatureC <= 10 && windSpeedKph > 4.8)
        {
            var windFactor = Math.Pow(windSpeedKph, 0.16);
            var windChill = 13.12 + (0.6215 * temperatureC) - (11.37 * windFactor) + (0.3965 * temperatureC * windFactor);
            return (int)Math.Round(windChill);
        }

        if (temperatureC >= 27 && humidityPercent >= 40)
        {
            var tempF = (temperatureC * 9d / 5d) + 32d;
            var hiF =
                -42.379 +
                (2.04901523 * tempF) +
                (10.14333127 * humidityPercent) -
                (0.22475541 * tempF * humidityPercent) -
                (0.00683783 * tempF * tempF) -
                (0.05481717 * humidityPercent * humidityPercent) +
                (0.00122874 * tempF * tempF * humidityPercent) +
                (0.00085282 * tempF * humidityPercent * humidityPercent) -
                (0.00000199 * tempF * tempF * humidityPercent * humidityPercent);

            return (int)Math.Round((hiF - 32d) * 5d / 9d);
        }

        return temperatureC;
    }

    private static double CalculateDewPointC(int temperatureC, int humidityPercent) =>
        Math.Round(temperatureC - ((100d - humidityPercent) / 5d), 1);

    private static string GetActivityRecommendation(
        string summary,
        int rainProbabilityPercent,
        int airQualityIndex,
        int uvIndex,
        int feelsLikeTemperatureC,
        double windSpeedKph)
    {
        if (airQualityIndex > 100)
        {
            return "Limit outdoor activity";
        }

        if (rainProbabilityPercent >= 70)
        {
            return "Carry an umbrella";
        }

        if (uvIndex >= 8)
        {
            return "Use sunscreen and seek shade";
        }

        if (windSpeedKph >= 30)
        {
            return "Secure loose outdoor items";
        }

        if (feelsLikeTemperatureC >= 30)
        {
            return "Stay hydrated";
        }

        if (feelsLikeTemperatureC <= 5)
        {
            return "Dress warmly";
        }

        return summary switch
        {
            "Sunny" or "Partly Cloudy" => "Good for outdoor activities",
            "Cloudy" or "Mild" => "Great for a walk",
            "Windy" or "Breezy" => "Good for light outdoor exercise",
            _ => "Conditions are moderate"
        };
    }

    private static string GetCardinalDirection(int degrees)
    {
        string[] directions = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];
        var index = (int)Math.Round(degrees / 45d, MidpointRounding.AwayFromZero) % directions.Length;

        return directions[index];
    }

    private static int GetSeasonAdjustment(string city, int month) =>
     city switch
     {
         "London" => month is 12 or 1 or 2 ? -3 : month is >= 6 and <= 8 ? 4 : 0,
         "Paris" => month is 12 or 1 or 2 ? -4 : month is >= 6 and <= 8 ? 5 : 0,
         "New York" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 7 : 0,
         "Toronto" => month is 12 or 1 or 2 ? -10 : month is >= 6 and <= 8 ? 6 : 0,
         "Montreal" => month is 12 or 1 or 2 ? -12 : month is >= 6 and <= 8 ? 6 : 0,
         "Oslo" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 4 : 0,
         "Cardiff" or "Edinburgh" or "Dublin" or "Belfast" => month is 12 or 1 or 2 ? -4 : month is >= 6 and <= 8 ? 3 : 0,
         "Moscow" => month is 12 or 1 or 2 ? -12 : month is >= 6 and <= 8 ? 5 : 0,
         "Beijing" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 8 : 0,
         "Madrid" => month is 12 or 1 or 2 ? -3 : month is >= 6 and <= 8 ? 8 : 0,
         "Los Angeles" => month is 12 or 1 or 2 ? 1 : month is >= 6 and <= 8 ? 4 : 0,
         "Dallas" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 8 ? 8 : 0,
         "Miami" => month is 12 or 1 or 2 ? 2 : month is >= 6 and <= 8 ? 3 : 0,
         "Mexico City" => month is 12 or 1 or 2 ? -1 : month is >= 4 and <= 6 ? 3 : 0,
         "Cairo" or "Dubai" or "Riyadh" or "Tehran" or "Jerusalem" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 8 ? 7 : 0,
         "Mumbai" => month is >= 6 and <= 9 ? -1 : month is >= 3 and <= 5 ? 2 : 0,
         "Bogota" => 0,
         "Caracas" or "Kingston" or "Lagos" or "Kuala Lumpur" => month is >= 6 and <= 9 ? -1 : 1,
         "Rio de Janeiro" or "Sydney" or "Cape Town" or "Buenos Aires" => month is >= 6 and <= 8 ? -4 : month is 12 or 1 or 2 ? 5 : 0,
         "Abuja" => month is >= 6 and <= 9 ? -2 : month is >= 3 and <= 5 ? 2 : 0,
         "Maiduguri" => month is 12 or 1 or 2 ? -1 : month is >= 3 and <= 6 ? 4 : month is >= 7 and <= 9 ? -2 : 1,
         _ => 0
     };

    private static Random CreateRandom(string city, DateTime dateUtc, int salt) =>
        new(GetDeterministicSeed(city, dateUtc, salt));

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
        double Latitude,
        double Longitude,
        int MinTempC,
        int MaxTempC,
        int MinHumidityPercent,
        int MaxHumidityPercent,
        double MinVisibilityKm,
        double MaxVisibilityKm,
        int MinPressureHpa,
        int MaxPressureHpa,
        double MinWindSpeedKph,
        double MaxWindSpeedKph,
        int MinAirQualityIndex,
        int MaxAirQualityIndex,
        WeatherSummary[] Summaries);

    private sealed record WeatherSummary(string Text, string Icon);

    private sealed record WindData(
        double SpeedKph,
        double GustKph,
        int DirectionDegrees,
        string DirectionText);
}