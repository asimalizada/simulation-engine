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

        // Snapshot leadership components first
        var leaderships = ctx.World.Components
            .Where(kv => kv.Value is FactionLeadership)
            .Select(kv => (FactionLeadership)kv.Value)
            .ToList();

        foreach (var L in leaderships)
        {
            // Pay from the owning faction's treasury (requires OwnerFactionId on FactionLeadership)
            var faction = ctx.World.Get<Faction>(L.OwnerFactionId);

            Pay(ref faction.Treasury, L.Sovereign, L.SovereignStipend, ctx);
            Pay(ref faction.Treasury, L.Chancellor, L.ChancellorStipend, ctx);
            Pay(ref faction.Treasury, L.Marshal, L.MarshalStipend, ctx);
        }
    }

    private static void Pay(ref double treasury, PersonRef? pr, double amount, EngineContext ctx)
    {
        if (pr is null || amount <= 0) return;

        // Cap by available treasury
        var pay = amount <= treasury ? amount : treasury;
        if (pay <= 0) return;
        treasury -= pay;

        var s = ctx.World.Get<Settlement>(pr.Value.SettlementId);
        var hh = s.Households[pr.Value.HouseholdIndex];
        hh.Wealth += pay;
    }
}