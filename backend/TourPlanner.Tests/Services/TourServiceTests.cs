using Moq;
using NUnit.Framework;
using TourPlanner.BL.Exceptions;
using TourPlanner.BL.DTOs;
using TourPlanner.BL.HttpClients;
using TourPlanner.BL.Services;
using TourPlanner.BL.Strategies;
using TourPlanner.DAL.Entities;
using TourPlanner.DAL.Entities.Enums;
using TourPlanner.DAL.Repositories.Interfaces;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class TourServiceTests
{
    private Mock<ITourRepository> _tourRepoMock = null!;
    private Mock<IOpenRouteServiceClient> _orsMock = null!;
    private TourService _tourService = null!;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _tourRepoMock = new Mock<ITourRepository>();
        _orsMock = new Mock<IOpenRouteServiceClient>();
        _tourService = new TourService(
            _tourRepoMock.Object,
            _orsMock.Object,
            new DefaultChildFriendlinessClassifier(),
            new TransportSpeedResolver(new ITransportSpeedStrategy[]
            {
                new BikeSpeedStrategy(), new HikeSpeedStrategy(),
                new RunningSpeedStrategy(), new VacationSpeedStrategy()
            }));
        _userId = Guid.NewGuid();

        _orsMock.Setup(o => o.GeocodeAsync(It.IsAny<string>()))
            .ReturnsAsync((14.3, 48.3));
        _orsMock.Setup(o => o.GetDirectionsAsync(
                It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<TransportType>()))
            .ReturnsAsync((10.5, 45, (double[][]?)null));
    }

    [Test]
    public async Task CreateTour_WithValidData_ReturnsTourResponse()
    {
        _tourRepoMock.Setup(r => r.AddAsync(It.IsAny<Tour>())).ReturnsAsync((Tour t) => t);
        var request = new CreateTourRequest("Hiking Trail", "Nice hike", "Vienna", "Salzburg", TransportType.Hike);

        var result = await _tourService.CreateTourAsync(request, _userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Hiking Trail"));
        Assert.That(result.Distance, Is.EqualTo(10.5));
        _tourRepoMock.Verify(r => r.AddAsync(It.IsAny<Tour>()), Times.Once);
    }

    [Test]
    public void CreateTour_WithEmptyName_ThrowsArgumentException()
    {
        var request = new CreateTourRequest("", "desc", "Vienna", "Salzburg", TransportType.Bike);
        Assert.ThrowsAsync<DomainValidationException>(() => _tourService.CreateTourAsync(request, _userId));
    }

    [Test]
    public async Task UpdateTour_ByOwner_ReturnsUpdatedTour()
    {
        var existingTour = new Tour
        {
            Id = Guid.NewGuid(), Name = "Old", Description = "",
            From = "A", To = "B", TransportType = TransportType.Bike,
            UserId = _userId, TourLogs = []
        };
        _tourRepoMock.Setup(r => r.GetByIdAsync(existingTour.Id)).ReturnsAsync(existingTour);
        _tourRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>())).Returns(Task.CompletedTask);

        var result = await _tourService.UpdateTourAsync(
            existingTour.Id,
            new UpdateTourRequest("New Name", "desc", "A", "B", TransportType.Bike),
            _userId);

        Assert.That(result.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public void UpdateTour_ByNonOwner_ThrowsUnauthorizedException()
    {
        var tour = new Tour { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tour.Id)).ReturnsAsync(tour);

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _tourService.UpdateTourAsync(tour.Id,
                new UpdateTourRequest("n", "d", "a", "b", TransportType.Bike), _userId));
    }

    [Test]
    public void DeleteTour_ByNonOwner_ThrowsUnauthorizedException()
    {
        var tour = new Tour { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tour.Id)).ReturnsAsync(tour);

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _tourService.DeleteTourAsync(tour.Id, _userId));
    }

    [Test]
    public async Task DeleteTour_ByOwner_CallsRepoDelete()
    {
        var tourId = Guid.NewGuid();
        var tour = new Tour { Id = tourId, UserId = _userId };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tourId)).ReturnsAsync(tour);
        _tourRepoMock.Setup(r => r.DeleteAsync(tourId)).Returns(Task.CompletedTask);

        await _tourService.DeleteTourAsync(tourId, _userId);

        _tourRepoMock.Verify(r => r.DeleteAsync(tourId), Times.Once);
    }

    [Test]
    public async Task GetTours_ReturnsUserTours()
    {
        var tours = new List<Tour>
        {
            new() { Id = Guid.NewGuid(), Name = "Tour A", UserId = _userId, TourLogs = [] },
            new() { Id = Guid.NewGuid(), Name = "Tour B", UserId = _userId, TourLogs = [] }
        };
        _tourRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(tours);

        var result = (await _tourService.GetToursAsync(_userId)).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task SearchTours_MatchingName_ReturnsTour()
    {
        var tours = new List<Tour>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpine Hike", UserId = _userId, TourLogs = [] },
            new() { Id = Guid.NewGuid(), Name = "Beach Walk", UserId = _userId, TourLogs = [] }
        };
        _tourRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(tours);

        var result = (await _tourService.SearchToursAsync(_userId, "alpine")).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Alpine Hike"));
    }

    [Test]
    public async Task SearchTours_MatchingLogComment_ReturnsTour()
    {
        var tours = new List<Tour>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Mystery Tour", UserId = _userId,
                TourLogs = [new TourLog { Comment = "saw a beautiful waterfall" }]
            }
        };
        _tourRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(tours);

        var result = (await _tourService.SearchToursAsync(_userId, "waterfall")).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task SearchTours_MatchingComputedChildFriendliness_ReturnsTour()
    {
        var tours = new List<Tour>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Easy Loop", UserId = _userId,
                TourLogs = [new TourLog { Difficulty = 1, TotalDistance = 5, TotalTime = 60 }]
            }
        };
        _tourRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(tours);

        var result = (await _tourService.SearchToursAsync(_userId, "Yes")).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task SearchTours_NoMatch_ReturnsEmpty()
    {
        var tours = new List<Tour> { new() { Id = Guid.NewGuid(), Name = "Forest Run", UserId = _userId, TourLogs = [] } };
        _tourRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(tours);

        var result = (await _tourService.SearchToursAsync(_userId, "nonexistent")).ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SetTourImage_ByOwner_UpdatesRouteImagePath()
    {
        var tour = new Tour { Id = Guid.NewGuid(), UserId = _userId, TourLogs = [] };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tour.Id)).ReturnsAsync(tour);
        _tourRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>())).Returns(Task.CompletedTask);

        var result = await _tourService.SetTourImageAsync(tour.Id, _userId, "/uploads/abc.jpg");

        Assert.That(result.RouteImagePath, Is.EqualTo("/uploads/abc.jpg"));
    }

    [Test]
    public void SetTourImage_ByNonOwner_ThrowsUnauthorizedException()
    {
        var tour = new Tour { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tour.Id)).ReturnsAsync(tour);

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _tourService.SetTourImageAsync(tour.Id, _userId, "/uploads/abc.jpg"));
    }

    [Test]
    public async Task GetTourStartCoordinates_WhenKnown_ReturnsCoordinates()
    {
        var tour = new Tour { Id = Guid.NewGuid(), UserId = _userId, FromLat = 48.2, FromLon = 16.4 };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tour.Id)).ReturnsAsync(tour);

        var result = await _tourService.GetTourStartCoordinatesAsync(tour.Id, _userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.Lat, Is.EqualTo(48.2));
    }

    [Test]
    public async Task GetTourStartCoordinates_WhenUnknown_ReturnsNull()
    {
        var tour = new Tour { Id = Guid.NewGuid(), UserId = _userId, FromLat = null, FromLon = null };
        _tourRepoMock.Setup(r => r.GetByIdAsync(tour.Id)).ReturnsAsync(tour);

        var result = await _tourService.GetTourStartCoordinatesAsync(tour.Id, _userId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateTour_WhenDirectionsFail_FallsBackToHaversineEstimate()
    {
        _orsMock.Setup(o => o.GeocodeAsync("Vienna")).ReturnsAsync((16.3738, 48.2082));
        _orsMock.Setup(o => o.GeocodeAsync("Salzburg")).ReturnsAsync((13.0550, 47.8095));
        _orsMock.Setup(o => o.GetDirectionsAsync(
                It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<TransportType>()))
            .ReturnsAsync(((double distance, int duration, double[][]? coordinates)?)null);
        _tourRepoMock.Setup(r => r.AddAsync(It.IsAny<Tour>())).ReturnsAsync((Tour t) => t);

        var result = await _tourService.CreateTourAsync(
            new CreateTourRequest("Cross Country", "desc", "Vienna", "Salzburg", TransportType.Vacation), _userId);

        Assert.That(result.Distance, Is.GreaterThan(0));
        Assert.That(result.EstimatedTime, Is.GreaterThan(0));
    }
}
