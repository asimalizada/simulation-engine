using medieval_sim.core.ECS;
using medieval_sim.core.time;

namespace medieval_sim.core.engine;

public sealed class Engine
{
    private readonly List<ISystem> _systems = new();
    private readonly EngineContext _ctx;
    private readonly FixedStepClock _clock;

    public Engine(EngineContext ctx, FixedStepClock clock)
    {
        _ctx = ctx;
        _clock = clock;
    }

    public Engine AddSystem(ISystem sys)
    {
        _systems.Add(sys);
        _systems.Sort((a, b) => a.Order.CompareTo(b.Order));
        return this;
    }

    public void RunTicks(int n)
    {
        for (int i = 0; i < n; i++)
        {
            _ctx.Scheduler.RunDue(_ctx.Clock.Now);      // scheduled actions due now
            foreach (var s in _systems)
                s.Tick(_ctx);    // main pipeline
            _ctx.Events.Drain();                         // optional: route/observe events
            _clock.Advance();
        }
    }
}
