using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

// Pays once per day at 06:00.
// Logic: give each person enough coins to buy that day's food at current local price (+5% buffer),
// bounded by the faction treasury (pro-rata if short).
public sealed class HouseholdIncomeSystem : ISystem
{
    public int Order => 14; // before feeding/market buying

    public void Tick(EngineContext ctx)
    {
        var now = ctx.Clock.Now;
        if (now.Hour != 6) return;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Settlement s) continue;
            var f = ctx.World.Get<Faction>(s.FactionId);
            var m = ctx.World.Get<SettlementMarket>(s.MarketId);

            int pop = s.Households.Sum(h => h.Size);
            if (pop <= 0) continue;

            double feeRate = (m.FeeRateOverride >= 0 ? m.FeeRateOverride : f.Policy.Taxes.MarketFeeRate);
            if (m.IsFairDay) feeRate *= 0.5;

            // cost to cover 1 day of food per person at current price (+5% buffer)
            double perPersonCost = f.Policy.DailyFoodPerPerson * m.PriceFood * (1 + feeRate) * 1.05;
            double totalNeeded = perPersonCost * pop;

            double budget = System.Math.Min(f.Treasury, totalNeeded);
            if (budget <= 0) continue;

            double perCapita = budget / pop;
            foreach (var hh in s.Households)
                hh.Wealth += perCapita * hh.Size;

            f.Treasury -= budget;
        }
    }
}