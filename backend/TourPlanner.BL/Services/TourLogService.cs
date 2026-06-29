using TourPlanner.BL.DTOs;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.Services.Interfaces;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Repositories.Interfaces;
using log4net;

namespace TourPlanner.BL.Services;

public class TourLogService : ITourLogService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TourLogService));
    private readonly ITourLogRepository _logRepo;
    private readonly ITourRepository _tourRepo;

    public TourLogService(ITourLogRepository logRepo, ITourRepository tourRepo)
    {
        _logRepo = logRepo;
        _tourRepo = tourRepo;
    }

    public async Task<IEnumerable<TourLogResponse>> GetLogsAsync(Guid tourId, Guid userId)
    {
        var tour = await _tourRepo.GetByIdAsync(tourId);
        if (tour == null || tour.UserId != userId)
            throw new EntityNotFoundException("Tour not found.");
        var logs = await _logRepo.GetByTourIdAsync(tourId);
        return logs.Select(MapToResponse);
    }

    public async Task<TourLogResponse> CreateLogAsync(Guid tourId, CreateTourLogRequest request, Guid userId)
    {
        ValidateLogRequest(request.Difficulty, request.Rating, request.TotalDistance, request.TotalTime);
        var tour = await _tourRepo.GetByIdAsync(tourId);
        if (tour == null || tour.UserId != userId)
            throw new EntityNotFoundException("Tour not found.");

        var log = new TourLog
        {
            TourId = tourId,
            UserId = userId,
            DateTime = request.DateTime,
            Comment = request.Comment,
            Difficulty = request.Difficulty,
            TotalDistance = request.TotalDistance,
            TotalTime = request.TotalTime,
            Rating = request.Rating
        };
        await _logRepo.AddAsync(log);
        Log.Info($"TourLog created for tour {tourId}");
        return MapToResponse(log);
    }

    public async Task<TourLogResponse> UpdateLogAsync(Guid tourId, Guid logId, UpdateTourLogRequest request, Guid userId)
    {
        ValidateLogRequest(request.Difficulty, request.Rating, request.TotalDistance, request.TotalTime);
        var log = await GetOwnedLogOrThrowAsync(tourId, logId, userId);

        log.DateTime = request.DateTime;
        log.Comment = request.Comment;
        log.Difficulty = request.Difficulty;
        log.TotalDistance = request.TotalDistance;
        log.TotalTime = request.TotalTime;
        log.Rating = request.Rating;
        await _logRepo.UpdateAsync(log);
        Log.Info($"TourLog updated: {logId}");
        return MapToResponse(log);
    }

    public async Task DeleteLogAsync(Guid tourId, Guid logId, Guid userId)
    {
        await GetOwnedLogOrThrowAsync(tourId, logId, userId);
        await _logRepo.DeleteAsync(logId);
        Log.Info($"TourLog deleted: {logId}");
    }

    private async Task<TourLog> GetOwnedLogOrThrowAsync(Guid tourId, Guid logId, Guid userId)
    {
        var log = await _logRepo.GetByIdAsync(logId);
        if (log == null)
        {
            Log.Warn($"TourLog {logId} not found.");
            throw new EntityNotFoundException("Log not found.");
        }
        if (log.TourId != tourId || log.UserId != userId)
        {
            Log.Warn($"User {userId} attempted to access log {logId} owned by {log.UserId}.");
            throw new ForbiddenAccessException("Access denied.");
        }
        return log;
    }

    private static void ValidateLogRequest(int difficulty, int rating, double distance, int time)
    {
        if (difficulty < 1 || difficulty > 5)
            throw new DomainValidationException("Difficulty must be between 1 and 5.");
        if (rating < 1 || rating > 5)
            throw new DomainValidationException("Rating must be between 1 and 5.");
        if (distance <= 0)
            throw new DomainValidationException("Distance must be positive.");
        if (time <= 0)
            throw new DomainValidationException("Time must be positive.");
    }

    private static TourLogResponse MapToResponse(TourLog log) =>
        new(log.Id, log.TourId, log.DateTime, log.Comment,
            log.Difficulty, log.TotalDistance, log.TotalTime, log.Rating, log.CreatedAt);
}
