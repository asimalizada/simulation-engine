namespace medieval_sim.core.ECS;

public interface IWorldState
{
    // Minimal: add typed stores; replace later with high-perf struct arrays if needed.
    Dictionary<EntityId, object> Components { get; }
    T Get<T>(EntityId id) where T : class;
    void Set<T>(EntityId id, T component) where T : class;
    EntityId Create();
}
