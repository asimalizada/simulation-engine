using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class LeadershipSelectionSystem : ISystem
{
    public int Order => 12;

    public void Tick(EngineContext ctx)
    {
        // Run at 07:00 on day 1, then yearly on 01/01 07:00
        var now = ctx.Clock.Now;
        if (now.Hour != 7) return;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not Faction f) continue;

            var lead = EnsureLeadership(ctx, f);
            // Re-select at start of the year or if any empty
            if (now.DayOfYear == 1 || lead.Sovereign is null || lead.Chancellor is null || lead.Marshal is null)
                SelectLeaders(ctx, f, lead);
        }
    }

    private static FactionLeadership EnsureLeadership(EngineContext ctx, Faction f)
    {
        // if no leadership component yet, attach one
        foreach (var kv in ctx.World.Components)
            if (kv.Value is FactionLeadership fl && ReferenceEquals(f, ctx.World.Get<Faction>(kv.Key))) return fl;

        var id = ctx.World.Create();
        var flNew = new FactionLeadership();
        ctx.World.Set(id, flNew);
        return flNew;
    }

    private static void SelectLeaders(EngineContext ctx, Faction f, FactionLeadership lead)
    {
        // collect all persons in this faction
        var people = ctx.World.Components
            .Where(kv => kv.Value is Settlement s && ReferenceEquals(ctx.World.Get<Faction>(s.FactionId), f))
            .SelectMany(kv =>
            {
                var s = (Settlement)kv.Value;
                var sid = kv.Key;
                return s.Households.SelectMany((hh, hi) => hh.People.Select((p, pi) =>
                    (pref: new PersonRef(sid, hi, pi), p, s)));
            })
            .Where(x => x.p.Age >= 18)
            .ToList();

        if (people.Count == 0) return;

        // simple scoring helpers
        double Score((PersonRef pref, Person p, Settlement s) x, Func<Profession, bool> prefer)
        {
            double prof = prefer(x.p.Profession) ? 40 : 0;
            double wealth = x.s.Households[x.pref.HouseholdIndex].Wealth;
            double wScore = Math.Min(20, wealth); // cap
            double age = (x.p.Age >= 25 && x.p.Age <= 60) ? 10 : 0;
            return prof + x.p.Skill + wScore + age;
        }

        bool IsNoble(Profession p) => p == Profession.Noble || p == Profession.Merchant;
        bool IsAdmin(Profession p) => p == Profession.Scribe || p == Profession.Merchant || p == Profession.Priest;
        bool IsSoldier(Profession p) => p == Profession.Soldier || p == Profession.Guard || p == Profession.Blacksmith;

        var sovereign = people.OrderByDescending(x => Score(x, IsNoble)).First();
        var chancellor = people.OrderByDescending(x => Score(x, IsAdmin)).First();
        var marshal = people.OrderByDescending(x => Score(x, IsSoldier)).First();

        lead.Sovereign = sovereign.pref;
        lead.Chancellor = chancellor.pref;
        lead.Marshal = marshal.pref;
    }
}