namespace medieval_sim.core.engine;

public interface IModule
{
    string Name { get; }
    int LoadOrder { get; } // load dependencies first if needed
    void Configure(EngineBuilder b); // add systems/services
    void Bootstrap(EngineContext ctx); // seed initial entities, schedules
}
