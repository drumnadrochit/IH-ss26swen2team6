using TourPlanner.DAL.Entities.Enums;

namespace TourPlanner.BL.Strategies;

// One strategy per TransportType. Adding a new transport type only requires a new
// strategy class registered in DI - no switch statement to extend (Open/Closed).
public interface ITransportSpeedStrategy
{
    TransportType Type { get; }
    double AverageSpeedKmh { get; }
}

public class BikeSpeedStrategy : ITransportSpeedStrategy
{
    public TransportType Type => TransportType.Bike;
    public double AverageSpeedKmh => 18;
}

public class HikeSpeedStrategy : ITransportSpeedStrategy
{
    public TransportType Type => TransportType.Hike;
    public double AverageSpeedKmh => 4.5;
}

public class RunningSpeedStrategy : ITransportSpeedStrategy
{
    public TransportType Type => TransportType.Running;
    public double AverageSpeedKmh => 10;
}

public class VacationSpeedStrategy : ITransportSpeedStrategy
{
    public TransportType Type => TransportType.Vacation;
    public double AverageSpeedKmh => 70;
}
