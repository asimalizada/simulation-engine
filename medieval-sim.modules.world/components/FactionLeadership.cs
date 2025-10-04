namespace medieval_sim.modules.world.components;

public sealed class FactionLeadership
{
    public PersonRef? Sovereign;   // King/Queen/Regent
    public PersonRef? Chancellor;  // administrator / minister
    public PersonRef? Marshal;     // military captain

    // Stipends (coins/day) paid from faction treasury
    public double SovereignStipend = 5.0;
    public double ChancellorStipend = 3.0;
    public double MarshalStipend = 3.0;
}