using TourPlanner.DAL.Entities;

namespace TourPlanner.BL.Strategies;

// Derives child-friendliness from recorded difficulty, distance and time of all logs of a tour.
public class DefaultChildFriendlinessClassifier : IChildFriendlinessClassifier
{
    public string Classify(IEnumerable<TourLog> logs)
    {
        var list = logs.ToList();
        if (list.Count == 0) return "Unknown";

        var avgDifficulty = list.Average(l => l.Difficulty);
        var avgDistance = list.Average(l => l.TotalDistance);
        var avgTime = list.Average(l => l.TotalTime);

        if (avgDifficulty <= 2 && avgDistance <= 15 && avgTime <= 180) return "Yes";
        if (avgDifficulty <= 3 && avgDistance <= 30 && avgTime <= 300) return "Moderate";
        return "No";
    }
}
