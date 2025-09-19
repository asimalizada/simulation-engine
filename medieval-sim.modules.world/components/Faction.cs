using medieval_sim.core.ECS;

namespace medieval_sim.modules.world.components;

public sealed class Faction
{
    public string Name = "";
    public double Treasury = 0;

    // Per-faction knobs
    public FactionPolicy Policy = new();

    // Diplomatic stance toward other factions: -100..100 (0 = neutral)
    public Dictionary<EntityId, int> Relations = new();
}