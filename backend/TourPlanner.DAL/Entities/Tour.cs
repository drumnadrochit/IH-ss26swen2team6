using TourPlanner.DAL.Entities.Enums;

namespace TourPlanner.DAL.Entities;

public class Tour
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public TransportType TransportType { get; set; }
    public double Distance { get; set; }
    public int EstimatedTime { get; set; }
    public string? RouteImagePath { get; set; }
    public double? FromLat { get; set; }
    public double? FromLon { get; set; }
    public double? ToLat { get; set; }
    public double? ToLon { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<TourLog> TourLogs { get; set; } = new List<TourLog>();
}
