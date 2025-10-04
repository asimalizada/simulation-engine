using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.systems;

public sealed class LeadershipStipendSystem : ISystem
{
    public int Order => 13; // after selection

    public void Tick(EngineContext ctx)
    {
        if (ctx.Clock.Now.Hour != 6) return;

        foreach (var kv in ctx.World.Components)
        {
            if (kv.Value is not FactionLeadership L) continue;

            // Find the owning faction (nearest preceding Faction in store)
            var faction = ctx.World.Components.FirstOrDefault(x => x.Value is Faction).Value as Faction;
            if (faction is null) continue;

            Pay(L.Sovereign, L.SovereignStipend);
            Pay(L.Chancellor, L.ChancellorStipend);
            Pay(L.Marshal, L.MarshalStipend);

            void Pay(PersonRef? pr, double amt)
            {
                if (pr is null || amt <= 0) return;
                if (faction.Treasury < amt) { amt = faction.Treasury; faction.Treasury = 0; }
                else faction.Treasury -= amt;

                var s = ctx.World.Get<Settlement>(pr.Value.SettlementId);
                var hh = s.Households[pr.Value.HouseholdIndex];
                hh.Wealth += amt;
            }
        }
    }
}