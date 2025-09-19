namespace medieval_sim.modules.world.components;

public sealed class TaxPolicy
{
    public double TitheRate = 0.10;          // fraction of production (approx. monetized below)
    public double MarketFeeRate = 0.05;      // ad valorem on local sales
    public double TransitTollPerUnit = 0.02; // coins per unit on arrivals
}