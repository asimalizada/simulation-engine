using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.core.events;
using medieval_sim.core.RNG;
using medieval_sim.core.time;
using medieval_sim.modules.world;
using medieval_sim.modules.world.components;
using System.Diagnostics;

public static class Program
{
    public static async Task Main()
    {
        // ==== Build engine context ====
        var clock = new FixedStepClock(new DateTime(1200, 3, 1, 6, 0, 0), TimeSpan.FromHours(1)); // 1 sim hour per tick
        var world = new WorldState();
        var rng = new SeededRng(1337);
        var eventsBus = new EventBus();
        var scheduler = new MinHeapScheduler();
        var ctx = new EngineContext(world, clock, rng, eventsBus, scheduler);

        // Load modules
        var builder = new EngineBuilder();
        IModule[] modules = { new WorldModule() };
        foreach (var m in modules) m.Configure(builder);

        var engine = new Engine(ctx, clock);
        
        foreach (var sys in builder.Systems)
            engine.AddSystem(sys);
        
        foreach (var m in modules)
            m.Bootstrap(ctx);

        // ==== Long-running loop config ====
        // Real-time pacing: 10 ticks per second (each tick advances 1 sim hour by our FixedStepClock)
        var tickInterval = TimeSpan.FromMilliseconds(100);
        var statusEvery = TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        Console.Title = "MedievalSim Sandbox";
        Console.WriteLine("MedievalSim Sandbox running. Press Ctrl+C to stop.");
        Console.WriteLine($"Sim step: {clock.Now} (each tick = {TimeSpan.FromHours(1)}).");

        var statusTimer = Stopwatch.StartNew();
        var timer = new PeriodicTimer(tickInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(cts.Token))
            {
                // Run exactly 1 simulation tick per real-time tick
                engine.RunTicks(1);

                // Periodic status
                if (statusTimer.Elapsed >= statusEvery)
                {
                    statusTimer.Restart();
                    DumpStatus(ctx, clock);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        Console.WriteLine();
        Console.WriteLine("Stopping… Final snapshot:");
        DumpStatus(ctx, clock, final: true);
    }

    private static void DumpStatus(EngineContext ctx, IClock clock, bool final = false)
    {
        var mark = final ? "[FINAL]" : "[STATUS]";
        Console.WriteLine($"{mark} {clock.Now:yyyy-MM-dd HH:mm}");

        // ===== snapshot world once to avoid 'modified during enumeration' =====
        var comps = ctx.World.Components.ToList();

        var factions = comps
            .Where(kv => kv.Value is Faction)
            .Select(kv => (id: kv.Key, f: (Faction)kv.Value))
            .ToList();

        var settlements = comps
            .Where(kv => kv.Value is Settlement)
            .Select(kv => (id: kv.Key, s: (Settlement)kv.Value))
            .ToList();

        var leadershipByFaction = comps
            .Where(kv => kv.Value is FactionLeadership)
            .Select(kv => (FactionLeadership)kv.Value)
            .ToDictionary(x => x.OwnerFactionId, x => x);

        // ===== factions =====
        foreach (var (fid, f) in factions)
        {
            var leaderTxt = leadershipByFaction.TryGetValue(fid, out var L)
                ? $"  Leaders: Sovereign: {ShortLeader(ctx, L.Sovereign)} / Chancellor: {ShortLeader(ctx, L.Chancellor)} / Marshall: {ShortLeader(ctx, L.Marshal)}"
                : string.Empty;

            Console.WriteLine($"  Faction: {f.Name}  Treasury={Math.Round(f.Treasury, 1)}  Taxes={f.Policy.Taxes.TitheRate:P0}/{f.Policy.Taxes.MarketFeeRate:P0}/{f.Policy.Taxes.TransitTollPerUnit:0.00}{leaderTxt}");
        }

        // ===== settlements =====
        foreach (var (sid, s) in settlements)
        {
            var f = ctx.World.Get<Faction>(s.FactionId);
            var m = ctx.World.Get<SettlementMarket>(s.MarketId);
            var econ = ctx.World.Get<SettlementEconomy>(s.EconomyId);

            // quick sums
            double hhFood = 0, hhWealth = 0; int pop = 0;
            foreach (var hh in s.Households) { hhFood += hh.Food; hhWealth += hh.Wealth; pop += hh.Size; }
            if (pop <= 0) pop = Math.Max(1, s.Pop); // fallback cache

            double perDay = f.Policy.DailyFoodPerPerson * pop;
            double daysBuffered = perDay > 0 ? (hhFood + s.FoodStock) / perDay : 0;

            // families (if present)
            int famCount = comps
                .Where(kv => kv.Value is SettlementFamilies sf && sf.SettlementId.Equals(sid))
                .Select(kv => ((SettlementFamilies)kv.Value).Families.Count)
                .FirstOrDefault();

            Console.WriteLine(
                $"  - {s.Name} [{f.Name}]: " +
                $"Pop={pop}, Price={m.PriceFood:0.00}{(m.IsFairDay ? " (fair)" : "")}, " +
                $"Granary={Math.Round(s.FoodStock, 1)}, HHFood={Math.Round(hhFood, 1)}, " +
                $"~{daysBuffered:0.0}d, WagePool={Math.Round(econ.WagePoolCoins, 1)}, " +
                $"Families={famCount}, Missed={s.MealsMissedToday:0.0}");
        }

        // ---- local helpers ----
        static string ShortLeader(EngineContext c, PersonRef? pr)
        {
            if (pr is null) return "--";
            var (sid, hi, pi) = pr.Value;
            var s = c.World.Get<Settlement>(sid);
            if (hi < 0 || hi >= s.Households.Count) return "--";
            var hh = s.Households[hi];
            if (pi < 0 || pi >= hh.People.Count) return "--";
            var p = hh.People[pi];
            return $"{p.GivenName} {p.FamilyName}-{p.Profession}-{p.Age}";
        }
    }
}
