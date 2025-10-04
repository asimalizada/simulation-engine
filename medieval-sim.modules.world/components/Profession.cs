namespace medieval_sim.modules.world.components;

public enum Profession
{
    Farmer, Miller, Baker, Brewer,
    Fisher, Hunter, Woodcutter, Miner,
    Blacksmith, Carpenter, Mason, Weaver, Tanner,
    Merchant, Scribe, Healer, Priest, Monk,
    Guard, Soldier, Noble, Caravaneer
}

// Weighted specialties for a settlement
public sealed class SettlementSpecialties
{
    // e.g., { Farmer: 2.0, Fisher: 1.5 } → boosts chance & wage
    public readonly Dictionary<Profession, double> Weights = new();
    public double SpecialtyWageBonus = 0.25; // +25% wage if profession is a specialty
}