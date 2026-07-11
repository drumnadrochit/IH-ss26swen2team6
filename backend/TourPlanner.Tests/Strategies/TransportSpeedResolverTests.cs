using NUnit.Framework;
using TourPlanner.BL.Strategies;
using TourPlanner.DAL.Entities.Enums;

namespace TourPlanner.Tests.Strategies;

[TestFixture]
public class TransportSpeedResolverTests
{
    private readonly TransportSpeedResolver _resolver = new(new ITransportSpeedStrategy[]
    {
        new BikeSpeedStrategy(), new HikeSpeedStrategy(),
        new RunningSpeedStrategy(), new VacationSpeedStrategy()
    });

    [TestCase(TransportType.Bike, 18)]
    [TestCase(TransportType.Hike, 4.5)]
    [TestCase(TransportType.Running, 10)]
    [TestCase(TransportType.Vacation, 70)]
    public void GetAverageSpeedKmh_ReturnsExpectedSpeedPerType(TransportType type, double expected)
    {
        Assert.That(_resolver.GetAverageSpeedKmh(type), Is.EqualTo(expected));
    }

    [Test]
    public void GetAverageSpeedKmh_WithNoMatchingStrategy_ReturnsFallback()
    {
        var resolver = new TransportSpeedResolver([]);
        Assert.That(resolver.GetAverageSpeedKmh(TransportType.Bike), Is.EqualTo(10));
    }
}
