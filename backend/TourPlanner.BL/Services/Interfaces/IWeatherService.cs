using TourPlanner.BL.DTOs;

namespace TourPlanner.BL.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponse?> GetCurrentWeatherAsync(double lat, double lon);
}
