using medieval_sim.core.engine;

namespace medieval_sim.core.ECS;

public interface ISystem
{
    // Order decides deterministic side-effects; keep pure where possible.
    void Tick(EngineContext ctx);
    int Order { get; } // lower runs first
}
