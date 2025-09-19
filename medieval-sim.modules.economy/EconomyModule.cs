using medieval_sim.core.engine;
using medieval_sim.modules.economy.components;
using medieval_sim.modules.economy.systems;

namespace medieval_sim.modules.economy;

public sealed class EconomyModule : IModule
{
    public string Name => "Economy";

    public int LoadOrder => 1;
    
    public void Configure(EngineBuilder b) => b.AddSystem(new MarketSystem());
    
    public void Bootstrap(EngineContext ctx) => ctx.Register(new Market { Scope = "Realm" });
}