using TourPlanner.DAL.Context;
using TourPlanner.DAL.Entities;

namespace TourPlanner.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TourPlannerDbContext _context;

    public UnitOfWork(TourPlannerDbContext context) => _context = context;

    public void AddTours(IEnumerable<Tour> tours) => _context.Tours.AddRange(tours);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
