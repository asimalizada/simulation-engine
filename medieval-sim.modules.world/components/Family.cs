using medieval_sim.core.ECS;

namespace medieval_sim.modules.world.components;

public sealed class Family
{
    public string Surname = "";
    public List<PersonRef> Members = new();
    public List<int> HouseholdIndices = new();
}

public sealed class SettlementFamilies
{
    public EntityId SettlementId;
    public List<Family> Families = new();
}