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

        public WeatherForecastController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Weather/{city}")]
        public async Task<ActionResult<string>> GetWeather(string city)
        {
            string apiKey = "5ce0370e0fbf737fe37f5550bb8c58d6";
            string locationUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(locationUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if(responseBody == "[]")
                    {
                        return BadRequest("No such place.");
                    }
                    List<Location>? location = JsonSerializer.Deserialize<List<Location>>(responseBody);
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    return BadRequest(e);
                }
            }
        }
    }
}
