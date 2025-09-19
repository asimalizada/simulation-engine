namespace medieval_sim.modules.world.components;

public sealed class SettlementMarket
{
    public string Name = "";
    public double PriceFood = 1.0;   // coins per unit
    public double FeeRateOverride = -1;  // <0 => use faction.tax.MarketFeeRate
    public bool IsFairDay = false;

    // Simple daily flow bookkeeping
    public double SupplyToday = 0;
    public double DemandToday = 0;
    public double LastSupply = 0;
    public double LastDemand = 0;
}