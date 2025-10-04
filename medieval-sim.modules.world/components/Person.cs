using medieval_sim.core.ECS;

namespace medieval_sim.modules.world.components;

public readonly record struct PersonRef(EntityId SettlementId, int HouseholdIndex, int PersonIndex);

public sealed class Relation
{
    public PersonRef Target;
    public int Score; // -100..100
}

public sealed class Person
{
    public string Name = "";
    public int Age;
    public Profession Profession;
    public double Skill;
    public List<Relation> Relations = new();
    public List<PassionIntensity> Passions { get; } = new();  // 1–3 items
    public int? FamilyIndex;                                   // index into SettlementFamilies.Families for the settlement
}