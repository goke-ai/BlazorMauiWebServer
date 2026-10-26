namespace GokeSharedCore.WeatherApp
{
    public sealed record WindData(
        double SpeedKph,
        double GustKph,
        int DirectionDegrees,
        string DirectionText);
}