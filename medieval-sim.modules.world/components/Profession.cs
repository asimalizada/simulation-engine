namespace medieval_sim.modules.world.components;

public enum Profession
{
    // Agrarian & nature
    Farmer, Shepherd, Fisher, Hunter, Woodcutter,

    // Materials & industry
    Carpenter, Mason, Miner, Tanner, Weaver, Dyer, Potter, Glassblower, Leatherworker, Tailor,

    // Metal & advanced craft
    Blacksmith, Jeweler, Shipwright, Alchemist, Brewer,

    // Trade & services
    Merchant, Caravaneer, Sailor, Scribe, Healer, Priest, Monk, Bard, Cook,

    // Security & rule
    Guard, Soldier, Noble
}

// Weighted specialties for a settlement
public sealed class SettlementSpecialties
{
    // e.g., { Farmer: 2.0, Fisher: 1.5 } → boosts chance & wage
    public readonly Dictionary<Profession, double> Weights = new();
    public double SpecialtyWageBonus = 0.25; // +25% wage if profession is a specialty

    public void Boost(Profession p, double factor)
    {
        var d = Weights;
        if (d.TryGetValue(p, out var current))
            d[p] = current * factor;
        else
            d[p] = factor;
    }
}