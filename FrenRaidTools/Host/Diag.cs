using System.Text;
using FrenRaidTools.Engine;

namespace FrenRaidTools;

public sealed class Diag : IDisposable
{
    public const string FileName = "replay-diag.log";
    public const string PreviousFileName = "replay-diag-prev.log";
    public const int MaxLines = 200_000;
    public const int FlushEvery = 200;
    public const double FlushSeconds = 2.0;

    private readonly object _gate = new();
    private readonly StringBuilder _pending = new();

    private string? _path;
    private int _written;
    private int _sinceFlush;
    private double _nextFlush;
    private double _now;

    public bool On { get; private set; }

    public int Lines => _written;

    public string Where => _path ?? "not started";

    public string Detail =>
        !On ? "Off."
        : _written >= MaxLines ? $"Full at {MaxLines:n0} lines. {_path}"
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
                _path = Path.Combine(dir.FullName, FileName);
                Keep(dir.FullName);
                File.WriteAllText(_path, "");
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

        Note("diag", "started");
        Header();
    }

    private void Keep(string dir)
    {
        if (_path is null || !File.Exists(_path)) return;

        try
        {
            File.Move(_path, Path.Combine(dir, PreviousFileName), overwrite: true);
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Could not keep the previous diagnostics file.");
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

        Note("diag", "stopped");
        Flush();
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
            if (_written >= MaxLines) return;

            _pending.Append(_now.ToString("0.00")).Append("  ")
                .Append(tag).Append("  ").Append(what).Append('\n');

            _written++;
            if (++_sinceFlush >= FlushEvery) FlushLocked();
        }
    }

    public void Event(GameEvent e, EventSource from)
    {
        if (!On) return;

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

    public void Call(string key, string description, string text, string speech)
    {
        if (!On) return;

        var spoken = speech == text ? "" : $" | says '{speech}'";
        Note("CALL", $"{key} ({description}) '{text}'{spoken}");
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

        try
        {
            File.AppendAllText(_path, _pending.ToString());
            _pending.Clear();
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Could not write the diagnostics file.");
            _pending.Clear();
        }
    }

    public void Dispose() => Stop();
}
