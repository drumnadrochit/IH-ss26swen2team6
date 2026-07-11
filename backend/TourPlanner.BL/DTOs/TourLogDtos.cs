using System.ComponentModel.DataAnnotations;

namespace TourPlanner.BL.DTOs;

public record CreateTourLogRequest(
    DateTime DateTime,
    [StringLength(2000)] string Comment,
    [Range(1, 5)] int Difficulty,
    [Range(0.01, double.MaxValue)] double TotalDistance,
    [Range(1, int.MaxValue)] int TotalTime,
    [Range(1, 5)] int Rating
);

public record UpdateTourLogRequest(
    DateTime DateTime,
    [StringLength(2000)] string Comment,
    [Range(1, 5)] int Difficulty,
    [Range(0.01, double.MaxValue)] double TotalDistance,
    [Range(1, int.MaxValue)] int TotalTime,
    [Range(1, 5)] int Rating
);

public record TourLogResponse(
    Guid Id,
    Guid TourId,
    DateTime DateTime,
    string Comment,
    int Difficulty,
    double TotalDistance,
    int TotalTime,
    int Rating,
    DateTime CreatedAt
);

public record TourLogExportDto(
    DateTime DateTime,
    string Comment,
    int Difficulty,
    double TotalDistance,
    int TotalTime,
    int Rating
);
