namespace medieval_sim.modules.world.components;

public sealed class Household
{
    public int Size;        // people
    public double Food;     // units owned by the household
    public double Wealth;   // coins
    public List<Person> People { get; } = new();
}