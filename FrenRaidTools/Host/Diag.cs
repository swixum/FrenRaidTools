using System.Text;
using FrenRaidTools.Engine;

namespace FrenRaidTools;

public sealed class Diag : IDisposable
{
    public const int KeepDays = 90;
    public const int MaxLines = 200_000;
    public const int FlushEvery = 200;
    public const double FlushSeconds = 2.0;
    public const int QuietShown = 20;

    private static readonly object Disk = new();

    private readonly object _gate = new();
    private readonly StringBuilder _pending = new();

    private readonly Dictionary<string, int> _drops = [];
    private readonly Dictionary<string, int> _events = [];
    private readonly Dictionary<uint, int> _quiet = [];

    private string? _path;
    private string _stem = "";
    private int _part;
    private int _written;
    private int _sinceFlush;
    private double _nextFlush;
    private double _now;
    private int _calls;
    private int _readFails;

    public bool On { get; private set; }

    public int Lines => _written;

    public int Part => _part;

    public string Where => _path ?? "not started";

    public string Detail =>
        !On ? "Off."
        : _part > 1 ? $"{_written:n0} lines, part {_part}. {_path}"
        : $"{_written:n0} lines. {_path}";

    public void Tick(double now)
    {
        _now = now;
        if (!On || now < _nextFlush) return;

        _nextFlush = now + FlushSeconds;
        Flush();
    }

    public void Start()
    {
        lock (_gate)
        {
            if (On) return;

            try
            {
                var dir = Service.PluginInterface.ConfigDirectory;
                dir.Create();
                _stem = Path.Combine(dir.FullName, $"replay-diag-{DateTime.Now:yyyyMMdd-HHmmss}");
                _part = 1;
                _path = $"{_stem}.log";
                var home = dir.FullName;
                Task.Run(() => Sweep(home));
                _written = 0;
                On = true;
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "Could not open the diagnostics file.");
                On = false;
                _path = null;
            }
        }

        _calls = 0;
        _readFails = 0;
        _drops.Clear();
        _events.Clear();
        _quiet.Clear();

        Note("diag", "started");
        Note("setup", $"wall={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}");
        Note("setup", $"version={typeof(Diag).Assembly.GetName().Version}");
        Header();
    }

    private static void Sweep(string dir)
    {
        var cutoff = DateTime.Now.AddDays(-KeepDays);

        try
        {
            lock (Disk)
                foreach (var file in new DirectoryInfo(dir).GetFiles("replay-diag*.log"))
                    if (file.LastWriteTime < cutoff)
                        try { file.Delete(); } catch { }
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Could not sweep old diagnostics files.");
        }
    }

    public Func<string>? Describe { get; set; }

    private void Header()
    {
        if (Describe is null) return;

        try
        {
            foreach (var line in Describe().Split('\n'))
                if (line.Length > 0) Note("setup", line);
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Could not write the diagnostics header.");
        }
    }

    public void Stop()
    {
        if (!On) return;

        Note("counts", $"calls={_calls} readfails={_readFails} lines={_written} part={_part}");
        foreach (var (code, n) in _drops)
            Note("counts", $"drop {code}={n}");
        foreach (var (kind, n) in _events)
            Note("counts", $"event {kind}={n}");

        var quiet = _quiet.OrderByDescending(pair => pair.Value).ToList();
        foreach (var (id, n) in quiet.Take(QuietShown))
            Note("counts", $"actorcontrol skipped {id:X}={n}");
        if (quiet.Count > QuietShown)
            Note("counts",
                $"actorcontrol skipped {quiet.Count - QuietShown} more ids, " +
                $"{quiet.Skip(QuietShown).Sum(pair => pair.Value)} lines");

        Note("diag", "stopped");
        FlushNow();
        lock (_gate) On = false;
    }

    public void Toggle()
    {
        if (On) Stop();
        else Start();
    }

    public void Note(string tag, string what)
    {
        if (!On) return;

        lock (_gate)
        {
            if (_written >= MaxLines) Roll();

            Line(tag, what);
            if (_sinceFlush >= FlushEvery) FlushLocked();
        }
    }

    private void Line(string tag, string what)
    {
        _pending.Append(_now.ToString("0.00")).Append("  ")
            .Append(tag).Append("  ").Append(what).Append('\n');

        _written++;
        _sinceFlush++;
    }

    private void Roll()
    {
        var next = $"{_stem}-{_part + 1}.log";

        Line("diag", $"full at {MaxLines:n0} lines, continues in {Path.GetFileName(next)}");
        FlushLocked();

        _part++;
        _path = next;
        _written = 0;

        Line("diag", $"part {_part}, continued from {Path.GetFileName(_stem)}.log");
    }

    public void Event(GameEvent e, EventSource from)
    {
        if (!On) return;

        var name = e.Kind.ToString();
        _events[name] = _events.GetValueOrDefault(name) + 1;

        if (e.Kind == EventKind.ActorControl && !ControlIds.Watched.Contains(e.Id))
        {
            _quiet[e.Id] = _quiet.GetValueOrDefault(e.Id) + 1;
            return;
        }

        var source = Who(e.Source);
        var target = Who(e.Target);
        var duration = e.Duration > 0 ? $" dur={e.Duration:0.0}" : "";
        var first = e.FirstTarget ? " first" : "";
        var late = _now - e.At;
        var behind = late > 0.05 ? $" late={late:0.00}" : "";
        var args = e.Kind == EventKind.ActorControl && (e.Arg1 | e.Arg2 | e.Arg3 | e.Arg4) != 0
            ? $" args={e.Arg1:X}/{e.Arg2:X}/{e.Arg3:X}/{e.Arg4:X}"
            : "";

        Note("event",
            $"[{(from == EventSource.Parser ? "log" : "game")}] " +
            $"{e.Kind} id={e.Id:X}{duration}{first}{behind}{args} src={source} tgt={target}");
    }

    public void Call(string key, string description, string text, string speech,
        double countdownEnds, double expires, string evidence = "", bool readFails = false)
    {
        if (!On) return;

        var spoken = speech == text ? "" : $" | says '{speech}'";
        var clock = countdownEnds > _now ? $" ends={countdownEnds - _now:0.0}" : "";
        var why = evidence.Length > 0 ? $" why={evidence}" : "";
        var hidden = readFails ? " readfail" : "";

        _calls++;
        if (readFails) _readFails++;

        Note("CALL", $"{key} ({description}) '{text}'{spoken}{clock} holds={expires - _now:0.0}{why}{hidden}");
    }

    public void Dropped(string code, string why, string key, double left)
    {
        if (!On) return;

        _drops[code] = _drops.GetValueOrDefault(code) + 1;
        Note("dropped", $"{key} [{code}] {why}, {left:0.0}s of its hold left");
    }

    public void Plan(string what) => Note("plan", what);

    public void Bearing(ArenaBearing b)
    {
        if (!On) return;

        if (b.From == "facing")
        {
            Note("bearing", $"facing heading={b.Degrees:0.0} -> {b.Sector.Name()}");
            return;
        }

        Note("bearing",
            $"{b.From} at=({b.X:0.0},{b.Y:0.0}) center=({b.CenterX:0.0},{b.CenterY:0.0}) " +
            $"off=({b.OffsetX:0.0},{b.OffsetY:0.0}) angle={b.Degrees:0.0} -> {b.Sector.Name()}");
    }

    public static string Who(Actor? actor)
    {
        if (actor is null) return "-";

        var name = actor.Name.Length > 0 ? actor.Name : actor.ObjectId.ToString("X");
        var job = actor.Job.Length > 0 ? $"/{actor.Job}" : "";
        var baseId = actor.BaseId != 0 ? $"#{actor.BaseId}" : "";
        var you = actor.IsYou ? "*" : "";
        var at = actor.Pos.Known ? $"@({actor.Pos.X:0.0},{actor.Pos.Y:0.0})" : "";
        var facing = $"^{ArenaPos.PointOf(actor.Heading).Name()}({actor.Heading * 180.0 / Math.PI:0})";

        return $"{you}{name}{job}{baseId}{at}{facing}";
    }

    public static string Args(IReadOnlyDictionary<string, object?>? args)
    {
        if (args is null || args.Count == 0) return "-";

        var parts = new List<string>();
        foreach (var (key, value) in args)
        {
            if (value is null) continue;
            var text = value is System.Collections.IEnumerable list and not string
                ? string.Join("/", list.Cast<object?>().Select(x => x?.ToString() ?? ""))
                : value.ToString();
            if (string.IsNullOrEmpty(text)) continue;
            parts.Add($"{key}={text}");
        }

        return parts.Count == 0 ? "-" : string.Join(" ", parts);
    }

    private void Flush()
    {
        lock (_gate) FlushLocked();
    }

    private void FlushLocked()
    {
        _sinceFlush = 0;
        if (_pending.Length == 0 || _path is null) return;

        var batch = _pending.ToString();
        var path = _path;
        _pending.Clear();

        Task.Run(() => Write(path, batch));
    }

    private void FlushNow()
    {
        string batch;
        string? path;

        lock (_gate)
        {
            _sinceFlush = 0;
            if (_pending.Length == 0 || _path is null) return;
            batch = _pending.ToString();
            path = _path;
            _pending.Clear();
        }

        Write(path, batch);
    }

    private static void Write(string path, string batch)
    {
        lock (Disk)
        {
            try
            {
                File.AppendAllText(path, batch);
            }
            catch (Exception ex)
            {
                Service.Log.Warning(ex, "Could not write the diagnostics file.");
            }
        }
    }

    public void Dispose() => Stop();
}
