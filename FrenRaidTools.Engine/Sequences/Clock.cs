namespace FrenRaidTools.Engine;

public interface IClock
{
    double Now { get; }
}

public interface IWorld
{
    IReadOnlyList<GameEvent> ActiveCasts();
    IReadOnlyList<GameEvent> ActiveStatuses();
    Actor? You { get; }
    IReadOnlyList<Actor> Party { get; }
    Actor? Latest(Actor actor);
    IReadOnlyList<Actor> NpcsById(uint baseId);
    string? Chosen(string optionKey) => null;
    Actor? Partner() => null;
    int SeatOf(Actor actor) => -1;
}

public sealed class LiveClock : IClock
{
    private readonly System.Diagnostics.Stopwatch _watch = System.Diagnostics.Stopwatch.StartNew();

    public double Now => _watch.Elapsed.TotalSeconds;
}

public sealed class TestClock : IClock
{
    public double Now { get; private set; }

    public void Advance(double seconds) => Now += seconds;

    public void AdvanceTo(double when)
    {
        if (when > Now) Now = when;
    }
}
