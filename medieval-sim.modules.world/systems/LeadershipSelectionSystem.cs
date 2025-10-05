using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class LeadershipSelectionSystem : ISystem
{
    public int Order => 12;

    public void Tick(EngineContext ctx)
    {
        var now = ctx.Clock.Now;
        if (now.Hour != 7) return;

        var factions = ctx.World.Components
            .Where(kv => kv.Value is Faction)
            .Select(kv => (id: kv.Key, f: (Faction)kv.Value))
            .ToList();

        foreach (var (fid, f) in factions)
        {
            var lead = GetOrCreateLeadership(ctx, fid);

            // Re-select yearly or if any slot is empty
            if (now.DayOfYear == 1 || lead.Sovereign is null || lead.Chancellor is null || lead.Marshal is null)
            {
                SelectLeaders(ctx, fid, f, lead);
            }
        }
    }

    private static FactionLeadership GetOrCreateLeadership(EngineContext ctx, EntityId factionId)
    {
        // Try to find an existing leadership tied to this faction
        foreach (var kv in ctx.World.Components)
            if (kv.Value is FactionLeadership fl && fl.OwnerFactionId.Equals(factionId))
                return fl;

        // Create one if missing (safe because we are NOT iterating components here)
        var id = ctx.World.Create();
        var flNew = new FactionLeadership { OwnerFactionId = factionId };
        ctx.World.Set(id, flNew);
        return flNew;
    }

    private static void SelectLeaders(EngineContext ctx, EntityId factionId, Faction f, FactionLeadership lead)
    {
        var people = ctx.World.Components
            .Where(kv => kv.Value is Settlement s && s.FactionId.Equals(factionId))
            .SelectMany(kv =>
            {
                var sid = kv.Key;
                var s = (Settlement)kv.Value;
                return s.Households.SelectMany((hh, hi) =>
                    hh.People.Select((p, pi) => (pref: new PersonRef(sid, hi, pi), p, s)));
            })
            .Where(x => x.p.Age >= 18)
            .ToList();

        if (people.Count == 0) return;

        // Profession buckets 
        static bool IsNoble(Profession p) => p == Profession.Noble || p == Profession.Merchant;
        static bool IsAdmin(Profession p) => p == Profession.Scribe || p == Profession.Merchant || p == Profession.Priest;
        static bool IsSoldier(Profession p) => p == Profession.Soldier || p == Profession.Guard || p == Profession.Blacksmith;

        // Central scoring; slight bump for faction capital residents
        static double Score((PersonRef pref, Person p, Settlement s) x, Func<Profession, bool> prefer, bool capitalBonus)
        {
            double prof = prefer(x.p.Profession) ? 40 : 0;
            double wealth = x.s.Households[x.pref.HouseholdIndex].Wealth;
            double wScore = Math.Min(20, wealth);
            double age = (x.p.Age >= 25 && x.p.Age <= 60) ? 10 : 0;
            double capital = (capitalBonus && x.s.IsCapital) ? 5 : 0;
            return prof + x.p.Skill + wScore + age + capital;
        }

        var sovereign = people.OrderByDescending(x => Score(x, IsNoble, capitalBonus: true)).First();
        var chancellor = people.Where(x => !x.pref.Equals(sovereign.pref))
                               .OrderByDescending(x => Score(x, IsAdmin, capitalBonus: false))
                               .FirstOrDefault();
        var marshalPool = people.Where(x => !x.pref.Equals(sovereign.pref) &&(chancellor.p is null || !x.pref.Equals(chancellor.pref)));
        var marshal = marshalPool.OrderByDescending(x => Score(x, IsSoldier, capitalBonus: false)).FirstOrDefault();

        lead.Sovereign = sovereign.pref;
        if (chancellor.p is not null) lead.Chancellor = chancellor.pref;
        if (marshal.p is not null) lead.Marshal = marshal.pref;
    }
}