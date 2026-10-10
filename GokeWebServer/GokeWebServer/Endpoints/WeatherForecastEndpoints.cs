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

            //api/weather/date/{date}
            endpoints.MapGet("api/weather/date/{date}", async (DateTime date, IWeatherForecastService weatherService) =>
            {
                var forecasts = await weatherService.GetWeatherForecastsForDate(date);
                return Results.Ok(forecasts);
            });

            //api/weather/datetime/{date}
            endpoints.MapGet("api/weather/datetime/{date}", async (DateTime date, IWeatherForecastService weatherService) =>
            {
                var forecasts = await weatherService.GetWeatherForecastsForDateTime(date);
                return Results.Ok(forecasts);
            });

            //api/weather/daterange?start={start}&end={end}
            endpoints.MapGet("api/weather/daterange", async (DateTime start, DateTime end, IWeatherForecastService weatherService) =>
            {
                var forecasts = await weatherService.GetWeatherForecastsForDateRange(start, end);
                return Results.Ok(forecasts);
            });

            //api/weather/city/{city}
            endpoints.MapGet("api/weather/cities/{cities}", async (string cities, IWeatherForecastService weatherService) =>
            {
                var cityArray = cities.Split(',');
                var forecasts = await weatherService.GetWeatherForecastsForCities(cityArray);
                return Results.Ok(forecasts);
            });

            //api/weather/city/{city}/date/{date}
            endpoints.MapGet("api/weather/cities/{cities}/date/{date}", async (string cities, DateTime date, IWeatherForecastService weatherService) =>
            {
                var cityArray = cities.Split(',');
                var forecasts = await weatherService.GetWeatherForecastsForDateAndCities(date, cityArray);
                return Results.Ok(forecasts);
            });

            //api/weather/city/{city}/datetime/{date}
            endpoints.MapGet("api/weather/cities/{cities}/datetime/{date}", async (string cities, DateTime date, IWeatherForecastService weatherService) =>
            {
                var cityArray = cities.Split(',');
                var forecasts = await weatherService.GetWeatherForecastsForDateTimeAndCities(date, cityArray);
                return Results.Ok(forecasts);
            });

            //api/weather/cities/{cities}/daterange?start={start}&end={end}
            endpoints.MapGet("api/weather/cities/{cities}/daterange", async (string cities, DateTime start, DateTime end, IWeatherForecastService weatherService) =>
            {
                var cityArray = cities.Split(',');
                var forecasts = await weatherService.GetWeatherForecastsForDateRangeAndCities(start, end, cityArray);
                return Results.Ok(forecasts);
            });

            return endpoints;
        }
    }
}
