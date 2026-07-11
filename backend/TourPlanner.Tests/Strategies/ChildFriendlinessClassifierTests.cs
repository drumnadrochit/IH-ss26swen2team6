using NUnit.Framework;
using TourPlanner.BL.Strategies;
using TourPlanner.DAL.Entities;

namespace TourPlanner.Tests.Strategies;

[TestFixture]
public class ChildFriendlinessClassifierTests
{
    private readonly DefaultChildFriendlinessClassifier _classifier = new();

    [Test]
    public void Classify_WithNoLogs_ReturnsUnknown()
    {
        var result = _classifier.Classify([]);
        Assert.That(result, Is.EqualTo("Unknown"));
    }

    [Test]
    public void Classify_WithEasyShortLogs_ReturnsYes()
    {
        var logs = new[]
        {
            new TourLog { Difficulty = 1, TotalDistance = 5, TotalTime = 60 },
            new TourLog { Difficulty = 2, TotalDistance = 8, TotalTime = 90 }
        };

        Assert.That(_classifier.Classify(logs), Is.EqualTo("Yes"));
    }

    [Test]
    public void Classify_WithModerateLogs_ReturnsModerate()
    {
        var logs = new[]
        {
            new TourLog { Difficulty = 3, TotalDistance = 25, TotalTime = 250 }
        };

        Assert.That(_classifier.Classify(logs), Is.EqualTo("Moderate"));
    }

    [Test]
    public void Classify_WithHardLongLogs_ReturnsNo()
    {
        var logs = new[]
        {
            new TourLog { Difficulty = 5, TotalDistance = 60, TotalTime = 600 }
        };

        Assert.That(_classifier.Classify(logs), Is.EqualTo("No"));
    }
}
