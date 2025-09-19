namespace medieval_sim.modules.world.components;

public sealed class FactionPolicy
{
    // Feeding per person
    public double DailyFoodPerPerson = 1.0;
    public int BufferDays = 5;
    public int[] MealHours = new[] { 9, 18 };

    // Trade policy
    public bool WillTradeExternally = true;
    public int MinRelationToTrade = -20;

    // Taxes
    public TaxPolicy Taxes = new();
}