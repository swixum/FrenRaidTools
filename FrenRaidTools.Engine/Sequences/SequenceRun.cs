namespace FrenRaidTools.Engine;

public delegate void CallSink(Callout callout, GameEvent? on, IReadOnlyDictionary<string, object?> args);

public sealed class SequenceStopped : Exception;

public sealed class SequenceRun
{
    public const int MaxPending = 512;
    public const double QuickSuccessionSeconds = 1.0;

    private readonly Queue<GameEvent> _pending = new();
    private readonly Dictionary<string, object?> _params = [];
    private readonly CallSink _sink;
    private readonly IClock _clock;

    private TaskCompletionSource<GameEvent?>? _waiter;
    private double _deadline = double.PositiveInfinity;
    private bool _waiterTakesEvents;

    public SequenceRun(CallSink sink, IClock clock)
    {
        _sink = sink;
        _clock = clock;
    }

    public double StartedAt { get; internal set; }

    public double SinceStart => _clock.Now - StartedAt;

    public double Now => _clock.Now;

    public IReadOnlyDictionary<string, object?> Params => _params;

    public bool Stopped { get; private set; }

    public int Dropped { get; private set; }

    public bool Waiting => _waiter is not null;

    public double NextDeadline => _deadline;

    public void Feed(GameEvent e)
    {
        if (Stopped) return;

        if (_waiter is { } waiting && _waiterTakesEvents)
        {
            ClearWaiter();
            waiting.TrySetResult(e);
            return;
        }

        if (_pending.Count >= MaxPending)
        {
            _pending.Dequeue();
            Dropped++;
        }

        _pending.Enqueue(e);
    }

    internal bool WakeIfDue(double now)
    {
        if (Stopped || _waiter is null || now < _deadline) return false;

        var waiting = _waiter;
        ClearWaiter();
        waiting.TrySetResult(null);
        return true;
    }

    public void Stop()
    {
        if (Stopped) return;
        Stopped = true;
        _pending.Clear();

        var waiting = _waiter;
        ClearWaiter();
        waiting?.TrySetException(new SequenceStopped());
    }

    private void ClearWaiter()
    {
        _waiter = null;
        _deadline = double.PositiveInfinity;
        _waiterTakesEvents = false;
    }

    public Action<GameEvent>? Emit { get; set; }

    public void Raise(GameEvent e) => Emit?.Invoke(e);

    public double Since(GameEvent e) => _clock.Now - e.At;

    public double Remaining(GameEvent? status) =>
        status is null ? 0 : status.At + status.Duration - _clock.Now;

    public GameEvent? DurationBelow(double seconds, params GameEvent?[] statuses)
    {
        foreach (var status in statuses)
        {
            if (status is null) continue;
            var left = Remaining(status);
            if (left > 0 && left < seconds) return status;
        }
        return null;
    }

    public void SetParam(string name, object? value) => _params[name] = value;

    public void ClearParams() => _params.Clear();

    public void DeleteParam(string name) => _params.Remove(name);

    public Action<string>? ExpireCall { get; set; }

    public CallTicket Call(Callout callout)
    {
        _sink(callout, null, Snapshot());
        return new CallTicket(callout.Key, ExpireCall);
    }

    public CallTicket Call(Callout callout, GameEvent on)
    {
        _params["event"] = on;
        _sink(callout, on, Snapshot());
        return new CallTicket(callout.Key, ExpireCall);
    }

    private Dictionary<string, object?> Snapshot() => new(_params);

    public async Task<GameEvent> WaitEvent(Func<GameEvent, bool> match)
    {
        while (true)
        {
            var e = await NextEvent();
            if (match(e)) return e;
        }
    }

    public Task<GameEvent> WaitEvent(EventKind kind, params uint[] ids) =>
        WaitEvent(e => e.Is(kind, ids));

    public Task<GameEvent> WaitEvent(EventKind kind, Func<GameEvent, bool> match) =>
        WaitEvent(e => e.Kind == kind && match(e));

    public async Task<List<GameEvent>> WaitEvents(int count, Func<GameEvent, bool> match)
    {
        var found = new List<GameEvent>(count);
        while (found.Count < count)
        {
            var e = await NextEvent();
            if (match(e)) found.Add(e);
        }
        return found;
    }

    public Task<List<GameEvent>> WaitEvents(int count, EventKind kind, Func<GameEvent, bool> match) =>
        WaitEvents(count, e => e.Kind == kind && match(e));

    public async Task<List<GameEvent>> WaitEventsQuickSuccession(
        int max, Func<GameEvent, bool> match, double window = QuickSuccessionSeconds)
    {
        var found = new List<GameEvent> { await WaitEvent(match) };
        var deadline = _clock.Now + window;

        while (found.Count < max)
        {
            if (_clock.Now >= deadline) break;

            var e = await NextEventOrDeadline(deadline);
            if (e is null) break;
            if (match(e)) found.Add(e);
        }

        return found;
    }

    public async Task<List<GameEvent>> WaitEventsUntil(
        Func<GameEvent, bool> match, Func<GameEvent, bool> until, int max = 64)
    {
        var found = new List<GameEvent>();
        while (found.Count < max)
        {
            var e = await NextEvent();
            if (match(e)) found.Add(e);
            if (until(e)) break;
        }
        return found;
    }

    public async Task WaitSeconds(double seconds)
    {
        if (seconds <= 0) return;
        Guard();

        var waiter = Arm(_clock.Now + seconds, takesEvents: false);
        await waiter.Task;
        Guard();
    }

    public Task WaitMs(double milliseconds) => WaitSeconds(milliseconds / 1000.0);

    public Task<GameEvent> WaitStatusLose(uint statusId, Func<Actor, bool> on) =>
        WaitEvent(e => e.Kind == EventKind.StatusLose && e.Id == statusId
                       && e.Target is not null && on(e.Target));

    public Task<GameEvent> WaitStatusRemoved(GameEvent gained) =>
        WaitEvent(e => e.Kind == EventKind.StatusLose && e.Id == gained.Id
                       && e.Target?.ObjectId == gained.Target?.ObjectId);

    public Task WaitStatusRemovedIfAny(GameEvent? gained) =>
        gained is null ? Task.CompletedTask : WaitStatusRemoved(gained);

    public async Task<GameEvent?> WaitEventUntil(Func<GameEvent, bool> match, double deadline)
    {
        while (_clock.Now < deadline)
        {
            var e = await NextEventOrDeadline(deadline);
            if (e is null) return null;
            if (match(e)) return e;
        }
        return null;
    }

    public Task<GameEvent?> WaitStatusRemovedUntil(GameEvent gained, double deadline) =>
        WaitEventUntil(
            e => e.Kind == EventKind.StatusLose && e.Id == gained.Id
                 && e.Target?.ObjectId == gained.Target?.ObjectId,
            deadline);

    public async Task WaitStatusRemovedOrExpired(GameEvent gained, double graceSeconds)
    {
        var deadline = Math.Max(gained.At + gained.Duration, _clock.Now) + graceSeconds;

        while (_clock.Now < deadline)
        {
            var e = await NextEventOrDeadline(deadline);
            if (e is null) return;
            if (e.Kind == EventKind.StatusLose && e.Id == gained.Id
                && e.Target?.ObjectId == gained.Target?.ObjectId) return;
        }
    }

    public async Task WaitCastFinished(GameEvent cast)
    {
        var deadline = cast.At + cast.Duration;
        while (true)
        {
            if (_clock.Now >= deadline) return;

            var e = await NextEventOrDeadline(deadline);
            if (e is null) return;
            if (e.Kind == EventKind.AbilityHit && e.Id == cast.Id) return;
        }
    }

    public Task Settle(double milliseconds = 100) => WaitMs(milliseconds);

    public async Task<GameEvent?> FindOrWaitForStatusWhere(
        IWorld world, Func<GameEvent, bool> match)
    {
        var live = world.ActiveStatuses().FirstOrDefault(match);
        if (live is not null) return live;
        return await WaitEvent(e => e.Kind == EventKind.StatusGain && match(e));
    }

    public async Task<GameEvent?> FindOrWaitForStatusWithin(
        IWorld world, Func<GameEvent, bool> match, double seconds)
    {
        var live = world.ActiveStatuses().FirstOrDefault(match);
        if (live is not null) return live;

        var deadline = _clock.Now + seconds;

        while (_clock.Now < deadline)
        {
            var e = await NextEventOrDeadline(deadline);
            if (e is null) return null;
            if (e.Kind == EventKind.StatusGain && match(e)) return e;
        }

        return null;
    }

    public async Task<GameEvent?> FindOrWaitForCast(
        IWorld world, Func<GameEvent, bool> match, bool onlyIfAlreadyCasting = false)
    {
        var live = world.ActiveCasts().FirstOrDefault(match);
        if (live is not null) return live;
        if (onlyIfAlreadyCasting) return null;
        return await WaitEvent(e => e.Kind == EventKind.CastStart && match(e));
    }

    public async Task<GameEvent?> FindOrWaitForStatus(
        IWorld world, uint statusId, Func<Actor, bool> on)
    {
        var live = world.ActiveStatuses()
            .FirstOrDefault(s => s.Id == statusId && s.Target is not null && on(s.Target));
        if (live is not null) return live;
        return await WaitEvent(e => e.Kind == EventKind.StatusGain && e.Id == statusId
                                    && e.Target is not null && on(e.Target));
    }

    private void Guard()
    {
        if (Stopped) throw new SequenceStopped();
    }

    private TaskCompletionSource<GameEvent?> Arm(double deadline, bool takesEvents)
    {
        var waiter = new TaskCompletionSource<GameEvent?>();
        _waiter = waiter;
        _deadline = deadline;
        _waiterTakesEvents = takesEvents;
        return waiter;
    }

    private async Task<GameEvent> NextEvent()
    {
        Guard();

        if (_pending.Count > 0) return _pending.Dequeue();

        var waiter = Arm(double.PositiveInfinity, takesEvents: true);
        var e = await waiter.Task;
        Guard();
        return e!;
    }

    private async Task<GameEvent?> NextEventOrDeadline(double deadline)
    {
        Guard();

        if (_pending.Count > 0) return _pending.Dequeue();

        var waiter = Arm(deadline, takesEvents: true);
        var e = await waiter.Task;
        Guard();
        return e;
    }
}
