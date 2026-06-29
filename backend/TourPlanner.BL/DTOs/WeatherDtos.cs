namespace TourPlanner.BL.DTOs;

public record WeatherResponse(
    double TemperatureCelsius,
    double WindSpeedKmh,
    int WeatherCode,
    string Description,
    bool IsDay
);
