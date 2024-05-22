using Xunit;
using Microsoft.EntityFrameworkCore;
using STINWebApiSmutny.Controllers;
using STINWebApiSmutny.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Moq;
using Moq.Protected;
using System.Threading;
using FluentAssertions;
using System.Net.Http.Json;

namespace STIN_Tests.ControllersTest
{
    public class WeatherForecastControllerTests
    {
        private AppDbContext GetDbContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            return new AppDbContext(dbContextOptions);
        }

        private HttpClient GetHttpClient(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);
            return httpClient;
        }

        [Fact]
        public async Task GetWeather_ShouldReturnWeatherPackage()
        {
            // Arrange
            var dbContext = GetDbContext();
            var locationResponse = new List<Location>
        {
            new Location { lat = 50.0755, lon = 14.4378 }
        };
            var currentWeatherResponse = new CurrentWeather
            {
                main = new Main { temp = 14.7 }
            };
            var forecastWeatherResponse = new ForecastWeather
            {
                city = new City { name = "Prague" },
                list = new List<ListForecast>
            {
                new ListForecast { temp = new Temp { day = 16.0 } }
            }
            };

            var locationHttpClient = GetHttpClient(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(locationResponse)
            });
            var currentWeatherHttpClient = GetHttpClient(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(currentWeatherResponse)
            });
            var forecastWeatherHttpClient = GetHttpClient(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(forecastWeatherResponse)
            });

            // Mock HttpClient responses
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(locationResponse)
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(currentWeatherResponse)
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(forecastWeatherResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var controller = new WeatherForecastController(dbContext);

            // Act
            var result = await controller.GetWeather("Prague");

            // Assert
            result.Value.Should().BeOfType<WeatherPackage>();
            var weatherPackage = result.Value;
            weatherPackage.CurrentWeather.main.temp.Should().Be(14.76);
            weatherPackage.ForecastWeather.city.name.Should().Be("Prague");
            weatherPackage.ForecastWeather.list.First().temp.day.Should().Be(14.33);
        }

        [Fact]
        public async Task GetWeatherLastWeek_ShouldReturnHistoryWeatherList()
        {
            // Arrange
            var dbContext = GetDbContext();
            var locationResponse = new List<Location>
        {
            new Location { lat = 50.0755, lon = 14.4378 }
        };
            var historyWeatherResponse = new HistoryWeather
            {
                list = new List<STINWebApiSmutny.Models.List>
            {
                new STINWebApiSmutny.Models.List
                {
                    main = new MainHistory { temp = 15.0 }
                }
            }
            };

            var locationHttpClient = GetHttpClient(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(locationResponse)
            });
            var historyWeatherHttpClient = GetHttpClient(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(historyWeatherResponse)
            });

            // Mock HttpClient responses
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(locationResponse)
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(historyWeatherResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var controller = new WeatherForecastController(dbContext);

            // Act
            var result = await controller.GetWeatherLastWeek("Prague");


            // Assert
            result.Value.Should().BeAssignableTo<List<HistoryWeather>>();
            var historyWeatherList = result.Value;
            historyWeatherList.Should().HaveCount(6);
            historyWeatherList.First().list.First().main.temp.Should().Be(23.6);
        }
    }
}
