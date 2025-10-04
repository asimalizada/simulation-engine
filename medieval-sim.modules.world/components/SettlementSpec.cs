namespace medieval_sim.modules.world.components;

public sealed class SettlementSpec
{
    public string Name = "";
    public int Pop;
    public double FoodStock;
    public double WagePool;
    public double WealthAvg;
    public double WealthVar;
    public Action<SettlementSpecialties> Specialties = _ => { };
    public Action<SettlementEconomy> EconomyTweaks = _ => { };
    public double ProductionMultiplier = 1.0;
}