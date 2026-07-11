using Moq;
using NUnit.Framework;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.Services;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Repositories.Interfaces;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class TourLogServiceTests
{
    private Mock<ITourLogRepository> _logRepoMock = null!;
    private Mock<ITourRepository> _tourRepoMock = null!;
    private TourLogService _service = null!;
    private Guid _userId;
    private Guid _tourId;

    [SetUp]
    public void SetUp()
    {
        _logRepoMock = new Mock<ITourLogRepository>();
        _tourRepoMock = new Mock<ITourRepository>();
        _service = new TourLogService(_logRepoMock.Object, _tourRepoMock.Object);
        _userId = Guid.NewGuid();
        _tourId = Guid.NewGuid();
    }

    [Test]
    public async Task CreateLog_WithValidData_ReturnsLogResponse()
    {
        var tour = new Tour { Id = _tourId, UserId = _userId };
        _tourRepoMock.Setup(r => r.GetByIdAsync(_tourId)).ReturnsAsync(tour);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<TourLog>())).ReturnsAsync((TourLog l) => l);

        var result = await _service.CreateLogAsync(_tourId,
            new CreateTourLogRequest(DateTime.UtcNow, "Great hike!", 3, 15.5, 120, 4), _userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Comment, Is.EqualTo("Great hike!"));
        Assert.That(result.Difficulty, Is.EqualTo(3));
    }

    [Test]
    public void CreateLog_WithInvalidDifficulty_ThrowsArgumentException()
    {
        var tour = new Tour { Id = _tourId, UserId = _userId };
        _tourRepoMock.Setup(r => r.GetByIdAsync(_tourId)).ReturnsAsync(tour);

        Assert.ThrowsAsync<DomainValidationException>(() =>
            _service.CreateLogAsync(_tourId,
                new CreateTourLogRequest(DateTime.UtcNow, "test", 0, 10, 60, 3), _userId));
    }

    [Test]
    public void CreateLog_WithInvalidRating_ThrowsArgumentException()
    {
        var tour = new Tour { Id = _tourId, UserId = _userId };
        _tourRepoMock.Setup(r => r.GetByIdAsync(_tourId)).ReturnsAsync(tour);

        Assert.ThrowsAsync<DomainValidationException>(() =>
            _service.CreateLogAsync(_tourId,
                new CreateTourLogRequest(DateTime.UtcNow, "test", 3, 10, 60, 6), _userId));
    }

    [Test]
    public async Task GetLogs_ForOwnTour_ReturnsLogs()
    {
        var tour = new Tour { Id = _tourId, UserId = _userId };
        _tourRepoMock.Setup(r => r.GetByIdAsync(_tourId)).ReturnsAsync(tour);
        _logRepoMock.Setup(r => r.GetByTourIdAsync(_tourId)).ReturnsAsync(new List<TourLog>
        {
            new()
            {
                Id = Guid.NewGuid(), TourId = _tourId, UserId = _userId,
                Comment = "Log 1", Difficulty = 2, Rating = 4,
                TotalDistance = 10, TotalTime = 60, DateTime = DateTime.UtcNow
            }
        });

        var result = await _service.GetLogsAsync(_tourId, _userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetLogs_ForOtherUsersTour_ThrowsKeyNotFoundException()
    {
        var tour = new Tour { Id = _tourId, UserId = Guid.NewGuid() };
        _tourRepoMock.Setup(r => r.GetByIdAsync(_tourId)).ReturnsAsync(tour);

        Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetLogsAsync(_tourId, _userId));
    }

    [Test]
    public async Task DeleteLog_ByOwner_DeletesSuccessfully()
    {
        var logId = Guid.NewGuid();
        var log = new TourLog { Id = logId, TourId = _tourId, UserId = _userId };
        _logRepoMock.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);
        _logRepoMock.Setup(r => r.DeleteAsync(logId)).Returns(Task.CompletedTask);

        await _service.DeleteLogAsync(_tourId, logId, _userId);

        _logRepoMock.Verify(r => r.DeleteAsync(logId), Times.Once);
    }

    [Test]
    public void DeleteLog_ByNonOwner_ThrowsUnauthorizedException()
    {
        var logId = Guid.NewGuid();
        var log = new TourLog { Id = logId, TourId = _tourId, UserId = Guid.NewGuid() };
        _logRepoMock.Setup(r => r.GetByIdAsync(logId)).ReturnsAsync(log);

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _service.DeleteLogAsync(_tourId, logId, _userId));
    }
}
