using TourPlanner.DAL.Entities;

namespace TourPlanner.BL.Strategies;

public interface IChildFriendlinessClassifier
{
    string Classify(IEnumerable<TourLog> logs);
}
