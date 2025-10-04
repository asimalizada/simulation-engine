using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class PassionAssignmentSystem : ISystem
{
    public int Order => 6;

    public void Tick(EngineContext ctx)
    {
        var rng = ctx.Rng;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Settlement s) continue;

            foreach (var hh in s.Households)
            {
                foreach (var p in hh.People)
                {
                    if (p.Passions.Count > 0) continue;

                    var primary = PassionMaps.PrimaryFor(p.Profession);
                    p.Passions.Add(new PassionIntensity { Passion = primary, Level = 0.6 + rng.NextDouble() * 0.35 }); // 0.6..0.95

                    // up to 2 secondaries (light)
                    foreach (var sec in PassionMaps.SecondaryFor(p.Profession).OrderBy(_ => rng.NextDouble()).Take(rng.Next(0, 2)))
                        p.Passions.Add(new PassionIntensity { Passion = sec, Level = 0.25 + rng.NextDouble() * 0.35 }); // 0.25..0.6

                    // small chance of a wildcard passion for diversity
                    if (rng.NextDouble() < 0.10)
                    {
                        var any = (Passion)rng.Next(0, System.Enum.GetValues(typeof(Passion)).Length);
                        if (!p.Passions.Any(x => x.Passion == any))
                            p.Passions.Add(new PassionIntensity { Passion = any, Level = 0.2 + rng.NextDouble() * 0.25 });
                    }
                }
            }
        }
    }
}
