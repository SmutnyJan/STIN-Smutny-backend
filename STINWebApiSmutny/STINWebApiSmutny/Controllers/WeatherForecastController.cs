using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using STINWebApiSmutny.Models;
using System.Text.Json;

namespace STINWebApiSmutny.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private string apiKey = "562a4e9efbf8cb651983751fe24986b1";



        public WeatherForecastController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Weather/{city}")]
        public async Task<ActionResult<WeatherPackage>> GetWeather(string city)
        {
            List<Location>? locations;
            try
            {
                locations = await GetLocation(city);
                if (locations.Count == 0)
                {
                    return BadRequest("No such place!");
                }
            }
            catch (HttpRequestException e)
            {
                return BadRequest(e);
            }

            string forecastUrl = $"https://api.openweathermap.org/data/2.5/forecast/daily?lat={locations[0].lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={locations[0].lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&cnt=1&appid={apiKey}&units=metric";

            ForecastWeather forecastWeather;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(forecastUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    forecastWeather = JsonSerializer.Deserialize<ForecastWeather>(responseBody);

                }
                catch (HttpRequestException e)
                {
                    return BadRequest(e);
                }
            }


            string weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={locations[0].lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={locations[0].lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&appid={apiKey}&units=metric";

            CurrentWeather currentWeather;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(weatherUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    currentWeather = JsonSerializer.Deserialize<CurrentWeather>(responseBody);

                   // return currentWeather;
                }
                catch (HttpRequestException e)
                {
                    return BadRequest(e);
                }
            }

            return new WeatherPackage() { CurrentWeather = currentWeather, ForecastWeather = forecastWeather };

        }

        [HttpGet("WeatherLastWeek/{city}")]
        public async Task<ActionResult<List<HistoryWeather>>> GetWeatherLastWeek(string city)
        {
            List<Location>? locations;
            try
            {
                locations = await GetLocation(city);
                if (locations.Count == 0)
                {
                    return BadRequest("No such place!");
                }
            }
            catch (HttpRequestException e)
            {
                return BadRequest(e);
            }

            List<HistoryWeather> weather = new List<HistoryWeather>();
            for (var i = 1; i < 7; i++)
            {
                DateTime today = DateTime.Today;

                DateTime noonToday = today.AddHours(15).AddDays(-i);

                var unixTime = ((DateTimeOffset)noonToday).ToUnixTimeSeconds();

                string weatherUrl = $"https://history.openweathermap.org/data/2.5/history/city?lat={locations[0].lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={locations[0].lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&type=day&start={unixTime}&cnt=1&appid={apiKey}&units=metric";

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(weatherUrl);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        HistoryWeather currentWeather = JsonSerializer.Deserialize<HistoryWeather>(responseBody);
                        currentWeather.DateTime = noonToday;

                        weather.Add(currentWeather);
                    }
                    catch (HttpRequestException e)
                    {
                        return BadRequest(e);
                    }
                }
            }

            return weather;

        }


        private async Task<List<Location>?> GetLocation(string city)
        {
            string locationUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(locationUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    List<Location>? location = JsonSerializer.Deserialize<List<Location>>(responseBody);
                    return location;
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException();
                }
            }


        }
    }
}
