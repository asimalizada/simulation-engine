using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.core.RNG;
using medieval_sim.modules.world.components;
using medieval_sim.modules.world.services;
using System.Text.RegularExpressions;

namespace medieval_sim.modules.world.systems;

// Runs until a settlement has generated all persons once, then no-ops.
public sealed class PopulationGenerationSystem : ISystem
{
    public int Order => 5;

    public void Tick(EngineContext ctx)
    {
        var rng = ctx.Rng;

        var settlements = ctx.World.Components
            .Where(kv => kv.Value is Settlement)
            .Select(kv => (id: kv.Key, s: (Settlement)kv.Value))
            .ToList();

        foreach (var (sid, s) in settlements)
        {
            if (s.PopulationInitialized) continue;

            // Get specialties if you attached one to this settlement
            var spec = s.SpecialtiesId.Value != default ? ctx.World.Get<SettlementSpecialties>(s.SpecialtiesId) : null;

            // We’ll need culture for naming and (optionally) kind for profession bias
            var culture = s.Culture;
            var kind = s.Kind;

            var uniq = ctx.Resolve<UniqueNameRegistry>();

            // Fill households with people
            for (int h = 0; h < s.Households.Count; h++)
            {
                var hh = s.Households[h];

                while (hh.People.Count < hh.Size)
                {
                    var prof = SampleProfession(rng, spec, culture, kind);
                    int age = SampleAge(rng);
                    double skill = Math.Clamp(age * 1.2 + rng.NextDouble() * 10, 0, 100);

                    // Culture-aware name generation
                    string family = NameGenerator.NextFamily(rng, culture);
                    var (given, gender) = NameGenerator.NextGiven(rng, culture);

                    string reserved = uniq.ReservePerson($"{given} {family}");
                    int sp = reserved.LastIndexOf(' ');
                    string finalGiven = sp > 0 ? reserved[..sp] : reserved;
                    string finalFamily = sp > 0 ? reserved[(sp + 1)..] : "";

                    hh.People.Add(new Person
                    {
                        Age = age,
                        Profession = prof,
                        Skill = skill,
                        GivenName = finalGiven,
                        FamilyName = finalFamily,
                        Gender = gender
                    });
                }

                for (int i = 0; i < hh.People.Count; i++)
                {
                    var p = hh.People[i];
                    if (string.IsNullOrWhiteSpace(p.GivenName))
                    {
                        string family = string.IsNullOrWhiteSpace(p.FamilyName)
                            ? NameGenerator.NextFamily(rng, culture)
                            : p.FamilyName;

                        var (given, gender) = NameGenerator.NextGiven(rng, culture);
                        string reserved = uniq.ReservePerson($"{given} {family}");
                        int sp = reserved.LastIndexOf(' ');
                        p.GivenName = sp > 0 ? reserved[..sp] : reserved;
                        p.FamilyName = sp > 0 ? reserved[(sp + 1)..] : "";
                        p.Gender = gender;
                    }
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

    private static Profession SampleProfession(IRng rng, SettlementSpecialties? spec, Culture culture, SettlementKind kind)
    {
        // base weights
        var W = new Dictionary<Profession, double>
        {
            [Profession.Farmer] = 2.2,[Profession.Shepherd] = 0.6,[Profession.Fisher] = 0.6,[Profession.Hunter] = 0.6,
            [Profession.Woodcutter] = 0.6,[Profession.Carpenter] = 0.7,[Profession.Mason] = 0.5,[Profession.Miner] = 0.5,
            [Profession.Tanner] = 0.3,[Profession.Weaver] = 0.5,[Profession.Dyer] = 0.3,[Profession.Potter] = 0.3,
            [Profession.Glassblower] = 0.2,[Profession.Leatherworker] = 0.3,[Profession.Tailor] = 0.5,[Profession.Blacksmith] = 0.6,
            [Profession.Jeweler] = 0.3,[Profession.Shipwright] = 0.2,[Profession.Alchemist] = 0.2,[Profession.Brewer] = 0.4,
            [Profession.Merchant] = 0.6,[Profession.Caravaneer] = 0.3,[Profession.Sailor] = 0.2,[Profession.Scribe] = 0.3,
            [Profession.Healer] = 0.3,[Profession.Priest] = 0.2,[Profession.Monk] = 0.2,[Profession.Bard] = 0.2,
            [Profession.Cook] = 0.3,[Profession.Guard] = 0.5,[Profession.Soldier] = 0.5,[Profession.Noble] = 0.05
        };

        void B(Profession p, double m) => W[p] = Math.Max(0.0, W[p] * m);

        // culture flavor
        switch (culture)
        {
            case Culture.Keshari: B(Profession.Merchant, 1.4); B(Profession.Caravaneer, 1.5); B(Profession.Scribe, 1.2); B(Profession.Farmer, 0.85); break;
            case Culture.Tzanel: B(Profession.Farmer, 1.6); B(Profession.Healer, 1.3); B(Profession.Blacksmith, 0.8); break;
            case Culture.Yura: B(Profession.Miner, 1.6); B(Profession.Mason, 1.4); B(Profession.Hunter, 1.2); B(Profession.Farmer, 0.8); break;
            case Culture.Ashari: B(Profession.Healer, 1.5); B(Profession.Hunter, 1.3); B(Profession.Woodcutter, 1.2); B(Profession.Blacksmith, 0.8); break;
            case Culture.Shokai: B(Profession.Blacksmith, 1.3); B(Profession.Shipwright, 1.5); B(Profession.Scribe, 1.2); B(Profession.Farmer, 0.9); break;
            case Culture.Norren: B(Profession.Hunter, 1.6); B(Profession.Fisher, 1.2); B(Profession.Guard, 1.2); B(Profession.Farmer, 0.75); break;
            case Culture.Zhurkan: B(Profession.Soldier, 1.6); B(Profession.Guard, 1.3); B(Profession.Caravaneer, 1.2); B(Profession.Scribe, 0.8); break;
            case Culture.Kaenji: B(Profession.Blacksmith, 1.5); B(Profession.Mason, 1.4); B(Profession.Alchemist, 1.4); B(Profession.Farmer, 0.9); break;
            case Culture.Aerani: B(Profession.Sailor, 1.8); B(Profession.Fisher, 1.5); B(Profession.Merchant, 1.3); B(Profession.Miner, 0.8); break;
            case Culture.Qazari: B(Profession.Blacksmith, 1.5); B(Profession.Mason, 1.3); B(Profession.Priest, 1.2); B(Profession.Farmer, 0.9); break;
        }

        // settlement kind flavor
        if (kind == SettlementKind.Port) { B(Profession.Sailor, 2.0); B(Profession.Fisher, 1.4); }
        if (kind == SettlementKind.Caravanserai) { B(Profession.Caravaneer, 2.0); B(Profession.Merchant, 1.3); }
        if (kind == SettlementKind.Castle) { B(Profession.Guard, 1.8); B(Profession.Soldier, 1.6); B(Profession.Blacksmith, 1.2); }
        if (kind == SettlementKind.Mine) { B(Profession.Miner, 2.0); B(Profession.Mason, 1.3); B(Profession.Blacksmith, 1.2); }
        if (kind == SettlementKind.City) { B(Profession.Merchant, 1.3); B(Profession.Scribe, 1.3); B(Profession.Healer, 1.2); }

        // specialties (your existing weights)
        if (spec != null)
            foreach (var kv in spec.Weights)
                W[kv.Key] = Math.Max(0.0, W.GetValueOrDefault(kv.Key, 0.0) * kv.Value);

        // roulette
        double sum = 0; foreach (var v in W.Values) sum += v;
        double r = rng.NextDouble() * sum;
        foreach (var kv in W)
        {
            if ((r -= kv.Value) <= 0) return kv.Key;
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