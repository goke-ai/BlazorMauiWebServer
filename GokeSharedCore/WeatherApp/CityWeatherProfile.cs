namespace GokeSharedCore.WeatherApp
{
    public sealed record CityWeatherProfile(
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
}