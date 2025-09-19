namespace medieval_sim.core.ECS;

public sealed class WorldState : IWorldState
{
    private int _next = 1;

    public Dictionary<EntityId, object> Components { get; } = new();
    
    public EntityId Create() => new(_next++);
    
    public T Get<T>(EntityId id) where T : class => Components.TryGetValue(id, out var o) ? (T)o : throw new KeyNotFoundException();
    
    public void Set<T>(EntityId id, T c) where T : class => Components[id] = c!;
}