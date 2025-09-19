using medieval_sim.core.engine;

namespace medieval_sim.core.persistence;

public interface ISnapshotStore
{
    Snapshot Capture(EngineContext ctx);
    void Restore(EngineContext ctx, Snapshot snap);
}
