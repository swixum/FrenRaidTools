namespace FrenRaidTools.Engine;

public sealed class SequenceHost
{
    private sealed class Live
    {
        public required SequenceRun Run { get; init; }
        public required Task Task { get; init; }
        public required double Deadline { get; init; }
    }

    public const int MaxRaised = 64;

    private readonly List<Sequence> _sequences = [];
    private readonly Queue<GameEvent> _raised = new();
    private readonly Dictionary<string, Live> _running = [];
    private readonly Dictionary<string, int> _invocations = [];
    private readonly SequenceScheduler _scheduler = new();
    private readonly IClock _clock;
    private readonly IWorld _world;
    private readonly CallSink _sink;

    public SequenceHost(IClock clock, IWorld world, CallSink sink)
    {
        _clock = clock;
        _world = world;
        _sink = sink;
    }

    public IReadOnlyList<Sequence> Sequences => _sequences;

    public IWorld World => _world;

    public List<string> Faults { get; } = [];

    public List<Action> ResetHooks { get; } = [];

    public Action<string>? ExpireCall { get; set; }

    public void Add(Sequence sequence) => _sequences.Add(sequence);

    public void AddRange(IEnumerable<Sequence> sequences) => _sequences.AddRange(sequences);

    public int RunningCount => _running.Count;

    public int RaisedDropped { get; private set; }

    public void Feed(GameEvent e)
    {
        if (e.Kind is EventKind.CombatStart or EventKind.CombatEnd or EventKind.ZoneChange)
        {
            Reset();
            return;
        }

        Expire();

        foreach (var live in _running.Values.ToList())
            live.Run.Feed(e);

        foreach (var sequence in _sequences)
        {
            if (_running.ContainsKey(sequence.Name)) continue;
            if (!sequence.Starts(e)) continue;
            Start(sequence, e);
        }

        Pump();
    }

    public int Pump()
    {
        var total = 0;
        while (true)
        {
            var ran = _scheduler.Pump();
            total += ran;

            var raised = Deliver();
            var woke = Wake();
            if (ran == 0 && woke == 0 && raised == 0) break;
        }

        Sweep();
        return total;
    }

    private int Deliver()
    {
        var delivered = 0;
        while (_raised.Count > 0)
        {
            var e = _raised.Dequeue();
            delivered++;
            foreach (var live in _running.Values.ToList())
                live.Run.Feed(e);
        }
        return delivered;
    }

    private void Raise(GameEvent e)
    {
        if (_raised.Count >= MaxRaised)
        {
            _raised.Dequeue();
            RaisedDropped++;
        }
        _raised.Enqueue(e);
    }

    private int Wake()
    {
        var now = _clock.Now;
        var woke = 0;
        foreach (var live in _running.Values.ToList())
            if (live.Run.WakeIfDue(now))
                woke++;
        return woke;
    }

    public void Tick()
    {
        Expire();
        Pump();
    }

    public double NextWake()
    {
        var next = double.PositiveInfinity;
        foreach (var live in _running.Values)
        {
            if (live.Run.NextDeadline < next) next = live.Run.NextDeadline;
            if (live.Deadline < next) next = live.Deadline;
        }
        return next;
    }

    private void Start(Sequence sequence, GameEvent start)
    {
        var invocation = _invocations.GetValueOrDefault(sequence.Name);
        _invocations[sequence.Name] = invocation + 1;

        var run = new SequenceRun(_sink, _clock)
        {
            StartedAt = _clock.Now,
            ExpireCall = ExpireCall,
            Emit = Raise,
        };
        run.SetParam("event", start);

        var task = _scheduler.Under(() => Guard(sequence, start, run, invocation));

        _running[sequence.Name] = new Live
        {
            Run = run,
            Task = task,
            Deadline = _clock.Now + sequence.TimeoutSeconds,
        };
    }

    private async Task Guard(Sequence sequence, GameEvent start, SequenceRun run, int invocation)
    {
        try
        {
            await sequence.Run(start, run, invocation);
        }
        catch (SequenceStopped)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Faults.Add($"{sequence.Name}: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            run.Stop();
        }
    }

    private void Sweep()
    {
        foreach (var (name, live) in _running.ToList())
            if (live.Task.IsCompleted)
                _running.Remove(name);
    }

    private void Expire()
    {
        var now = _clock.Now;
        foreach (var (name, live) in _running.ToList())
        {
            if (live.Task.IsCompleted)
            {
                _running.Remove(name);
                continue;
            }

            if (now < live.Deadline) continue;
            live.Run.Stop();
            _running.Remove(name);
        }
    }

    public void Reset()
    {
        foreach (var live in _running.Values.ToList())
            live.Run.Stop();

        _raised.Clear();
        Pump();
        _running.Clear();
        _invocations.Clear();

        foreach (var hook in ResetHooks)
            hook();
    }
}
