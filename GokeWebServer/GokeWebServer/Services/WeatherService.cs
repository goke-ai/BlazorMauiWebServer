using Goke.Core.Models;

namespace GokeWebServer.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {

        public Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts()
        {
            var startDate = DateTime.Now;
            var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

            // generate 100 forecast in 10 sec difference

            var DATA = Enumerable.Range(1, 100).Select(index => new WeatherForecast
            {
                Date = startDate.AddSeconds(index * 10),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            });

            return Task.FromResult(DATA);
        }


        public Task<IEnumerable<WeatherForecast>> GetWeatherForecastsInRange(DateTime start, DateTime end)
        {
            var startDate = DateTime.Now;
            var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
            // generate 100 forecast in 10 sec difference
            var DATA = Enumerable.Range(1, 100).Select(index => new WeatherForecast
            {
                Date = startDate.AddSeconds(index * 10),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            });
            var filteredData = DATA.Where(f => f.Date >= start && f.Date <= end);
            return Task.FromResult(filteredData);
        }

    }
}
