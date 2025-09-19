using medieval_sim.core.ECS;

namespace medieval_sim.modules.world.services;

public sealed class RouteBook
{
    private readonly Dictionary<(int a, int b), double> _hours = new();

    public void Set(EntityId a, EntityId b, double hours)
    {
        _hours[(a.Value, b.Value)] = hours;
        _hours[(b.Value, a.Value)] = hours;
    }

    public double Hours(EntityId a, EntityId b)
        => _hours.TryGetValue((a.Value, b.Value), out var h) ? h : double.PositiveInfinity;
}