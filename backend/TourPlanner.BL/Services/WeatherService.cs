using TourPlanner.BL.DTOs;
using TourPlanner.BL.HttpClients;
using TourPlanner.BL.Services.Interfaces;

namespace TourPlanner.BL.Services;

public class WeatherService : IWeatherService
{
    private static readonly Dictionary<int, string> Descriptions = new()
    {
        [0] = "Clear sky", [1] = "Mainly clear", [2] = "Partly cloudy", [3] = "Overcast",
        [45] = "Fog", [48] = "Freezing fog",
        [51] = "Light drizzle", [53] = "Drizzle", [55] = "Dense drizzle",
        [61] = "Light rain", [63] = "Rain", [65] = "Heavy rain",
        [71] = "Light snow", [73] = "Snow", [75] = "Heavy snow",
        [80] = "Light showers", [81] = "Showers", [82] = "Violent showers",
        [95] = "Thunderstorm", [96] = "Thunderstorm with hail", [99] = "Severe thunderstorm with hail"
    };

    private readonly IWeatherClient _weatherClient;

    public WeatherService(IWeatherClient weatherClient) => _weatherClient = weatherClient;

    public async Task<WeatherResponse?> GetCurrentWeatherAsync(double lat, double lon)
    {
        var weather = await _weatherClient.GetCurrentWeatherAsync(lat, lon);
        if (weather == null) return null;

        return new WeatherResponse(
            weather.Temperature,
            weather.WindSpeed,
            weather.WeatherCode,
            Descriptions.GetValueOrDefault(weather.WeatherCode, "Unknown"),
            weather.IsDay == 1);
    }
}
