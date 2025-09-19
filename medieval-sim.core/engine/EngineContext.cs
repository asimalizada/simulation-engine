using medieval_sim.core.ECS;
using medieval_sim.core.events;
using medieval_sim.core.RNG;
using medieval_sim.core.time;

namespace medieval_sim.core.engine;

public sealed class EngineContext
{
    public IWorldState World { get; }
    public IClock Clock { get; }
    public IRng Rng { get; }
    public IEventBus Events { get; }
    public IScheduler Scheduler { get; }

    // lightweight service registry (no external DI framework)
    private readonly Dictionary<Type, object> _services = new();

    public EngineContext(IWorldState w, IClock c, IRng r, IEventBus e, IScheduler s)
    {
        World = w;
        Clock = c;
        Rng = r;
        Events = e;
        Scheduler = s;
    }

    public void Register<T>(T service) where T : notnull => _services[typeof(T)] = service;

    public T Resolve<T>() where T : notnull => (T)_services[typeof(T)];
}