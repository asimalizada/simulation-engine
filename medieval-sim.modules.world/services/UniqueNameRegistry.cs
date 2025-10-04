namespace medieval_sim.modules.world.services;

public sealed class UniqueNameRegistry
{
    private readonly HashSet<string> _people = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _settlements = new(StringComparer.OrdinalIgnoreCase);

    public string ReservePerson(string full)
    {
        var name = full;
        int n = 2;
        while (!_people.Add(name))
            name = $"{full} {n++}";
        return name;
    }

    public string ReserveSettlement(string name)
    {
        var s = name;
        int n = 2;
        while (!_settlements.Add(s))
            s = $"{name} {n++}";
        return s;
    }
}