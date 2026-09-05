using GokeWebServer.Services;

namespace GokeWebServer.Endpoints
{
    public static class WeatherForecastEndpoints
    {
        // Weather forecast endpoints
        public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
        {
            //api/weather
            endpoints.MapGet("api/weather", async (IWeatherForecastService weatherService) =>
            {
                var forecasts = await weatherService.GetAllWeatherForecasts();
                return Results.Ok(forecasts);
            });

            return endpoints;
        }
    }
}
