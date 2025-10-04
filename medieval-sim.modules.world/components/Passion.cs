namespace medieval_sim.modules.world.components;

public enum Passion
{
    Farming, Crafting, Trade, Warfare,
    Scholarship, Faith, Nature, Seafaring,
    Artistry, Healing, Leadership, Brewing
}

public static class PassionMaps
{
    // Main passion suggested by a profession
    public static Passion PrimaryFor(Profession prof) => prof switch
    {
        Profession.Farmer => Passion.Farming,
        Profession.Miller or Profession.Baker or Profession.Blacksmith or Profession.Carpenter
            or Profession.Mason or Profession.Weaver or Profession.Tanner => Passion.Crafting,
        Profession.Merchant or Profession.Scribe => Passion.Trade,
        Profession.Guard or Profession.Soldier => Passion.Warfare,
        Profession.Priest or Profession.Monk => Passion.Faith,
        Profession.Healer => Passion.Healing,
        Profession.Fisher or Profession.Hunter or Profession.Woodcutter => Passion.Nature,
        Profession.Caravaneer => Passion.Seafaring,
        Profession.Brewer => Passion.Brewing,
        Profession.Noble => Passion.Leadership,
        _ => Passion.Crafting
    };

    // Optional secondaries by profession (used for variety)
    public static IReadOnlyList<Passion> SecondaryFor(Profession prof) => prof switch
    {
        Profession.Merchant => new[] { Passion.Leadership, Passion.Scholarship },
        Profession.Scribe => new[] { Passion.Scholarship, Passion.Faith },
        Profession.Noble => new[] { Passion.Leadership, Passion.Warfare },
        Profession.Fisher => new[] { Passion.Seafaring, Passion.Nature },
        Profession.Hunter => new[] { Passion.Nature, Passion.Warfare },
        Profession.Brewer => new[] { Passion.Crafting, Passion.Trade },
        _ => Array.Empty<Passion>()
    };
}

public sealed class PassionIntensity
{
    public Passion Passion;
    public double Level; // 0..1
}