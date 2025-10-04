using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.core.RNG;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

// Runs until all households have People == Size, then becomes a no-op.
public sealed class PopulationGenerationSystem : ISystem
{
    public int Order => 5;

    public void Tick(EngineContext ctx)
    {
        var rng = ctx.Rng;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Settlement s) continue;

            // Ensure specialties + economy exist
            var spec = TryGet<SettlementSpecialties>(ctx, s);
            var econ = ctx.World.Get<SettlementEconomy>(s.EconomyId);

            foreach (var (hh, hIndex) in s.Households.Select((h, i) => (h, i)))
            {
                if (hh.People.Count >= hh.Size)
                    continue;

                // fill the household with Size people
                while (hh.People.Count < hh.Size)
                {
                    var prof = SampleProfession(rng, spec);
                    int age = SampleAge(rng); // adults dominate
                    double skill = Math.Clamp(age * 1.2 + rng.NextDouble() * 10, 0, 100);

                    hh.People.Add(new Person
                    {
                        Age = age,
                        Profession = prof,
                        Skill = skill
                    });
                }
            }

            // very light-weight relations: add 2 random acquaintances per person within settlement
            var sid = FindSettlementId(ctx, s);
            var all = s.Households.SelectMany((h, hi) => h.People.Select((p, pi) => (person: p, pref: new PersonRef(sid, hi, pi)))).ToList();

            foreach (var entry in all)
            {
                var p = entry.person;
                var pref = entry.pref;

                for (int k = 0; k < 2; k++)
                {
                    var other = all[rng.Next(0, all.Count)];
                    if (other.pref.Equals(pref)) continue;

                    int score = rng.Next(-5, 21); // -5..20
                    p.Relations.Add(new Relation { Target = other.pref, Score = score });
                }
            }
        }
    }

    private static Profession SampleProfession(IRng rng, SettlementSpecialties? spec)
    {
        // baseline weights
        var baseW = new (Profession p, double w)[]
        {
                (Profession.Farmer, 3.0),(Profession.Miller,0.3),(Profession.Baker,0.6),
                (Profession.Blacksmith,0.5),(Profession.Carpenter,0.6),(Profession.Mason,0.3),
                (Profession.Fisher,0.4),(Profession.Hunter,0.4),(Profession.Woodcutter,0.5),
                (Profession.Merchant,0.5),(Profession.Scribe,0.2),(Profession.Healer,0.2),
                (Profession.Priest,0.2),(Profession.Monk,0.2),(Profession.Guard,0.6),
                (Profession.Soldier,0.5),(Profession.Noble,0.05),(Profession.Caravaneer,0.3),
                (Profession.Brewer,0.3)
        };

        double sum = 0;
        foreach (var b in baseW)
            sum += b.w * (spec != null && spec.Weights.TryGetValue(b.p, out var boost) ? boost : 1.0);

        double r = rng.NextDouble() * sum;
        foreach (var b in baseW)
        {
            double w = b.w * (spec != null && spec.Weights.TryGetValue(b.p, out var boost) ? boost : 1.0);
            if (r <= w) return b.p;
            r -= w;
        }
        return Profession.Farmer;
    }

    private static int SampleAge(IRng rng)
    {
        // 16..70 with peak around 28–40
        double u = rng.NextDouble();
        return (int)Math.Clamp(16 + Math.Sqrt(u) * 40 + rng.NextDouble() * 10, 16, 70);
    }

    private static SettlementSpecialties? TryGet<SettlementSpecialties>(EngineContext ctx, Settlement s)
        => ctx.World.Components.Where(kv => ReferenceEquals(kv.Value, s)).Select(_ => default(SettlementSpecialties)).FirstOrDefault();

    private static EntityId FindSettlementId(EngineContext ctx, Settlement s)
        => ctx.World.Components.First(kv => ReferenceEquals(kv.Value, s)).Key;
}