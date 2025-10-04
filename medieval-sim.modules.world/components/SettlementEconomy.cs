namespace medieval_sim.modules.world.components;

public sealed class SettlementEconomy
{
    public string Name = "";
    public double WagePoolCoins = 0.0;

    public readonly Dictionary<Profession, double> DailyWage = new();
}