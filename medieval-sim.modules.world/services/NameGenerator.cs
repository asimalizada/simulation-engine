using medieval_sim.core.RNG;
using medieval_sim.modules.world.components;

namespace medieval_sim.modules.world.services;

public static class NameGenerator
{
    // --- GIVEN NAMES ---
    private static readonly Dictionary<Culture, string[]> MaleBase = new()
    {
        [Culture.Keshari] = new[] { "Samir", "Rafiq", "Idran", "Faizal", "Hamir", "Tariq", "Aamir" },
        [Culture.Tzanel] = new[] { "Chaal", "Tzun", "Mako", "Ixen", "Paari", "Haru", "Talen" },
        [Culture.Yura] = new[] { "Temur", "Altan", "Dorji", "Khenpo", "Loden", "Tashi", "Rinpo" },
        [Culture.Ashari] = new[] { "Kwasi", "Eno", "Jabari", "Madu", "Tari", "Oben", "Kiro" },
        [Culture.Shokai] = new[] { "Renji", "Kaito", "Daichi", "Hiroto", "Kenzo", "Masaru", "Taro" },
        [Culture.Norren] = new[] { "Soren", "Inuk", "Varek", "Kaldr", "Ariq", "Taavo", "Rurik" },
        [Culture.Zhurkan] = new[] { "Batu", "Arslan", "Temjin", "Kadan", "Khotan", "Aruz", "Baatur" },
        [Culture.Kaenji] = new[] { "Ravi", "Tenzar", "Jirok", "Amun", "Kaen", "Nirin", "Osat" },
        [Culture.Aerani] = new[] { "Kaelan", "Ario", "Tane", "Loris", "Vano", "Keone", "Theron" },
        [Culture.Qazari] = new[] { "Azar", "Cyrus", "Khoran", "Darius", "Menes", "Rostam", "Navid" }
    };

    private static readonly Dictionary<Culture, string[]> FemaleBase = new()
    {
        [Culture.Keshari] = new[] { "Nura", "Samira", "Leila", "Zarah", "Amira", "Sadia", "Hanin" },
        [Culture.Tzanel] = new[] { "Nali", "Asha", "Keela", "Ixara", "Amatla", "Tzani", "Mira" },
        [Culture.Yura] = new[] { "Saran", "Dema", "Yalun", "Mirga", "Lhamo", "Tseten", "Norin" },
        [Culture.Ashari] = new[] { "Ama", "Nia", "Sefa", "Kira", "Luma", "Tayo", "Zina" },
        [Culture.Shokai] = new[] { "Aiko", "Hana", "Mei", "Sora", "Yuna", "Reika", "Kaori" },
        [Culture.Norren] = new[] { "Runa", "Talya", "Nivi", "Eira", "Inna", "Sira", "Kalla" },
        [Culture.Zhurkan] = new[] { "Sura", "Aluna", "Kira", "Enkha", "Tulan", "Mira", "Sayan" },
        [Culture.Kaenji] = new[] { "Mira", "Sita", "Rani", "Keira", "Liani", "Anya", "Veda" },
        [Culture.Aerani] = new[] { "Moana", "Kaila", "Nera", "Ione", "Vaela", "Aria", "Naiya" },
        [Culture.Qazari] = new[] { "Nisara", "Zaleh", "Tarani", "Imena", "Oris", "Yasna", "Shirin" }
    };

    // --- FAMILY / HOUSE NAMES ---
    private static readonly Dictionary<Culture, string[]> FamilyBase = new()
    {
        [Culture.Keshari] = new[] { "al-Keshar", "ibn-Salim", "Daroun", "Bahari", "el-Rashid" },
        [Culture.Tzanel] = new[] { "Nahu", "Tzal", "Omek", "Paari", "of the River" },
        [Culture.Yura] = new[] { "Tsering", "Dawa", "Norin", "Dorje", "Orong" },
        [Culture.Ashari] = new[] { "Okoro", "Nyame", "Baako", "Aduma", "Kendi" },
        [Culture.Shokai] = new[] { "Ishida", "Tsubaki", "Hoshino", "Takara", "Mori" },
        [Culture.Norren] = new[] { "of the Frost", "Sarnak", "Iqal", "Korrik", "Odrin" },
        [Culture.Zhurkan] = new[] { "Ortak", "Zhurk", "Baatur", "Temul", "Khagan" },
        [Culture.Kaenji] = new[] { "Varun", "Kaenji", "Dhaal", "Nirin", "Osat" },
        [Culture.Aerani] = new[] { "Arai", "Keone", "Pelari", "Orana", "Thalos" },
        [Culture.Qazari] = new[] { "Qazari", "Menet", "Oron", "Ashael", "Paren" }
    };

    // Syllable banks per culture for combinator expansion
    private static readonly Dictionary<Culture, (string[] A, string[] B, string[] C)> Syll = new()
    {
        [Culture.Keshari] = (new[] { "sa", "ra", "fi", "ha", "ta", "na", "le", "am", "da" },
                             new[] { "mir", "zim", "dar", "fal", "rif", "han", "sir", "zan" },
                             new[] { "a", "im", "un", "al", "ir", "ar", "el", "in" }),
        [Culture.Tzanel] = (new[] { "cha", "tz", "ma", "ha", "ix", "pa", "na", "ke" },
                             new[] { "al", "un", "ko", "ri", "ta", "za", "lo" },
                             new[] { "a", "i", "an", "li", "en", "o" }),
        [Culture.Yura] = (new[] { "do", "al", "te", "rin", "lo", "kh", "tsa" },
                             new[] { "rji", "shi", "tan", "lten", "dor", "rpo" },
                             new[] { "a", "o", "u", "en", "in" }),
        [Culture.Ashari] = (new[] { "ko", "ki", "ja", "ba", "ta", "na", "lu", "se" },
                             new[] { "ro", "ri", "ba", "ko", "do", "ma", "fa" },
                             new[] { "a", "e", "i", "o" }),
        [Culture.Shokai] = (new[] { "ka", "ta", "ra", "sa", "mi", "hi", "yo" },
                             new[] { "to", "ri", "na", "ko", "shi", "da" },
                             new[] { "o", "a", "i", "u" }),
        [Culture.Norren] = (new[] { "ru", "so", "kal", "ni", "ei", "va", "ta" },
                             new[] { "rik", "ldr", "vik", "nn", "gr", "rn" },
                             new[] { "a", "i", "o" }),
        [Culture.Zhurkan] = (new[] { "ba", "ar", "tem", "ka", "hot", "en", "sa" },
                             new[] { "tur", "slan", "jin", "dan", "ruk", "tan" },
                             new[] { "a", "o", "i" }),
        [Culture.Kaenji] = (new[] { "ka", "ra", "te", "ji", "ni", "os", "va" },
                             new[] { "en", "ri", "rok", "nar", "mun", "zin" },
                             new[] { "a", "i", "o" }),
        [Culture.Aerani] = (new[] { "ae", "ke", "lo", "va", "ta", "mo", "ne" },
                             new[] { "ra", "na", "ri", "la", "no", "ri" },
                             new[] { "i", "a", "e", "o" }),
        [Culture.Qazari] = (new[] { "za", "kh", "da", "na", "ro", "me", "pa" },
                             new[] { "har", "zar", "ran", "var", "ris", "nes" },
                             new[] { "a", "eh", "an", "in" })
    };

    public static string NextSettlement(IRng rng, Culture culture)
    {
        // 60% combinator, 40% curated
        return rng.NextDouble() < 0.6
            ? Titlecase(Combine(rng, Syll[culture], suffix: SetSuffix(culture, rng)))
            : Titlecase(Pick(rng, SettlementBase[culture]));
    }

    // base settlement themes to mix with combinator output
    private static readonly Dictionary<Culture, string[]> SettlementBase = new()
    {
        [Culture.Keshari] = new[] { "Qasira", "Daroun", "Ibnar", "Baharet", "Sadir", "Rashim", "Keshar Oasis" },
        [Culture.Tzanel] = new[] { "Tzuna", "Amatli", "Paara", "Ixen Vale", "Makora", "Nahu Flats", "Reedholm" },
        [Culture.Yura] = new[] { "Dorjun", "Temur Pass", "Altara", "Khenpo Heights", "Rinholm", "Tser Ridge", "Drol Peak" },
        [Culture.Ashari] = new[] { "Nyame Canopy", "Okoro Root", "Kendari", "Baako Glade", "Adum Hollow", "Zina Bower", "Luma Vines" },
        [Culture.Shokai] = new[] { "Hoshima", "Takara Bay", "Ishidome", "Kairin", "Morioka", "Sora Atoll", "Reika Port" },
        [Culture.Norren] = new[] { "Kaldr Fjord", "Nivi Tundra", "Sarnak Camp", "Eira Inlet", "Taavoberg", "Frostway" },
        [Culture.Zhurkan] = new[] { "Ortak Steppe", "Khagan Post", "Temul Wells", "Baatur Fold", "Zhurk Banner", "Aruz Ridge" },
        [Culture.Kaenji] = new[] { "Kaen Forge", "Varun Spires", "Dhaal Quay", "Nirin Works", "Osat Halls", "Rainsteam" },
        [Culture.Aerani] = new[] { "Arai Shoals", "Keone Reef", "Pelari Sound", "Orana Keys", "Thalos Harbor", "Vaela Isle" },
        [Culture.Qazari] = new[] { "Azar Gate", "Menet Forge", "Oron Spires", "Ashael Plaza", "Paren Rise", "Solarum" }
    };

    public static (string given, Gender gender) NextGiven(IRng rng, Culture culture)
    {
        var male = rng.NextDouble() < 0.5;
        var curated = male ? MaleBase[culture] : FemaleBase[culture];
        string name = rng.NextDouble() < 0.6
            ? Titlecase(Combine(rng, Syll[culture]))
            : Pick(rng, curated);
        return (name, male ? Gender.Male : Gender.Female);
    }

    public static string NextFamily(IRng rng, Culture culture)
    {
        // 50% curated house, 50% constructed family
        return rng.NextDouble() < 0.5
            ? Pick(rng, FamilyBase[culture])
            : Titlecase(Combine(rng, Syll[culture], familyStyle: true));
    }

    private static string Combine(IRng rng, (string[] A, string[] B, string[] C) s, bool familyStyle = false, string? suffix = null)
    {
        var a = s.A[rng.Next(0, s.A.Length)];
        var b = s.B[rng.Next(0, s.B.Length)];
        var c = s.C[rng.Next(0, s.C.Length)];
        string core = a + b + (rng.NextDouble() < 0.5 ? "" : c);
        if (familyStyle)
        {
            // allow particles
            if (rng.NextDouble() < 0.2) core = "al-" + core;
            if (rng.NextDouble() < 0.1) core = core + (rng.NextDouble() < 0.5 ? "i" : "en");
        }
        return suffix is null ? core : $"{core} {suffix}";
    }

    private static string SetSuffix(Culture c, IRng rng)
    {
        return c switch
        {
            Culture.Keshari => rng.NextDouble() < 0.5 ? "Oasis" : "Bazaar",
            Culture.Tzanel => rng.NextDouble() < 0.5 ? "Reeds" : "Weir",
            Culture.Yura => rng.NextDouble() < 0.5 ? "Pass" : "Heights",
            Culture.Ashari => rng.NextDouble() < 0.5 ? "Canopy" : "Glade",
            Culture.Shokai => rng.NextDouble() < 0.5 ? "Bay" : "Port",
            Culture.Norren => rng.NextDouble() < 0.5 ? "Fjord" : "Tundra",
            Culture.Zhurkan => rng.NextDouble() < 0.5 ? "Steppe" : "Banner",
            Culture.Kaenji => rng.NextDouble() < 0.5 ? "Forge" : "Works",
            Culture.Aerani => rng.NextDouble() < 0.5 ? "Reef" : "Harbor",
            Culture.Qazari => rng.NextDouble() < 0.5 ? "Gate" : "Spires",
            _ => ""
        };
    }

    private static string Pick(IRng rng, string[] arr) => arr[rng.Next(0, arr.Length)];
    private static string Titlecase(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
}