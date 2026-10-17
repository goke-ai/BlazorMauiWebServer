using Goke.Core.Models;
using Goke.Core.Services;

namespace GokeWeb.Endpoints
{
    public static class ApiEndpoints
    {
        extension(IEndpointRouteBuilder endpoints)
        {
            public IEndpointRouteBuilder MapApiEndpoints()
            {
                // Map your API endpoints here
                // Example:
                // endpoints.MapGet("api/example", async () => { /* Your logic here */ });

                // weather forecast endpoints
                //api/weather/city/{city}
                endpoints.MapGet("api/weather/cities/{cities}", async (string cities, IHttpClientFactory httpClientFactory) =>
                {

                    HttpClient httpClient = httpClientFactory.CreateClient(BackendApiEndpoints.ClientName);
                    var forecasts = await httpClient.GetFromJsonAsync<List<WeatherForecast>>($"api/weather/cities/{cities}") ?? new List<WeatherForecast>();
                    return Results.Ok(forecasts);
                });

                return endpoints;
            }
        }
    }
}
