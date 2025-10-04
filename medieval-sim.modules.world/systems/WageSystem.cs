using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

// Pays once per day at 06:00 FROM the settlement WagePool (pro-rated if short).
public sealed class WageSystem : ISystem
{
    public int Order => 14; // before feeding purchases

    public void Tick(EngineContext ctx)
    {
        if (ctx.Clock.Now.Hour != 6) return;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Settlement s) continue;
            var econ = ctx.World.Get<SettlementEconomy>(s.EconomyId);
            var spec = ctx.World.Components.OfType<KeyValuePair<EntityId, object>>()
                                           .Where(k => k.Value is SettlementSpecialties && k.Key.Equals(ctx.World.Components.First(x => ReferenceEquals(x.Value, s)).Key))
                                           .Select(k => (SettlementSpecialties)k.Value).FirstOrDefault();

            // compute desired wages
            double desired = 0;
            var perPerson = new List<(Household hh, Person p, double wage)>();

            foreach (var hh in s.Households)
            {
                foreach (var p in hh.People)
                {
                    double baseW = econ.DailyWage.TryGetValue(p.Profession, out var w) ? w : 0.8; // default
                    double skillFactor = 0.8 + (p.Skill / 100.0) * 0.6;   // 0.8..1.4
                    double specBonus = (spec != null && spec.Weights.ContainsKey(p.Profession)) ? 1.0 + spec.SpecialtyWageBonus : 1.0;

                    var primaryPassion = PassionMaps.PrimaryFor(p.Profession);
                    double passionLevel = p.Passions.FirstOrDefault(x => x.Passion == primaryPassion)?.Level ?? 0.0;
                    double passionBonus = 1.0 + 0.2 * passionLevel;  // up to +20% at Level=1

                    double wage = baseW * skillFactor * specBonus;
                    desired += wage;
                    perPerson.Add((hh, p, wage));
                }
            }

            if (desired <= 0) continue;
            double factor = econ.WagePoolCoins >= desired ? 1.0 : (econ.WagePoolCoins / desired);

            foreach (var (hh, p, wage) in perPerson)
                hh.Wealth += wage * factor;

            econ.WagePoolCoins -= desired * factor;
            if (econ.WagePoolCoins < 0) econ.WagePoolCoins = 0;
        }
    }
}