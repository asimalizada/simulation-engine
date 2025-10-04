using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.core.RNG;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

// Runs until a settlement has generated all persons once, then no-ops.
public sealed class PopulationGenerationSystem : ISystem
{
    public int Order => 5;

    public void Tick(EngineContext ctx)
    {
        var rng = ctx.Rng;

        // SNAPSHOT settlements once
        var settlements = ctx.World.Components
            .Where(kv => kv.Value is Settlement)
            .Select(kv => (id: kv.Key, s: (Settlement)kv.Value))
            .ToList();

        foreach (var (sid, s) in settlements)
        {
            // Optional fast gate: store a flag on Settlement so we don't redo work every tick
            // Add this field to Settlement if you haven't yet: public bool PopulationInitialized;
            if (s.PopulationInitialized) continue;

            // Get specialties if you attached one to this settlement
            var spec = s.SpecialtiesId.Value != default ? ctx.World.Get<SettlementSpecialties>(s.SpecialtiesId) : null;

            // Fill households with people
            for (int h = 0; h < s.Households.Count; h++)
            {
                var hh = s.Households[h];
                while (hh.People.Count < hh.Size)
                {
                    var prof = SampleProfession(rng, spec);
                    int age = SampleAge(rng);
                    double skill = Math.Clamp(age * 1.2 + rng.NextDouble() * 10, 0, 100);

                    hh.People.Add(new Person
                    {
                        Age = age,
                        Profession = prof,
                        Skill = skill
                    });
                }
            }

            // Build a flat index of people with their PersonRef
            var all = new List<(Person person, PersonRef pref)>(s.Pop);
            for (int hi = 0; hi < s.Households.Count; hi++)
            {
                var hh = s.Households[hi];
                for (int pi = 0; pi < hh.People.Count; pi++)
                    all.Add((hh.People[pi], new PersonRef(sid, hi, pi)));
            }

            // Add 2 random acquaintances per person (no duplicates, no self)
            for (int i = 0; i < all.Count; i++)
            {
                var (p, pref) = all[i];

                int links = 0;
                int guard = 0;
                while (links < 2 && guard < 16) // guard avoids infinite loop if very small populations
                {
                    guard++;
                    var (op, oref) = all[rng.Next(0, all.Count)];
                    if (oref.Equals(pref)) continue;

                    // already have a relation?
                    if (p.Relations.Any(r => r.Target.Equals(oref))) continue;

                    int score = rng.Next(-5, 21); // -5..20
                    p.Relations.Add(new Relation { Target = oref, Score = score });
                    links++;
                }
            }

            s.PopulationInitialized = true; // <-- set the flag so we don't run again
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

        // sum with specialty multipliers
        double sum = 0;
        for (int i = 0; i < baseW.Length; i++)
        {
            var (p, w) = baseW[i];
            if (spec != null && spec.Weights.TryGetValue(p, out var boost)) w *= boost;
            sum += w;
            baseW[i].w = w; // reuse weighted value
        }

        double r = rng.NextDouble() * sum;
        for (int i = 0; i < baseW.Length; i++)
        {
            if (r <= baseW[i].w) return baseW[i].p;
            r -= baseW[i].w;
        }
        return Profession.Farmer;
    }

    private static int SampleAge(IRng rng)
    {
        // 16..70 with peak around ~30s
        double u = rng.NextDouble();
        return (int)Math.Clamp(16 + Math.Sqrt(u) * 40 + rng.NextDouble() * 10, 16, 70);
    }
}