using medieval_sim.core.ECS;

namespace medieval_sim.modules.world.components;

public sealed class Settlement
{
    public string Name = "";
    public int Pop = 0;

    // owner
    public EntityId FactionId;

    // Storage & market
    public double FoodStock = 0;        // public granary stock
    public EntityId MarketId;           // SettlementMarket component id

    // Households (inequality)
    public List<Household> Households = new();

    // Diagnostics for the current day (reset at 00:00)
    // Number of *people-equivalent* meals missed (e.g., 3.0 means 3 people missed one meal)
    public double MealsMissedToday = 0;
    public double ProductionMultiplier = 1.0;
}