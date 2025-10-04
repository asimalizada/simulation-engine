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
        b.AddSystem(new PopulationGenerationSystem())
         .AddSystem(new PassionAssignmentSystem())
         .AddSystem(new FamilyGenerationSystem())
         .AddSystem(new MarketPricingSystem())
         .AddSystem(new LeadershipSelectionSystem())
         .AddSystem(new LeadershipStipendSystem())
         .AddSystem(new WageSystem())
         .AddSystem(new FoodProductionSystem())
         .AddSystem(new FeedingSystem())
         .AddSystem(new TradeSystem());
    }

    public void Bootstrap(EngineContext ctx)
    {
        // ===== Factions =====
        var fEld = ctx.World.Create();
        var cro = new Faction
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
        };
        ctx.World.Set(fEld, cro);
        cro.SelfId = fEld;

        var fLeague = ctx.World.Create();
        var fre = new Faction
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
        };
        ctx.World.Set(fLeague, fre);
        fre.SelfId = fLeague;

        var fOrder = ctx.World.Create();
        var gre = new Faction
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
        };
        ctx.World.Set(fOrder, gre);
        gre.SelfId = fOrder;

        // Relations
        ctx.World.Get<Faction>(fEld).Relations[fLeague] = +20;
        ctx.World.Get<Faction>(fLeague).Relations[fEld] = +20;
        ctx.World.Get<Faction>(fEld).Relations[fOrder] = +5;
        ctx.World.Get<Faction>(fOrder).Relations[fEld] = +5;

        // ===== Markets helper
        EntityId NewMarket(string name)
        {
            var mid = ctx.World.Create();
            var mkt = new SettlementMarket { Name = name, PriceFood = 1.0 };
            ctx.World.Set(mid, mkt);
            mkt.SelfId = mid;
            return mid;
        }

        // ===== Settlements + households (with Economy + Specialties)

        // Rivenshade — agrarian + brewing
        var a = ctx.World.Create();
        var riv = new Settlement
        {
            Name = "Rivenshade",
            FactionId = fEld,
            MarketId = NewMarket("Rivenshade Market"),
            EconomyId = NewEconomy(ctx, "Rivenshade Economy", e => { e.WagePoolCoins = 120; }),
            FoodStock = 80
        };
        ctx.World.Set(a, riv);
        riv.SelfId = a;
        AddSpecialties(ctx, a, sp =>
        {
            sp.Weights[Profession.Farmer] = 2.0;
            sp.Weights[Profession.Brewer] = 1.5;
            sp.Weights[Profession.Miller] = 1.3;
        });
        SeedHouseholds(ctx, a, pop: 320, wealthAvg: 6, wealthVar: 3);

        // Stoneford — farmers + woodcutters
        var b = ctx.World.Create();
        var sto = new Settlement
        {
            Name = "Stoneford",
            FactionId = fEld,
            MarketId = NewMarket("Stoneford Market"),
            EconomyId = NewEconomy(ctx, "Stoneford Economy", e => { e.WagePoolCoins = 80; }),
            FoodStock = 40
        };
        ctx.World.Set(b, sto);
        sto.SelfId = b;
        AddSpecialties(ctx, b, sp =>
        {
            sp.Weights[Profession.Farmer] = 1.6;
            sp.Weights[Profession.Woodcutter] = 1.5;
            sp.Weights[Profession.Carpenter] = 1.3;
        });
        SeedHouseholds(ctx, b, pop: 180, wealthAvg: 4, wealthVar: 2);

        // Port Kelda — merchants + smiths + caravaneers
        var c = ctx.World.Create();
        var por = new Settlement
        {
            Name = "Port Kelda",
            FactionId = fLeague,
            MarketId = NewMarket("Port Kelda Exchange"),
            EconomyId = NewEconomy(ctx, "Port Kelda Economy", e =>
            {
                e.WagePoolCoins = 200;
                e.DailyWage[Profession.Merchant] = 2.0;
                e.DailyWage[Profession.Blacksmith] = 1.8;
            }),
            FoodStock = 220
        };
        ctx.World.Set(c, por);
        por.SelfId = c;
        AddSpecialties(ctx, c, sp =>
        {
            sp.Weights[Profession.Merchant] = 2.5;
            sp.Weights[Profession.Blacksmith] = 1.7;
            sp.Weights[Profession.Caravaneer] = 1.5;
        });
        SeedHouseholds(ctx, c, pop: 220, wealthAvg: 10, wealthVar: 4);

        // Grey Abbey — scribes/monks/healers
        var d = ctx.World.Create();
        var abb = new Settlement
        {
            Name = "Grey Abbey",
            FactionId = fOrder,
            MarketId = NewMarket("Abbey Court"),
            EconomyId = NewEconomy(ctx, "Grey Abbey Economy", e =>
            {
                e.WagePoolCoins = 70;
                e.DailyWage[Profession.Scribe] = 1.7;
                e.DailyWage[Profession.Healer] = 1.6;
            }),
            FoodStock = 15
        };
        ctx.World.Set(d, abb);
        abb.SelfId = d;
        AddSpecialties(ctx, d, sp =>
        {
            sp.Weights[Profession.Monk] = 2.2;
            sp.Weights[Profession.Scribe] = 1.8;
            sp.Weights[Profession.Healer] = 1.4;
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
                m.IsFairDay = true;                 // FeedingSystem halves fee on fair days
                m.SupplyToday += Math.Max(0, s.FoodStock * 0.10); // small supply bump
                ScheduleNext(next);                 // schedule the following week
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

    private EntityId NewEconomy(EngineContext ctx, string name, Action<SettlementEconomy> cfg)
    {
        var id = ctx.World.Create();
        var e = new SettlementEconomy { Name = name };
        // default wages (tune as you like)
        foreach (Profession p in Enum.GetValues(typeof(Profession)))
            e.DailyWage[p] = p switch
            {
                Profession.Farmer => 1.0,
                Profession.Soldier or Profession.Guard => 1.2,
                Profession.Blacksmith or Profession.Carpenter => 1.5,
                Profession.Merchant or Profession.Scribe => 1.6,
                Profession.Noble => 2.0,
                _ => 1.1
            };
        cfg(e);
        ctx.World.Set(id, e);
        e.SelfId = id;
        return id;
    }

    private void AddSpecialties(EngineContext ctx, EntityId sid, Action<SettlementSpecialties> cfg)
    {
        var id = ctx.World.Create();
        var sp = new SettlementSpecialties();
        cfg(sp);
        ctx.World.Set(id, sp);

        // Link to the owning settlement
        var s = ctx.World.Get<Settlement>(sid);
        s.SpecialtiesId = id;
    }
}