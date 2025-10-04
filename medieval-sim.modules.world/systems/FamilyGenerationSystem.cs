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

        var settlements = ctx.World.Components
            .Where(kv => kv.Value is Settlement)
            .Select(kv => (id: kv.Key, s: (Settlement)kv.Value))
            .ToList();

        foreach (var (sid, s) in settlements)
        {
            if (HasFamilies(ctx, sid)) continue;

            // create the families component for THIS settlement
            var famCompId = ctx.World.Create();
            var famComp = new SettlementFamilies { SettlementId = sid };
            ctx.World.Set(famCompId, famComp);

            // group 1–3 households per family
            int h = 0;
            while (h < s.Households.Count)
            {
                int groupSize = Math.Max(1, Math.Min(3, 1 + (int)(rng.NextDouble() * 3)));
                var fam = new Family { Surname = SurnameFromSpecialtyOrRandom(ctx, s, rng) };

                for (int k = 0; k < groupSize && h < s.Households.Count; k++, h++)
                {
                    var hh = s.Households[h];
                    fam.HouseholdIndices.Add(h);

                    for (int pi = 0; pi < hh.People.Count; pi++)
                    {
                        var p = hh.People[pi];
                        // temporary; will be the same after we push
                        p.FamilyIndex = famComp.Families.Count;
                        fam.Members.Add(new PersonRef(sid, h, pi));
                    }
                }

                famComp.Families.Add(fam);

                // set correct final index on people
                int finalIndex = famComp.Families.Count - 1;
                foreach (var m in fam.Members)
                    s.Households[m.HouseholdIndex].People[m.PersonIndex].FamilyIndex = finalIndex;
            }

            // strengthen intra-family relations
            foreach (var (fi, family) in famComp.Families.Select((f, fi) => (fi, f)))
            {
                foreach (var meRef in family.Members)
                {
                    var me = s.Households[meRef.HouseholdIndex].People[meRef.PersonIndex];
                    foreach (var otherRef in family.Members)
                    {
                        if (otherRef.Equals(meRef)) continue;

                        var existing = me.Relations.FirstOrDefault(r => r.Target.Equals(otherRef));
                        if (existing is not null)
                            existing.Score = Math.Min(100, existing.Score + 20);
                        else
                            me.Relations.Add(new Relation
                            {
                                Target = otherRef,
                                Score = 25 + (int)(rng.NextDouble() * 25) // +25..+50
                            });
                    }
                }
            }
        }
    }

    private static bool HasFamilies(EngineContext ctx, EntityId settlementId)
        => ctx.World.Components.Any(kv => kv.Value is SettlementFamilies sf && sf.SettlementId.Equals(settlementId));

    private static string SurnameFromSpecialtyOrRandom(EngineContext ctx, Settlement s, IRng rng)
    {
        string[] baseNames = { "Oakheart", "Rivers", "Hillman", "Miller", "Smith", "Baker", "Weaver", "Stone", "Carter", "Fletcher", "Cooper", "Barley", "Goodman" };

        var spec = ctx.World.Components
            .Where(kv => kv.Value is SettlementSpecialties)
            .Select(kv => (SettlementSpecialties)kv.Value)
            .FirstOrDefault();

        if (spec is not null && spec.Weights.Count > 0)
        {
            var top = spec.Weights.OrderByDescending(x => x.Value).First().Key;
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