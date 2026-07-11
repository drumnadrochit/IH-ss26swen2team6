using TourPlanner.BL.DTOs;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.HttpClients;
using TourPlanner.BL.Services.Interfaces;
using TourPlanner.BL.Strategies;
using TourPlanner.BL.Utils;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Repositories.Interfaces;
using log4net;

namespace TourPlanner.BL.Services;

public class TourService : ITourService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TourService));
    private readonly ITourRepository _tourRepo;
    private readonly IOpenRouteServiceClient _orsClient;
    private readonly IChildFriendlinessClassifier _childFriendlinessClassifier;
    private readonly ITransportSpeedResolver _speedResolver;

    public TourService(
        ITourRepository tourRepo,
        IOpenRouteServiceClient orsClient,
        IChildFriendlinessClassifier childFriendlinessClassifier,
        ITransportSpeedResolver speedResolver)
    {
        _tourRepo = tourRepo;
        _orsClient = orsClient;
        _childFriendlinessClassifier = childFriendlinessClassifier;
        _speedResolver = speedResolver;
    }

    public async Task<IEnumerable<TourResponse>> GetToursAsync(Guid userId)
    {
        var tours = await _tourRepo.GetByUserIdAsync(userId);
        return tours.Select(MapToResponse);
    }

    public async Task<TourResponse?> GetTourByIdAsync(Guid tourId, Guid userId)
    {
        var tour = await _tourRepo.GetWithLogsAsync(tourId);
        if (tour == null || tour.UserId != userId) return null;
        return MapToResponse(tour);
    }

    public async Task<IEnumerable<TourResponse>> SearchToursAsync(Guid userId, string query)
    {
        var tours = await _tourRepo.GetByUserIdAsync(userId);
        var term = query.Trim();
        if (term.Length == 0) return tours.Select(MapToResponse);

        return tours
            .Select(t => (Tour: t, Response: MapToResponse(t)))
            .Where(x => MatchesQuery(x.Tour, x.Response, term))
            .Select(x => x.Response);
    }

    // Full-text search across tour fields, tour-log comments, and computed attributes
    // (popularity, child-friendliness) as required by the spec.
    private static bool MatchesQuery(Tour tour, TourResponse response, string term)
        =>
        Contains(tour.Name, term) ||
        Contains(tour.Description, term) ||
        Contains(tour.From, term) ||
        Contains(tour.To, term) ||
        Contains(response.TransportType, term) ||
        Contains(response.ChildFriendliness, term) ||
        Contains(response.Popularity.ToString(), term) ||
        (tour.TourLogs?.Any(l => Contains(l.Comment, term)) ?? false);

    private static bool Contains(string? value, string term)
        => value != null && value.Contains(term, StringComparison.OrdinalIgnoreCase);

    public async Task<TourResponse> CreateTourAsync(CreateTourRequest request, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            Log.Warn($"CreateTour rejected for user {userId}: missing name.");
            throw new DomainValidationException("Tour name is required.");
        }
        if (string.IsNullOrWhiteSpace(request.From) || string.IsNullOrWhiteSpace(request.To))
        {
            Log.Warn($"CreateTour rejected for user {userId}: missing From/To.");
            throw new DomainValidationException("From and To locations are required.");
        }

        var tour = new Tour
        {
            Name = request.Name,
            Description = request.Description,
            From = request.From,
            To = request.To,
            TransportType = request.TransportType,
            UserId = userId
        };

        await FetchRouteDataAsync(tour);
        await _tourRepo.AddAsync(tour);
        Log.Info($"Tour created: {tour.Name} (id={tour.Id})");
        return MapToResponse(tour);
    }

    public async Task<TourResponse> UpdateTourAsync(Guid tourId, UpdateTourRequest request, Guid userId)
    {
        var tour = await GetOwnedTourOrThrowAsync(tourId, userId);

        bool routeChanged = tour.From != request.From || tour.To != request.To || tour.TransportType != request.TransportType;
        tour.Name = request.Name;
        tour.Description = request.Description;
        tour.From = request.From;
        tour.To = request.To;
        tour.TransportType = request.TransportType;
        tour.UpdatedAt = DateTime.UtcNow;

        if (routeChanged)
            await FetchRouteDataAsync(tour);

        await _tourRepo.UpdateAsync(tour);
        Log.Info($"Tour updated: {tour.Id}");
        return MapToResponse(tour);
    }

    public async Task<TourResponse> SetTourImageAsync(Guid tourId, Guid userId, string imagePath)
    {
        var tour = await GetOwnedTourOrThrowAsync(tourId, userId);

        tour.RouteImagePath = imagePath;
        tour.UpdatedAt = DateTime.UtcNow;
        await _tourRepo.UpdateAsync(tour);
        Log.Info($"Tour image updated: {tour.Id}");
        return MapToResponse(tour);
    }

    public async Task<(double Lat, double Lon)?> GetTourStartCoordinatesAsync(Guid tourId, Guid userId)
    {
        var tour = await _tourRepo.GetByIdAsync(tourId);
        if (tour == null || tour.UserId != userId) return null;
        if (tour.FromLat == null || tour.FromLon == null) return null;
        return (tour.FromLat.Value, tour.FromLon.Value);
    }

    public async Task DeleteTourAsync(Guid tourId, Guid userId)
    {
        await GetOwnedTourOrThrowAsync(tourId, userId);
        await _tourRepo.DeleteAsync(tourId);
        Log.Info($"Tour deleted: {tourId}");
    }

    private async Task<Tour> GetOwnedTourOrThrowAsync(Guid tourId, Guid userId)
    {
        var tour = await _tourRepo.GetByIdAsync(tourId);
        if (tour == null)
        {
            Log.Warn($"Tour {tourId} not found.");
            throw new EntityNotFoundException("Tour not found.");
        }
        if (tour.UserId != userId)
        {
            Log.Warn($"User {userId} attempted to access tour {tourId} owned by {tour.UserId}.");
            throw new ForbiddenAccessException("Access denied.");
        }
        return tour;
    }

    private async Task FetchRouteDataAsync(Tour tour)
    {
        try
        {
            var fromCoords = await _orsClient.GeocodeAsync(tour.From);
            var toCoords = await _orsClient.GeocodeAsync(tour.To);

            if (fromCoords.HasValue)
            {
                tour.FromLon = fromCoords.Value.lon;
                tour.FromLat = fromCoords.Value.lat;
            }
            if (toCoords.HasValue)
            {
                tour.ToLon = toCoords.Value.lon;
                tour.ToLat = toCoords.Value.lat;
            }

            if (fromCoords.HasValue && toCoords.HasValue)
            {
                var route = await _orsClient.GetDirectionsAsync(
                    fromCoords.Value.lon, fromCoords.Value.lat,
                    toCoords.Value.lon, toCoords.Value.lat,
                    tour.TransportType);
                if (route.HasValue)
                {
                    tour.Distance = route.Value.distance;
                    tour.EstimatedTime = route.Value.duration;
                }
                else
                {
                    // Directions API failed but we still have coordinates - approximate via Strategy pattern.
                    Log.Warn($"ORS directions unavailable for tour '{tour.Name}', falling back to haversine estimate.");
                    var distanceKm = GeoUtils.HaversineKm(
                        fromCoords.Value.lat, fromCoords.Value.lon,
                        toCoords.Value.lat, toCoords.Value.lon);
                    var speedKmh = _speedResolver.GetAverageSpeedKmh(tour.TransportType);
                    tour.Distance = Math.Round(distanceKm, 2);
                    tour.EstimatedTime = (int)Math.Round(distanceKm / speedKmh * 60);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Could not fetch route data for tour '{tour.Name}': {ex.Message}", ex);
        }
    }

    private TourResponse MapToResponse(Tour tour)
    {
        var logs = tour.TourLogs?.ToList() ?? [];
        int popularity = logs.Count;
        string childFriendliness = _childFriendlinessClassifier.Classify(logs);
        return new TourResponse(
            tour.Id, tour.Name, tour.Description, tour.From, tour.To,
            tour.TransportType.ToString(), tour.Distance, tour.EstimatedTime,
            tour.RouteImagePath, popularity, childFriendliness,
            tour.CreatedAt, tour.UpdatedAt);
    }
}
