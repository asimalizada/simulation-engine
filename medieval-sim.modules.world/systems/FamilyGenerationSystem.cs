using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.core.RNG;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class FamilyGenerationSystem : ISystem
{
    public int Order => 7;

    public void Tick(EngineContext ctx)
    {
        var rng = ctx.Rng;

        foreach (var pair in ctx.World.Components)
        {
            if (pair.Value is not Settlement s) continue;

            // If already generated, skip
            if (HasFamilies(ctx, s)) continue;

            var famCompId = ctx.World.Create();
            var famComp = new SettlementFamilies { SettlementId = FindSettlementId(ctx, s) };
            ctx.World.Set(famCompId, famComp);

            // Group households into 1–3 household families ("clans")
            int h = 0;
            while (h < s.Households.Count)
            {
                int groupSize = Math.Max(1, Math.Min(3, 1 + (int)(rng.NextDouble() * 3))); // 1..3
                var fam = new Family { Surname = SurnameFromSpecialtyOrRandom(ctx, s, rng) };

                for (int k = 0; k < groupSize && h < s.Households.Count; k++, h++)
                {
                    var hh = s.Households[h];
                    fam.HouseholdIndices.Add(h);

                    // assign all persons in this household to this family
                    for (int pi = 0; pi < hh.People.Count; pi++)
                    {
                        var p = hh.People[pi];
                        p.FamilyIndex = famComp.Families.Count; // temporary index before adding; updated after push
                        fam.Members.Add(new PersonRef(famComp.SettlementId, h, pi));
                    }
                }

                famComp.Families.Add(fam);
                // Fix indices on people now that we know final index
                int finalIndex = famComp.Families.Count - 1;
                foreach (var m in fam.Members)
                {
                    var hh = s.Households[m.HouseholdIndex];
                    hh.People[m.PersonIndex].FamilyIndex = finalIndex;
                }
            }

            // Strengthen intra-family relations
            var allMembers = famComp.Families
                .SelectMany((f, fi) => f.Members.Select(m => (fi, m)))
                .ToList();

            foreach (var (fi, m) in allMembers)
            {
                var hh = s.Households[m.HouseholdIndex];
                var me = hh.People[m.PersonIndex];

                foreach (var other in famComp.Families[fi].Members)
                {
                    if (other.Equals(m)) continue;
                    // if relation already exists, boost it; else add new positive tie
                    var existing = me.Relations.FirstOrDefault(r => r.Target.Equals(other));
                    if (existing is not null)
                    {
                        existing.Score = Math.Min(100, existing.Score + 20);
                    }
                    else
                    {
                        me.Relations.Add(new Relation { Target = other, Score = 25 + (int)(rng.NextDouble() * 25) }); // +25..+50
                    }
                }
            }
        }
    }

    private static bool HasFamilies(EngineContext ctx, Settlement s)
    {
        // naive: check if any SettlementFamilies exists for this settlement
        return ctx.World.Components.Any(kv => kv.Value is SettlementFamilies sf && sf.SettlementId.Equals(FindSettlementId(ctx, s)));
    }

    private static EntityId FindSettlementId(EngineContext ctx, Settlement s)
        => ctx.World.Components.First(kv => ReferenceEquals(kv.Value, s)).Key;

    private static string SurnameFromSpecialtyOrRandom(EngineContext ctx, Settlement s, IRng rng)
    {
        // very small on-purpose list; replace with richer generator later
        string[] baseNames = { "Oakheart", "Rivers", "Hillman", "Miller", "Smith", "Baker", "Weaver", "Stone", "Carter", "Fletcher", "Cooper", "Barley", "Goodman" };

        // if settlement has specialties, bias surname to match (fun flavor)
        var maybeSpec = ctx.World.Components.FirstOrDefault(kv => kv.Value is SettlementSpecialties).Value as SettlementSpecialties;
        if (maybeSpec is not null && maybeSpec.Weights.Count > 0)
        {
            var top = maybeSpec.Weights.OrderByDescending(x => x.Value).First().Key;
            return top switch
            {
                Profession.Blacksmith => "Smith",
                Profession.Miller => "Miller",
                Profession.Baker => "Baker",
                Profession.Carpenter => "Carpenter",
                Profession.Mason => "Stone",
                Profession.Weaver => "Weaver",
                Profession.Farmer => "Barley",
                Profession.Caravaneer => "Carter",
                _ => baseNames[rng.Next(0, baseNames.Length)]
            };
        }
        return baseNames[rng.Next(0, baseNames.Length)];
    }
}