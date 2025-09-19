using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class MarketPricingSystem : ISystem
{
    public int Order => 18; // before FeedingSystem (which buys at replenish)

    private const double Alpha = 0.08;

    public void Tick(EngineContext ctx)
    {
        var now = ctx.Clock.Now;

        // 07:00 — adjust price using yesterday's flows
        if (now.Hour == 7)
        {
            foreach (var kv in ctx.World.Components)
            {
                if (kv.Value is not Settlement s) continue;
                var m = ctx.World.Get<SettlementMarket>(s.MarketId);
                var supply = Math.Max(1.0, m.LastSupply);
                var demand = m.LastDemand;

                // tâtonnement
                m.PriceFood = Math.Max(0.01, m.PriceFood * (1 + Alpha * (demand - supply) / supply));
            }
        }

        // 23:00 — roll today's flows into "last", clear, and end any fair flags
        if (now.Hour == 23)
        {
            foreach (var kv in ctx.World.Components)
            {
                if (kv.Value is not Settlement s) continue;
                var m = ctx.World.Get<SettlementMarket>(s.MarketId);
                m.LastDemand = m.DemandToday;
                m.LastSupply = m.SupplyToday;
                m.DemandToday = 0;
                m.SupplyToday = 0;
                m.IsFairDay = false; // weekly fairs last one day
            }
        }
    }
}
