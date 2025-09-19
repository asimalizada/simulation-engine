namespace medieval_sim.modules.world.components;

public sealed class FeedingPolicy
{
    // Total per person per day
    public double DailyPerPerson = 1.0;

    // Households won't replenish if they already have >= BufferDays of food
    public int BufferDays = 5;

    // Meal hours (24h)
    public int[] MealHours = { 9, 18 };
}