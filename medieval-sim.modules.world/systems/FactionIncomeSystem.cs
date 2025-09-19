using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class FactionIncomeSystem : ISystem
{
    // before feeding/trade
    public int Order => 15; 

    public void Tick(EngineContext ctx)
    {
        // Flat daily income at 06:00 (once per day)
        if (ctx.Clock.Now.Hour != 6) return;

        foreach (var kv in ctx.World.Components)
        {
            //if (kv.Value is Faction f)
            //    f.Treasury += f.Policy.DailyIncome;
        }
    }
}