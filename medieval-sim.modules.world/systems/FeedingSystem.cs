using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class FeedingSystem : ISystem
{
    public int Order => 20;

    public void Tick(EngineContext ctx)
    {
        var now = ctx.Clock.Now;

        // Reset daily diagnostics at midnight
        if (now.Hour == 0)
            foreach (var kv in ctx.World.Components)
                if (kv.Value is Settlement s) s.MealsMissedToday = 0;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Settlement s) continue;
            var f = ctx.World.Get<Faction>(s.FactionId);
            var pol = f.Policy;
            var mkt = ctx.World.Get<SettlementMarket>(s.MarketId);

            // Replenish just before earliest meal
            int earliest = pol.MealHours.Min();
            if (now.Hour == ((earliest + 24 - 1) % 24))
            {
                // Express supply for pricing statistics this day
                mkt.SupplyToday = s.FoodStock;

                foreach (var hh in s.Households)
                {
                    double target = pol.BufferDays * pol.DailyFoodPerPerson * hh.Size;
                    double needed = Math.Max(0, target - hh.Food);
                    if (needed <= 0) continue;

                    // Demand bookkeeping (for price adjustment next day)
                    mkt.DemandToday += needed;

                    // Purchase from public stock if available and affordable
                    double feeRate = (mkt.FeeRateOverride >= 0 ? mkt.FeeRateOverride : f.Policy.Taxes.MarketFeeRate);
                    if (mkt.IsFairDay) feeRate *= 0.5;

                    double maxAffordable = mkt.PriceFood > 0 ? (hh.Wealth / (mkt.PriceFood * (1 + feeRate))) : 0;
                    double purch = Math.Min(needed, Math.Min(maxAffordable, s.FoodStock));
                    if (purch <= 0) continue;

                    double cost = purch * mkt.PriceFood;
                    double fee = cost * feeRate;

                    hh.Wealth -= (cost + fee);
                    f.Treasury += fee;       // market fee revenue
                    s.FoodStock -= purch;
                    hh.Food += purch;
                }
            }

            // Serve meals at policy hours
            if (pol.MealHours.Contains(now.Hour))
            {
                double portionPerPerson = pol.DailyFoodPerPerson / pol.MealHours.Length;
                foreach (var hh in s.Households)
                {
                    double need = portionPerPerson * hh.Size;

                    // 1) eat from household reserves
                    double eat = Math.Min(need, hh.Food);
                    hh.Food -= eat;
                    double remaining = need - eat;

                    // 2) ration fallback: pull the remaining directly from the public granary (free at point of use)
                    if (remaining > 0 && s.FoodStock > 0)
                    {
                        double ration = Math.Min(remaining, s.FoodStock);
                        s.FoodStock -= ration;
                        remaining -= ration;
                    }

                    if (remaining > 0)
                        s.MealsMissedToday += remaining / portionPerPerson;
                }

            }
        }
    }
}