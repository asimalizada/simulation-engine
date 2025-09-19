using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.economy.components;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.economy.systems;

public sealed class MarketSystem : ISystem
{
    public int Order => 30;
    private const double Alpha = 0.1;

    public void Tick(EngineContext ctx)
    {
        // Aggregate supply/demand from settlements (toy)
        double supply = 0, demand = 0;
        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is Settlement s)
            {
                supply += Math.Max(0, s.FoodStock - 10); // surplus above buffer
                demand += Math.Max(0, 10 - s.FoodStock); // shortage below buffer
            }
        }
        var m = ctx.Resolve<Market>();
        m.LastSupply = supply; m.LastDemand = demand;
        m.FoodPrice = Math.Max(0.01, m.FoodPrice * (1 + Alpha * (demand - supply) / Math.Max(1.0, supply)));
    }
}