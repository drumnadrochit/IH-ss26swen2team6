using TourPlanner.BL.DTOs;

namespace TourPlanner.BL.Services.Interfaces;

public interface ITourService
{
    Task<IEnumerable<TourResponse>> GetToursAsync(Guid userId);
    Task<TourResponse?> GetTourByIdAsync(Guid tourId, Guid userId);
    Task<IEnumerable<TourResponse>> SearchToursAsync(Guid userId, string query);
    Task<TourResponse> CreateTourAsync(CreateTourRequest request, Guid userId);
    Task<TourResponse> UpdateTourAsync(Guid tourId, UpdateTourRequest request, Guid userId);
    Task<TourResponse> SetTourImageAsync(Guid tourId, Guid userId, string imagePath);
    Task<(double Lat, double Lon)?> GetTourStartCoordinatesAsync(Guid tourId, Guid userId);
    Task DeleteTourAsync(Guid tourId, Guid userId);
}
