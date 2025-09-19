using medieval_sim.core.ECS;

namespace medieval_sim.core.engine;

public sealed class EngineBuilder
{
    private readonly List<ISystem> _systems = new();
    
    public EngineBuilder AddSystem(ISystem s)
    {
        _systems.Add(s);
        return this;
    }
    
    public IReadOnlyList<ISystem> Systems => _systems;
}