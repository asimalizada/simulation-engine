using medieval_sim.core.events;

namespace medieval_sim.modules.economy.components;

public sealed class Market : IEvent
{
    public string Scope = "";
    public double FoodPrice = 1.0;
    public double LastSupply;
    public double LastDemand;
}
