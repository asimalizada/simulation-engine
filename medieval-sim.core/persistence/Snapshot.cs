namespace medieval_sim.core.persistence;

public sealed class Snapshot
{
    public string Version { get; init; } = "0.1.0";
    public DateTime Time { get; init; }
    public List<(int id, string type, string json)> Components { get; init; } = new();
}