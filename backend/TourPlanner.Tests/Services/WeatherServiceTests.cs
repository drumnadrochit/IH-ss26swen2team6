using Moq;
using NUnit.Framework;
using TourPlanner.BL.HttpClients;
using TourPlanner.BL.Services;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class WeatherServiceTests
{
    private Mock<IWeatherClient> _clientMock = null!;
    private WeatherService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _clientMock = new Mock<IWeatherClient>();
        _service = new WeatherService(_clientMock.Object);
    }

    [Test]
    public async Task GetCurrentWeatherAsync_WithSuccessfulResponse_MapsFields()
    {
        _clientMock.Setup(c => c.GetCurrentWeatherAsync(48.2, 16.4))
            .ReturnsAsync(new CurrentWeather { Temperature = 21.5, WindSpeed = 12.3, WeatherCode = 1, IsDay = 1 });

        var result = await _service.GetCurrentWeatherAsync(48.2, 16.4);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TemperatureCelsius, Is.EqualTo(21.5));
        Assert.That(result.Description, Is.EqualTo("Mainly clear"));
        Assert.That(result.IsDay, Is.True);
    }

    [Test]
    public async Task GetCurrentWeatherAsync_WithUnknownCode_ReturnsUnknownDescription()
    {
        _clientMock.Setup(c => c.GetCurrentWeatherAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new CurrentWeather { WeatherCode = 999, IsDay = 0 });

        var result = await _service.GetCurrentWeatherAsync(0, 0);

        Assert.That(result!.Description, Is.EqualTo("Unknown"));
        Assert.That(result.IsDay, Is.False);
    }

    [Test]
    public async Task GetCurrentWeatherAsync_WhenClientReturnsNull_ReturnsNull()
    {
        _clientMock.Setup(c => c.GetCurrentWeatherAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync((CurrentWeather?)null);

        var result = await _service.GetCurrentWeatherAsync(0, 0);

        Assert.That(result, Is.Null);
    }
}
