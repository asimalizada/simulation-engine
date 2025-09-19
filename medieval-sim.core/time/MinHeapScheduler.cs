namespace medieval_sim.core.time;

public sealed class MinHeapScheduler : IScheduler
{
    private readonly PriorityQueue<(DateTime when, Action a), DateTime> _q = new();
    
    public void Schedule(DateTime when, Action a) => _q.Enqueue((when, a), when);

    public void RunDue(DateTime now)
    {
        while (_q.TryPeek(out var top, out var t) && t <= now)
        { 
            _q.Dequeue();
            top.a();
        }
    }
}
