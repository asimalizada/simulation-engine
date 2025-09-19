namespace medieval_sim.core.events;

public sealed class EventBus : IEventBus
{
    private readonly List<object> _buf = new();

    public void Publish<T>(T ev) where T : IEvent => _buf.Add(ev);

    public IReadOnlyList<object> Drain()
    {
        var outp = _buf.ToArray();
        _buf.Clear();
        return outp;
    }
}