using TourPlanner.BL.DTOs;

namespace TourPlanner.BL.Services.Interfaces;

public interface ITourImportExportService
{
    Task<List<TourExportDto>> ExportToursAsync(Guid userId);
    Task<int> ImportToursAsync(Guid userId, List<TourExportDto> tours);
}
