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
        Console.WriteLine($"{mark} Time={clock.Now}");

        // Factions
        foreach (var kv in ctx.World.Components)
            if (kv.Value is Faction f)
                Console.WriteLine($"  Faction: {f.Name}  Treasury={Math.Round(f.Treasury, 1)}  Taxes(Tithe/Market/Toll)={f.Policy.Taxes.TitheRate:P0}/{f.Policy.Taxes.MarketFeeRate:P0}/{f.Policy.Taxes.TransitTollPerUnit:0.00}");

        // Settlements
        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is Settlement s)
            {
                var f = ctx.World.Get<Faction>(s.FactionId);
                var m = ctx.World.Get<SettlementMarket>(s.MarketId);
                double perDay = f.Policy.DailyFoodPerPerson * Math.Max(1, s.Pop);
                double householdFood = 0; double wealth = 0;
                foreach (var hh in s.Households) { householdFood += hh.Food; wealth += hh.Wealth; }
                double daysBuffered = perDay > 0 ? (householdFood + s.FoodStock) / perDay : 0;
                Console.WriteLine($"  - {s.Name} [{f.Name}]: Price={m.PriceFood:0.00}{(m.IsFairDay ? " (fair)" : "")}, Granary={Math.Round(s.FoodStock, 1)}, HHFood={Math.Round(householdFood, 1)} (~{daysBuffered:0.0}d), MissedMealsToday≈{s.MealsMissedToday:0.0}, HH wealth≈{wealth:0.0}");
            }
        }
    }
}
