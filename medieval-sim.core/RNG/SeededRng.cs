namespace medieval_sim.core.RNG;

public sealed class SeededRng : IRng
{
    private readonly Random _r; public SeededRng(int seed) => _r = new Random(seed);

    public int Next(int min, int max) => _r.Next(min, max);

    public double NextDouble() => _r.NextDouble();
}
