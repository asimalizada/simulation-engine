using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;
using medieval_sim.modules.world.services;
using medieval_sim.modules.world.systems;

namespace medieval_sim.modules.world;

public sealed class WorldModule : IModule
{
    public string Name => "World";
    public int LoadOrder => 0;

    public void Configure(EngineBuilder b)
    {
        b.AddSystem(new MarketPricingSystem())
         .AddSystem(new HouseholdIncomeSystem())
         .AddSystem(new FoodProductionSystem())
         .AddSystem(new FeedingSystem())
         .AddSystem(new TradeSystem());
    }

    public void Bootstrap(EngineContext ctx)
    {
        // ===== Factions =====
        var fEld = ctx.World.Create();
        ctx.World.Set(fEld, new Faction
        {
            Name = "Crown of Eldermere",
            Treasury = 120,
            Policy = new FactionPolicy
            {
                DailyFoodPerPerson = 1.0,
                BufferDays = 5,
                MealHours = new[] { 9, 18 },
                WillTradeExternally = true,
                MinRelationToTrade = -10,
                Taxes = new TaxPolicy { TitheRate = 0.08, MarketFeeRate = 0.05, TransitTollPerUnit = 0.02 }
            }
        });

        var fLeague = ctx.World.Create();
        ctx.World.Set(fLeague, new Faction
        {
            Name = "Free Cities League",
            Treasury = 200,
            Policy = new FactionPolicy
            {
                DailyFoodPerPerson = 1.2,
                BufferDays = 4,
                MealHours = new[] { 8, 13, 19 },
                WillTradeExternally = true,
                MinRelationToTrade = 0,
                Taxes = new TaxPolicy { TitheRate = 0.05, MarketFeeRate = 0.06, TransitTollPerUnit = 0.03 }
            }
        });

        var fOrder = ctx.World.Create();
        ctx.World.Set(fOrder, new Faction
        {
            Name = "Grey Monastic Order",
            Treasury = 60,
            Policy = new FactionPolicy
            {
                DailyFoodPerPerson = 0.9,
                BufferDays = 6,
                MealHours = new[] { 10, 18 },
                WillTradeExternally = true,
                MinRelationToTrade = -5,
                Taxes = new TaxPolicy { TitheRate = 0.10, MarketFeeRate = 0.03, TransitTollPerUnit = 0.01 }
            }
        });

        // Relations
        ctx.World.Get<Faction>(fEld).Relations[fLeague] = +20;
        ctx.World.Get<Faction>(fLeague).Relations[fEld] = +20;
        ctx.World.Get<Faction>(fEld).Relations[fOrder] = +5;
        ctx.World.Get<Faction>(fOrder).Relations[fEld] = +5;

        // ===== Markets helper
        EntityId NewMarket(string name)
        {
            var mid = ctx.World.Create();
            ctx.World.Set(mid, new SettlementMarket { Name = name, PriceFood = 1.0 });
            return mid;
        }

        // ===== Settlements + households
        var a = ctx.World.Create();
        ctx.World.Set(a, new Settlement
        {
            Name = "Rivenshade",
            FactionId = fEld,
            MarketId = NewMarket("Rivenshade Market"),
            FoodStock = 80
        });
        SeedHouseholds(ctx, a, pop: 320, wealthAvg: 6, wealthVar: 3);

        var b = ctx.World.Create();
        ctx.World.Set(b, new Settlement
        {
            Name = "Stoneford",
            FactionId = fEld,
            MarketId = NewMarket("Stoneford Market"),
            FoodStock = 40
        });
        SeedHouseholds(ctx, b, pop: 180, wealthAvg: 4, wealthVar: 2);

        var c = ctx.World.Create();
        ctx.World.Set(c, new Settlement
        {
            Name = "Port Kelda",
            FactionId = fLeague,
            MarketId = NewMarket("Port Kelda Exchange"),
            FoodStock = 220
        });
        SeedHouseholds(ctx, c, pop: 220, wealthAvg: 10, wealthVar: 4);

        var d = ctx.World.Create();
        ctx.World.Set(d, new Settlement
        {
            Name = "Grey Abbey",
            FactionId = fOrder,
            MarketId = NewMarket("Abbey Court"),
            FoodStock = 15
        });
        SeedHouseholds(ctx, d, pop: 120, wealthAvg: 3, wealthVar: 1.5);

        // ===== Distance graph (hours)
        var routes = new RouteBook();
        routes.Set(a, b, hours: 8);
        routes.Set(a, c, hours: 30);
        routes.Set(b, c, hours: 26);
        routes.Set(b, d, hours: 10);
        routes.Set(c, d, hours: 24);
        ctx.Register(routes);

        // ===== Weekly fairs & random bad harvests
        ScheduleWeeklyFair(ctx, a);
        ScheduleWeeklyFair(ctx, b);
        ScheduleWeeklyFair(ctx, c);
        ScheduleWeeklyFair(ctx, d);

        ScheduleRandomBadHarvest(ctx, a);
        ScheduleRandomBadHarvest(ctx, b);
        ScheduleRandomBadHarvest(ctx, c);
        ScheduleRandomBadHarvest(ctx, d);
    }

    // ----- helpers -----
    private static void SeedHouseholds(EngineContext ctx, EntityId sid, int pop, double wealthAvg, double wealthVar)
    {
        var s = ctx.World.Get<Settlement>(sid);
        var rng = ctx.Rng;
        s.Households.Clear();
        int remaining = pop;

        while (remaining > 0)
        {
            int size = Math.Max(2, Math.Min(6, rng.Next(2, 7)));
            if (size > remaining) size = remaining;

            double w = Math.Max(0.5, wealthAvg + (rng.NextDouble() * 2 - 1) * wealthVar);
            double food = Math.Max(0, rng.NextDouble() * size * 2); // up to ~2 days

            s.Households.Add(new Household { Size = size, Wealth = w, Food = food });
            remaining -= size;
        }

        // cache pop
        s.Pop = pop;
    }

    private static void ScheduleWeeklyFair(EngineContext ctx, EntityId sid)
    {
        void ScheduleNext(DateTime from)
        {
            var next = from.Date.AddDays(7).AddHours(6); // starts 06:00 once a week
            ctx.Scheduler.Schedule(next, () =>
            {
                var s = ctx.World.Get<Settlement>(sid);
                var m = ctx.World.Get<SettlementMarket>(s.MarketId);
                m.IsFairDay = true;          // FeedingSystem halves fee on fair days
                                             // Optional: inject a bit more supply signal
                m.SupplyToday += Math.Max(0, s.FoodStock * 0.10);
                // End of day reset happens in MarketPricingSystem at 23:00
                ScheduleNext(next);          // re-schedule next week
            });
        }
        ScheduleNext(ctx.Clock.Now);
    }
    
    private static void ScheduleRandomBadHarvest(EngineContext ctx, EntityId sid)
    {
        void Next(DateTime from)
        {
            int days = 30 + ctx.Rng.Next(0, 91);
            var start = from.Date.AddDays(days).AddHours(5);

            ctx.Scheduler.Schedule(start, () =>
            {
                var s = ctx.World.Get<Settlement>(sid);
                var old = s.ProductionMultiplier;
                s.ProductionMultiplier = 0.5; // -50%

                // end after 30 days
                ctx.Scheduler.Schedule(start.AddDays(30), () =>
                {
                    s.ProductionMultiplier = old;
                });

                Next(start);
            });
        }
        Next(ctx.Clock.Now);
    }

}