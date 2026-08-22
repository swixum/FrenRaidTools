namespace FrenRaidTools.Engine;

public delegate Task SequenceBody(GameEvent start, SequenceRun run);

public delegate Task IndexedSequenceBody(GameEvent start, SequenceRun run, int invocation);

public sealed class Sequence
{
    private readonly Func<GameEvent, bool> _starts;
    private readonly IndexedSequenceBody _body;

    public Sequence(
        string name, double timeoutSeconds,
        Func<GameEvent, bool> starts, IndexedSequenceBody body)
    {
        Name = name;
        TimeoutSeconds = timeoutSeconds;
        _starts = starts;
        _body = body;
    }

    public string Name { get; }
    public double TimeoutSeconds { get; }

    public bool Starts(GameEvent e) => _starts(e);

    public Task Run(GameEvent start, SequenceRun run, int invocation) =>
        _body(start, run, invocation);

    public static Sequence Once(
        string name, double timeoutSeconds, Func<GameEvent, bool> starts, SequenceBody body) =>
        new(name, timeoutSeconds, starts,
            (start, run, invocation) => invocation == 0 ? body(start, run) : Task.CompletedTask);

    public static Sequence Repeat(
        string name, double timeoutSeconds, Func<GameEvent, bool> starts, SequenceBody body) =>
        new(name, timeoutSeconds, starts, (start, run, invocation) => body(start, run));

    public static Sequence Multi(
        string name, double timeoutSeconds, Func<GameEvent, bool> starts, params SequenceBody[] bodies) =>
        new(name, timeoutSeconds, starts,
            (start, run, invocation) => invocation < bodies.Length
                ? bodies[invocation](start, run)
                : Task.CompletedTask);

    public static Sequence Indexed(
        string name, double timeoutSeconds, Func<GameEvent, bool> starts, IndexedSequenceBody body) =>
        new(name, timeoutSeconds, starts, body);
}
