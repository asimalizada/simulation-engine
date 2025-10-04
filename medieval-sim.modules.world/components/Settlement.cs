using medieval_sim.core.ECS;

namespace medieval_sim.modules.world.components;

public sealed class Settlement
{
    public EntityId SelfId;
    public string Name = "";
    public int Pop = 0;
    public EntityId FactionId;
    public EntityId MarketId;

    public EntityId EconomyId;
    public double ProductionMultiplier = 1.0;
    public double FoodStock = 0;
    public List<Household> Households = new();

    public double MealsMissedToday = 0;
    public bool PopulationInitialized;
    public EntityId SpecialtiesId;
}