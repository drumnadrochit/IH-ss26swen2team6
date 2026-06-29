using TourPlanner.BL.DTOs;
using TourPlanner.BL.Services.Interfaces;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Entities.Enums;
using TourPlanner.DAL.Repositories.Interfaces;
using TourPlanner.DAL.UnitOfWork;
using log4net;

namespace TourPlanner.BL.Services;

public class TourImportExportService : ITourImportExportService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TourImportExportService));
    private readonly ITourRepository _tourRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TourImportExportService(ITourRepository tourRepo, IUnitOfWork unitOfWork)
    {
        _tourRepo = tourRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TourExportDto>> ExportToursAsync(Guid userId)
    {
        var tours = await _tourRepo.GetByUserIdAsync(userId);
        return tours.Select(t => new TourExportDto(
            t.Name, t.Description, t.From, t.To, t.TransportType.ToString(),
            t.Distance, t.EstimatedTime,
            (t.TourLogs ?? []).Select(l => new TourLogExportDto(
                l.DateTime, l.Comment, l.Difficulty, l.TotalDistance, l.TotalTime, l.Rating)).ToList()
        )).ToList();
    }

    // All imported tours and their logs are persisted in a single SaveChanges call via
    // the Unit of Work, so a partially invalid import file cannot leave half-written data.
    public async Task<int> ImportToursAsync(Guid userId, List<TourExportDto> dtos)
    {
        var tours = new List<Tour>();

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.From) || string.IsNullOrWhiteSpace(dto.To))
                continue;

            if (!Enum.TryParse<TransportType>(dto.TransportType, true, out var transportType))
                transportType = TransportType.Hike;

            var tour = new Tour
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                From = dto.From,
                To = dto.To,
                TransportType = transportType,
                Distance = dto.Distance,
                EstimatedTime = dto.EstimatedTime,
                UserId = userId
            };

            foreach (var log in dto.Logs)
            {
                tour.TourLogs.Add(new TourLog
                {
                    TourId = tour.Id,
                    UserId = userId,
                    DateTime = log.DateTime,
                    Comment = log.Comment,
                    Difficulty = log.Difficulty,
                    TotalDistance = log.TotalDistance,
                    TotalTime = log.TotalTime,
                    Rating = log.Rating
                });
            }

            tours.Add(tour);
        }

        if (tours.Count == 0) return 0;

        _unitOfWork.AddTours(tours);
        await _unitOfWork.SaveChangesAsync();
        Log.Info($"Imported {tours.Count} tour(s) for user {userId}");
        return tours.Count;
    }
}
