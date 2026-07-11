using Moq;
using NUnit.Framework;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Services;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Repositories.Interfaces;
using TourPlanner.DAL.UnitOfWork;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class TourImportExportServiceTests
{
    private Mock<ITourRepository> _tourRepoMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private TourImportExportService _service = null!;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _tourRepoMock = new Mock<ITourRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new TourImportExportService(_tourRepoMock.Object, _unitOfWorkMock.Object);
        _userId = Guid.NewGuid();
    }

    [Test]
    public async Task ExportToursAsync_MapsToursAndLogsToDto()
    {
        var tour = new Tour
        {
            Id = Guid.NewGuid(), Name = "Tour A", Description = "desc", From = "A", To = "B",
            UserId = _userId, Distance = 10, EstimatedTime = 60,
            TourLogs = [new TourLog { Comment = "nice", Difficulty = 2, TotalDistance = 10, TotalTime = 60, Rating = 4 }]
        };
        _tourRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync([tour]);

        var result = await _service.ExportToursAsync(_userId);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Tour A"));
        Assert.That(result[0].Logs.Count, Is.EqualTo(1));
        Assert.That(result[0].Logs[0].Comment, Is.EqualTo("nice"));
    }

    [Test]
    public async Task ImportToursAsync_WithValidData_AddsToursAndSavesOnce()
    {
        var dtos = new List<TourExportDto>
        {
            new("Imported Tour", "desc", "From", "To", "Bike", 5, 30,
                [new TourLogExportDto(DateTime.UtcNow, "log", 2, 5, 30, 4)])
        };

        var count = await _service.ImportToursAsync(_userId, dtos);

        Assert.That(count, Is.EqualTo(1));
        _unitOfWorkMock.Verify(u => u.AddTours(It.Is<IEnumerable<Tour>>(t => t.Count() == 1)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ImportToursAsync_SkipsEntriesMissingRequiredFields()
    {
        var dtos = new List<TourExportDto>
        {
            new("", "desc", "From", "To", "Bike", 0, 0, []),
            new("Valid Tour", "desc", "From", "To", "Bike", 0, 0, [])
        };

        var count = await _service.ImportToursAsync(_userId, dtos);

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task ImportToursAsync_WithNoValidTours_DoesNotCallUnitOfWork()
    {
        var dtos = new List<TourExportDto> { new("", "desc", "", "", "Bike", 0, 0, []) };

        var count = await _service.ImportToursAsync(_userId, dtos);

        Assert.That(count, Is.EqualTo(0));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task ImportToursAsync_WithUnknownTransportType_DefaultsToHike()
    {
        var dtos = new List<TourExportDto> { new("Tour", "d", "A", "B", "UnknownType", 0, 0, []) };
        IEnumerable<Tour>? captured = null;
        _unitOfWorkMock.Setup(u => u.AddTours(It.IsAny<IEnumerable<Tour>>()))
            .Callback<IEnumerable<Tour>>(t => captured = t);

        await _service.ImportToursAsync(_userId, dtos);

        Assert.That(captured!.First().TransportType, Is.EqualTo(TourPlanner.DAL.Entities.Enums.TransportType.Hike));
    }
}
