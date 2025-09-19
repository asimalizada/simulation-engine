namespace medieval_sim.core.RNG;

public interface IRng
{
    int Next(int min, int max);
    double NextDouble();
}
