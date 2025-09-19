using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class FoodProductionSystem : ISystem
{
    public int Order => 10;

    public void Tick(EngineContext ctx)
    {
        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Settlement s) continue;
            var f = ctx.World.Get<Faction>(s.FactionId);
            var mods = TryGet<SettlementModifiers>(ctx, s) ?? new SettlementModifiers();
            var market = ctx.World.Get<SettlementMarket>(s.MarketId);

            // Baseline: ~1.1 units/person/day => 0.045 per hour
            double perHour = 0.045 * System.Math.Max(0, s.ProductionMultiplier); // ~1.08/day at 1.0x
            double prod = perHour * (s.Households.Count > 0 ? s.Households.Sum(h => h.Size) : s.Pop);
            s.FoodStock += prod;

            // monetized tithe on production at current local price
            double titheCoins = prod * f.Policy.Taxes.TitheRate * market.PriceFood;
            f.Treasury += titheCoins;

            // Book supply offered later (set at 08:00 each day)
            // nothing here; see MarketPricingSystem
        }
    }

    private static int CurrentPop(Settlement s)
    {
        if (s.Households.Count > 0)
        {
            int sum = 0; foreach (var h in s.Households) sum += h.Size;
            s.Pop = sum;
        }
        return s.Pop;
    }

    private static T? TryGet<T>(EngineContext ctx, Settlement s) where T : class
    {
        foreach (var kv in ctx.World.Components)
            if (ReferenceEquals(kv.Value, s)) break;
        // No stable reverse lookup in this simple store; we’ll assume a modifier exists via separate id if you add it.
        return null;
    }
}