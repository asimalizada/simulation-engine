
namespace medieval_sim.core.time;

public class FixedStepClock : IClock
{
    public DateTime Now {  get; private set; }
    private readonly TimeSpan _step;

    public FixedStepClock(DateTime start, TimeSpan step)
    {
        Now = start;
        _step = step;
    }

    public void Advance() => Now = Now.Add(_step);
}
