namespace TourPlanner.BL.HttpClients;

public interface IWeatherClient
{
    Task<CurrentWeather?> GetCurrentWeatherAsync(double lat, double lon);
}
