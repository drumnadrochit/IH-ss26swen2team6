using TourPlanner.DAL.Entities;

namespace TourPlanner.DAL.UnitOfWork;

// Wraps multi-entity operations (e.g. importing tours with their logs) in a single
// SaveChanges call so they commit atomically instead of each repository auto-saving.
public interface IUnitOfWork
{
    void AddTours(IEnumerable<Tour> tours);
    Task<int> SaveChangesAsync();
}
