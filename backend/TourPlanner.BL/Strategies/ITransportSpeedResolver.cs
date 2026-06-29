using TourPlanner.DAL.Entities.Enums;

namespace TourPlanner.BL.Strategies;

public interface ITransportSpeedResolver
{
    double GetAverageSpeedKmh(TransportType type);
}

public class TransportSpeedResolver : ITransportSpeedResolver
{
    private readonly Dictionary<TransportType, ITransportSpeedStrategy> _strategies;

    public TransportSpeedResolver(IEnumerable<ITransportSpeedStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Type);
    }

    public double GetAverageSpeedKmh(TransportType type)
        => _strategies.TryGetValue(type, out var strategy) ? strategy.AverageSpeedKmh : 10;
}
