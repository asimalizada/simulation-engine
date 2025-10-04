namespace medieval_sim.modules.world.components;

public sealed class FactionSpec
{
    public string Name = "";
    public double Treasury;
    public FactionPolicy Policy = new();
    public List<SettlementSpec> Settlements = new();
    public Dictionary<string, double> Wages = new();
    public List<(string otherName, int relation)> StartingRelations = new();
}