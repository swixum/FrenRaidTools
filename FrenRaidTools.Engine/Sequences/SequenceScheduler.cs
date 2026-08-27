using System.Collections.Concurrent;

namespace FrenRaidTools.Engine;

public sealed class SequenceScheduler : SynchronizationContext
{
    private readonly ConcurrentQueue<(SendOrPostCallback Work, object? State)> _queue = new();

    public const int MaxDrainPerPump = 10_000;

    public int Pending => _queue.Count;

    public override void Post(SendOrPostCallback work, object? state) => _queue.Enqueue((work, state));

    public override void Send(SendOrPostCallback work, object? state) => work(state);

    public override SynchronizationContext CreateCopy() => this;

    public int Pump() => Under(Drain);

    private int Drain()
    {
        var ran = 0;
        while (ran < MaxDrainPerPump && _queue.TryDequeue(out var item))
        {
            item.Work(item.State);
            ran++;
        }
        return ran;
    }

    public T Under<T>(Func<T> work)
    {
        var previous = Current;
        if (ReferenceEquals(previous, this)) return work();

        SetSynchronizationContext(this);
        try
        {
            return work();
        }
        finally
        {
            SetSynchronizationContext(previous);
        }
    }
}
