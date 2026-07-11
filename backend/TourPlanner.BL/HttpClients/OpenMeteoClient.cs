using System.Net.Http.Json;
using log4net;

namespace TourPlanner.BL.HttpClients;

// Open-Meteo requires no API key, fitting nicely as a small "unique feature" add-on:
// current weather at the tour's starting location.
public class OpenMeteoClient : IWeatherClient
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(OpenMeteoClient));
    private readonly HttpClient _httpClient;

    public OpenMeteoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.open-meteo.com");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<CurrentWeather?> GetCurrentWeatherAsync(double lat, double lon)
    {
        try
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var url = $"/v1/forecast?latitude={lat.ToString(inv)}&longitude={lon.ToString(inv)}&current_weather=true";
            var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
            return response?.CurrentWeather;
        }
        catch (Exception ex)
        {
            Log.Warn($"Weather request failed: {ex.Message}");
            return null;
        }
    }
}
