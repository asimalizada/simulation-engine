namespace medieval_sim.core.events;

public interface IEventBus
{
    void Publish<T>(T ev) where T : IEvent;
    IReadOnlyList<object> Drain(); // consumed each tick
}
