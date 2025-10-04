namespace medieval_sim.modules.world.components;

public enum Passion
{
    Farming, Crafting, Trade, Warfare,
    Scholarship, Faith, Nature, Seafaring,
    Artistry, Healing, Leadership, Brewing
}

public static class PassionMaps
{
    public static Passion PrimaryFor(Profession prof) => prof switch
    {
        // Agrarian / Nature
        Profession.Farmer or Profession.Shepherd => Passion.Farming,
        Profession.Fisher or Profession.Hunter or Profession.Woodcutter => Passion.Nature,

        // Materials / Industry (practical vs aesthetic)
        Profession.Miner or Profession.Mason or Profession.Carpenter
            or Profession.Tanner or Profession.Leatherworker
            or Profession.Cook => Passion.Crafting,
        Profession.Weaver or Profession.Tailor or Profession.Dyer
            or Profession.Potter or Profession.Glassblower
            or Profession.Jeweler or Profession.Bard => Passion.Artistry,

        // Metal & advanced craft
        Profession.Blacksmith or Profession.Shipwright or Profession.Brewer => Passion.Crafting,
        Profession.Alchemist => Passion.Scholarship,

        // Trade & movement
        Profession.Merchant or Profession.Caravaneer => Passion.Trade,
        Profession.Sailor => Passion.Seafaring,

        // Knowledge / care / faith
        Profession.Scribe => Passion.Scholarship,
        Profession.Healer => Passion.Healing,
        Profession.Priest or Profession.Monk => Passion.Faith,

        // Security / rule
        Profession.Guard or Profession.Soldier => Passion.Warfare,
        Profession.Noble => Passion.Leadership,

        _ => Passion.Crafting
    };

    public static IReadOnlyList<Passion> SecondaryFor(Profession prof) => prof switch
    {
        // Agrarian / Nature
        Profession.Farmer => new[] { Passion.Nature, Passion.Trade },
        Profession.Shepherd => new[] { Passion.Nature, Passion.Farming },
        Profession.Fisher => new[] { Passion.Seafaring, Passion.Nature },
        Profession.Hunter => new[] { Passion.Nature, Passion.Warfare },
        Profession.Woodcutter => new[] { Passion.Crafting, Passion.Nature },

        // Materials / Industry
        Profession.Miner => new[] { Passion.Crafting, Passion.Warfare },
        Profession.Mason => new[] { Passion.Crafting, Passion.Artistry },
        Profession.Carpenter => new[] { Passion.Crafting, Passion.Artistry },
        Profession.Tanner => new[] { Passion.Crafting, Passion.Trade },
        Profession.Leatherworker => new[] { Passion.Artistry, Passion.Trade },
        Profession.Cook => new[] { Passion.Brewing, Passion.Trade },

        // Aesthetic crafts
        Profession.Weaver => new[] { Passion.Artistry, Passion.Trade },
        Profession.Tailor => new[] { Passion.Artistry, Passion.Trade },
        Profession.Dyer => new[] { Passion.Artistry, Passion.Crafting },
        Profession.Potter => new[] { Passion.Artistry, Passion.Crafting },
        Profession.Glassblower => new[] { Passion.Artistry, Passion.Crafting },
        Profession.Jeweler => new[] { Passion.Artistry, Passion.Trade },
        Profession.Bard => new[] { Passion.Faith, Passion.Scholarship },

        // Metal / advanced craft
        Profession.Blacksmith => new[] { Passion.Warfare, Passion.Trade },
        Profession.Shipwright => new[] { Passion.Seafaring, Passion.Crafting },
        Profession.Brewer => new[] { Passion.Crafting, Passion.Trade },
        Profession.Alchemist => new[] { Passion.Healing, Passion.Crafting },

        // Trade & movement
        Profession.Merchant => new[] { Passion.Leadership, Passion.Scholarship },
        Profession.Caravaneer => new[] { Passion.Nature, Passion.Trade },
        Profession.Sailor => new[] { Passion.Trade, Passion.Leadership },

        // Knowledge / care / faith
        Profession.Scribe => new[] { Passion.Trade, Passion.Faith },
        Profession.Healer => new[] { Passion.Scholarship, Passion.Faith },
        Profession.Priest => new[] { Passion.Leadership, Passion.Scholarship },
        Profession.Monk => new[] { Passion.Scholarship, Passion.Healing },

        // Security / rule
        Profession.Guard => new[] { Passion.Leadership, Passion.Crafting },
        Profession.Soldier => new[] { Passion.Leadership, Passion.Crafting },
        Profession.Noble => new[] { Passion.Warfare, Passion.Scholarship },

        _ => Array.Empty<Passion>()
    };
}


public sealed class PassionIntensity
{
    public Passion Passion;
    public double Level; // 0..1
}