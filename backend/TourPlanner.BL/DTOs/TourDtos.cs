using System.ComponentModel.DataAnnotations;
using TourPlanner.DAL.Entities.Enums;

namespace TourPlanner.BL.DTOs;

public record CreateTourRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [StringLength(2000)] string Description,
    [Required, StringLength(500, MinimumLength = 1)] string From,
    [Required, StringLength(500, MinimumLength = 1)] string To,
    TransportType TransportType
);

public record UpdateTourRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [StringLength(2000)] string Description,
    [Required, StringLength(500, MinimumLength = 1)] string From,
    [Required, StringLength(500, MinimumLength = 1)] string To,
    TransportType TransportType
);

public record TourResponse(
    Guid Id,
    string Name,
    string Description,
    string From,
    string To,
    string TransportType,
    double Distance,
    int EstimatedTime,
    string? RouteImagePath,
    int Popularity,
    string ChildFriendliness,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record TourExportDto(
    string Name,
    string Description,
    string From,
    string To,
    string TransportType,
    double Distance,
    int EstimatedTime,
    List<TourLogExportDto> Logs
);
