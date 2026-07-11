using NUnit.Framework;
using TourPlanner.BL.Utils;

namespace TourPlanner.Tests.Utils;

[TestFixture]
public class GeoUtilsTests
{
    [Test]
    public void HaversineKm_SamePoint_ReturnsZero()
    {
        var distance = GeoUtils.HaversineKm(48.2, 16.37, 48.2, 16.37);
        Assert.That(distance, Is.EqualTo(0).Within(0.001));
    }

    [Test]
    public void HaversineKm_ViennaToSalzburg_ReturnsApproximateDistance()
    {
        // Vienna (48.2082, 16.3738) to Salzburg (47.8095, 13.0550) is ~250km as the crow flies.
        var distance = GeoUtils.HaversineKm(48.2082, 16.3738, 47.8095, 13.0550);
        Assert.That(distance, Is.InRange(240, 260));
    }
}
