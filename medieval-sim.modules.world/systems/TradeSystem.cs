using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;
using medieval_sim.modules.world.services;

namespace medieval_sim.modules.world.systems;

public sealed class TradeSystem : ISystem
{
    public int Order => 40;

    private const double CaravanCapacity = 120.0; // units

    public void Tick(EngineContext ctx)
    {
        // One matching run per day at 12:00
        if (ctx.Clock.Now.Hour != 12) return;

        var routes = ctx.Resolve<RouteBook>();

        // collect settlements
        var all = ctx.World.Components
            .Where(kv => kv.Value is Settlement)
            .Select(kv => (id: kv.Key, s: (Settlement)kv.Value))
            .ToList();

        // build sellers/buyers
        var sellers = new List<(EntityId id, Settlement s, Faction f, SettlementMarket m, double avail)>();
        var buyers = new List<(EntityId id, Settlement s, Faction f, SettlementMarket m, double need)>();

        foreach (var (id, s) in all)
        {
            var f = ctx.World.Get<Faction>(s.FactionId);
            var m = ctx.World.Get<SettlementMarket>(s.MarketId);

            double pop = Math.Max(1, s.Pop);
            double perDay = f.Policy.DailyFoodPerPerson * pop;
            double total = s.FoodStock + s.Households.Sum(h => h.Food);
            double target = f.Policy.BufferDays * perDay;

            if (total > target + 1)
            {
                // offer only from public stock beyond what we'd need for tomorrow's top-up
                double requiredForTopUpTomorrow = Math.Max(0, target - s.Households.Sum(h => h.Food));
                double granarySurplus = Math.Max(0, s.FoodStock - requiredForTopUpTomorrow);
                if (granarySurplus > 0.1) sellers.Add((id, s, f, m, granarySurplus));
            }
            else if (total < target - 1)
            {
                double need = (target - total);
                buyers.Add((id, s, f, m, need));
            }
        }

        if (buyers.Count == 0 || sellers.Count == 0) return;

        // Intra-faction first
        Match(ctx, routes, buyers, sellers, preferSameFaction: true);
        // Then cross-faction (respect relation/policy)
        Match(ctx, routes, buyers, sellers, preferSameFaction: false);
    }

    private static void Match(
        EngineContext ctx, RouteBook routes,
        List<(EntityId id, Settlement s, Faction f, SettlementMarket m, double need)> buyers,
        List<(EntityId id, Settlement s, Faction f, SettlementMarket m, double avail)> sellers,
        bool preferSameFaction)
    {
        for (int i = 0; i < buyers.Count; i++)
        {
            var b = buyers[i];
            if (b.need <= 0) continue;

            var candidates = sellers
                .Where(sl =>
                {
                    bool same = sl.f == b.f;
                    if (preferSameFaction && !same) return false;
                    if (!preferSameFaction && same) return false;

                    if (!same)
                    {
                        if (!b.f.Policy.WillTradeExternally || !sl.f.Policy.WillTradeExternally) return false;
                        var buyerFId = ctx.World.Components.First(kv => kv.Value == b.f).Key;
                        var sellerFId = ctx.World.Components.First(kv => kv.Value == sl.f).Key;
                        int relAB = b.f.Relations.TryGetValue(sellerFId, out var r1) ? r1 : 0;
                        int relBA = sl.f.Relations.TryGetValue(buyerFId, out var r2) ? r2 : 0;
                        int thresh = Math.Max(b.f.Policy.MinRelationToTrade, sl.f.Policy.MinRelationToTrade);
                        if (relAB < thresh || relBA < thresh) return false;
                    }
                    return sl.avail > 0.1;
                })
                // cheaper sellers first (use seller's local price)
                .OrderBy(sl => sl.m.PriceFood)
                .ThenByDescending(sl => sl.avail)
                .ToList();

            double remaining = b.need;

            for (int j = 0; j < candidates.Count && remaining > 0; j++)
            {
                var si = candidates[j];
                double sellerAvail = si.avail;

                // travel time
                double hours = routes.Hours(si.id, b.id);
                if (double.IsPositiveInfinity(hours)) continue;

                // Unit price = seller's current market price
                double price = si.m.PriceFood;

                // Compute max possible considering availability, capacity, and buyer treasury (price + fee + toll)
                double feeRate = (b.m.FeeRateOverride >= 0 ? b.m.FeeRateOverride : b.f.Policy.Taxes.MarketFeeRate);
                double tollPerUnit = b.f.Policy.Taxes.TransitTollPerUnit;

                // how much buyer can fund?
                double denom = price + (price * feeRate) + tollPerUnit;
                double affordable = denom > 0 ? (b.f.Treasury / denom) : 0;

                double qty = Math.Min(remaining, Math.Min(si.avail, affordable));
                if (qty <= 0) continue;

                // Respect caravan capacity: split into trips
                while (qty > 0.0001)
                {
                    double ship = Math.Min(qty, CaravanCapacity);

                    // lock funds now; pay seller on arrival
                    double cost = ship * price;
                    double fee = cost * feeRate;
                    double toll = ship * tollPerUnit;

                    b.f.Treasury -= (cost + fee + toll);
                    si.avail -= ship;
                    remaining -= ship;

                    // remove goods from seller now (they're in transit)
                    si.s.FoodStock -= ship;

                    // destination earns fees immediately (gate/market control)
                    b.f.Treasury += 0; // fees/tolls already deducted; if you prefer accrual on arrival, move credits there
                    si.f.Treasury += 0;

                    var buyerSettlement = b.s;
                    var sellerFaction = si.f;
                    var buyerFaction = b.f;

                    // credit fee/toll to destination faction immediately
                    buyerFaction.Treasury += fee + toll;

                    // schedule arrival
                    var deliverQty = ship;
                    var deliverCost = cost;
                    ctx.Scheduler.Schedule(ctx.Clock.Now.AddHours(hours), () =>
                    {
                        // goods arrive to dest granary; seller receives payment
                        buyerSettlement.FoodStock += deliverQty;
                        sellerFaction.Treasury += deliverCost;
                    });

                    qty -= ship;
                }

                // write back seller availability
                int sidx = sellers.FindIndex(x => ReferenceEquals(x.s, si.s));
                if (sidx >= 0) sellers[sidx] = (si.id, si.s, si.f, si.m, si.avail);
            }

            buyers[i] = (b.id, b.s, b.f, b.m, remaining);
        }
    }
}