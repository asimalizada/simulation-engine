namespace medieval_sim.core.time;

public interface IScheduler
{
    // For “market day every 7 days”, “taxes monthly”, etc.
    void Schedule(DateTime when, Action action);
    void RunDue(DateTime now); // executes in timestamp order
}
