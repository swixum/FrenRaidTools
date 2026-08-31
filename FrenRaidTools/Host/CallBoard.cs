using FrenRaidTools.Engine;

namespace FrenRaidTools;

public sealed class LiveCall
{
    public required string Key { get; init; }
    public int Phase { get; init; }
    public required string Description { get; init; }
    public required string Text { get; init; }
    public required string Speech { get; init; }
    public required double Raised { get; init; }
    public required double Expires { get; set; }
    public double CountdownEnds { get; set; }
    public double LingerUntil { get; init; }
    public bool Countdown { get; init; }
    public uint AnchorActor { get; init; }
    public uint AnchorStatus { get; init; }
    public double AnchorOffset { get; init; }
    public bool Test { get; init; }
    public IReadOnlyList<uint> Icons { get; init; } = [];

    public string Rendered(double now, bool showCountdown)
    {
        if (!Countdown || !showCountdown) return Text;

        var left = CountdownEnds - now;
        if (!Callout.ShowsNumber(left)) return Text;
        if (left < 0) left = 0;

        return $"{Text} ({left:0.0})";
    }
}

public sealed class CallLog
{
    public required string Description { get; init; }
    public required string Text { get; init; }
    public required double At { get; init; }
    public required bool Muted { get; init; }
    public required bool Test { get; init; }
}

public sealed class CallBoard
{
    private const int HistoryCap = 120;

    public const int LiveCap = 16;

    private readonly object _gate = new();
    private readonly List<LiveCall> _live = [];
    private readonly List<CallLog> _history = [];
    private readonly Configuration _config;
    private readonly Speech _speech;
    private readonly Diag _diag;
    private readonly SpeechQueue _lines = new();

    private double _now;

    public CallBoard(Configuration config, Speech speech, Diag diag)
    {
        _config = config;
        _speech = speech;
        _diag = diag;
    }

    public Func<double>? FightNow { get; set; }

    public IReadOnlyList<CatalogEntry> Catalog { get; private set; } = [];

    public int Fired { get; private set; }

    public int Skipped { get; private set; }

    public string LastFault { get; private set; } = "";

    public string Notice { get; private set; } = "";

    public void Note(string text)
    {
        Notice = text;
        _noticeUntil = _now + NoticeSeconds;
    }

    private double _noticeUntil;

    private const double NoticeSeconds = 5.0;

    private bool _planRidesItself;

    public bool PlanRidesItself => _planRidesItself;

    public void PlanCarriesItsOwnCalls() => _planRidesItself = true;

    public void SetCatalog(IReadOnlyList<CatalogEntry> entries)
    {
        Catalog = entries;
        SeedQuietCalls();
        PruneMutes();
    }

    private void SeedQuietCalls()
    {
        if (Catalog.Count == 0) return;
        if (!QuietSeed.Apply(Catalog, _config.MutedCalls, _config.SeededQuiet)) return;

        _config.Save(_now);
    }

    public void SetCatalog(CalloutCatalog catalog) => SetCatalog(catalog.Entries);

    private void PruneMutes()
    {
        if (Catalog.Count == 0 || _config.MutedCalls.Count == 0) return;

        var known = new HashSet<string>(Catalog.Select(e => e.Key), StringComparer.Ordinal);
        _config.MutedCalls.RemoveWhere(key => !known.Contains(key));

        foreach (var stale in _config.CallEdits.Keys.Where(key => !known.Contains(key)).ToList())
            _config.CallEdits.Remove(stale);
    }

    public bool Muted(string key) => key.Length > 0 && _config.MutedCalls.Contains(key);

    public void SetMuted(string key, bool muted)
    {
        if (key.Length == 0) return;
        if (muted) _config.MutedCalls.Add(key);
        else _config.MutedCalls.Remove(key);
    }

    public void Fire(Callout callout, GameEvent? on, IReadOnlyDictionary<string, object?> args)
    {
        try
        {
            Raise(callout, on, args, test: false);
        }
        catch (Exception ex)
        {
            LastFault = $"{callout.Description}: {ex.Message}";
            Service.Log.Error(ex, "Call failed.");
        }
    }

    public void Test(Callout callout)
    {
        try
        {
            Raise(callout, null, EmptyArgs, test: true);
            if (!_config.OverlayOn) Note("Overlay is off. That one only went to Recent.");
        }
        catch (Exception ex)
        {
            LastFault = $"{callout.Description}: {ex.Message}";
        }
    }

    private static readonly Dictionary<string, object?> EmptyArgs = [];

    private void Raise(Callout callout, GameEvent? on, IReadOnlyDictionary<string, object?> args, bool test)
    {
        var now = _now;

        if (callout.FromPlan && !test) _planRidesItself = true;

        if (!test && !_config.CallsOn)
        {
            Record(callout.Description, "", now, muted: true, test: false);
            return;
        }

        if (!test && Muted(callout.Key))
        {
            Skipped++;
            _diag.Note("muted", callout.Key);
            Record(callout.Description, "", now, muted: true, test: false);
            return;
        }

        var edit = _config.EditFor(callout.Key);
        var body = edit is { Text.Length: > 0 } ? edit.Text : callout.Text;
        var spoken = edit is { Speech.Length: > 0 } ? edit.Speech : callout.Speech;

        var countdown = callout.FromDuration && body.EndsWith(Callout.CountdownToken, StringComparison.Ordinal);
        if (countdown) body = body[..^Callout.CountdownToken.Length];

        var text = Fill(body, args, test);
        var speech = Fill(spoken, args, test);

        if (!_planRidesItself)
            (text, speech) = PlanSource.WithSpot(_config, callout.Key, args, text, speech);

        speech = SpeechText.Plain(speech);

        var duration = callout.CountdownDuration(on?.Duration, test ? 8.0 : 0.0);
        var linger = Math.Max(0.5, callout.LingerSeconds * _config.OverlayLingerScale);

        var remaining = Callout.Remaining(on?.At, duration, FightNow?.Invoke() ?? now);
        var ends = now + remaining + callout.CountdownOffsetSeconds;
        var ticking = countdown && remaining > 0;

        var anchored = ticking && on is { Kind: EventKind.StatusGain, Target: not null };

        var call = new LiveCall
        {
            Key = callout.Key,
            Phase = callout.Phase,
            Description = callout.Description,
            Text = text,
            Speech = speech,
            Raised = now,
            Expires = Callout.Expiry(now, linger, ends, ticking, callout.HoldsToCountdown),
            CountdownEnds = ends,
            LingerUntil = now + linger,
            Countdown = ticking,
            AnchorActor = anchored ? on!.Target!.ObjectId : 0,
            AnchorStatus = anchored ? on!.Id : 0,
            AnchorOffset = callout.CountdownOffsetSeconds,
            Test = test,
            Icons = _config.OverlayIcons ? Ui.Icons.For(callout, on) : [],
        };

        lock (_gate)
        {
            if (!test) Reached(call.Phase);
            _live.RemoveAll(c => c.Key.Length > 0 && c.Key == call.Key);
            _live.Add(call);
            Trim();
        }

        Fired++;
        if (!test) _diag.Call(callout.Key, callout.Description, text, speech, ends, call.Expires);
        Record(callout.Description, text, now, muted: false, test);
        Queue(speech, now + callout.SpeechDelaySeconds, callout.Rank, callout.RepeatsAloud, test);
    }

    private string Fill(string template, IReadOnlyDictionary<string, object?> args, bool test)
    {
        if (!template.Contains('{', StringComparison.Ordinal)) return template;

        var result = Placeholders.Fill(template, args);
        if (result.Ok) return Placeholders.Tidy(result.Text);

        if (test) return CallText.Words(result.Text);

        LastFault = $"Unresolved: {string.Join(", ", result.Unresolved)}";
        return Placeholders.Bare(result.Text);
    }

    private void Queue(string speech, double at, CallRank rank, bool repeatsAloud, bool test)
    {
        if (!_config.TtsOn || string.IsNullOrWhiteSpace(speech)) return;
        if (!test && !Game.Fighting) return;

        lock (_gate) _lines.Add(speech, at, rank, repeatsAloud);
    }

    private void Record(string description, string text, double at, bool muted, bool test)
    {
        lock (_gate)
        {
            _history.Add(new CallLog
            {
                Description = description,
                Text = text,
                At = at,
                Muted = muted,
                Test = test,
            });

            if (_history.Count > HistoryCap) _history.RemoveRange(0, _history.Count - HistoryCap);
        }
    }

    private readonly PhaseGate _phases = new();

    public int Phase => _phases.Phase;

    public int LeftBehind => _phases.Dropped;

    private void Reached(int phase)
    {
        if (!_phases.Enter(phase)) return;

        foreach (var call in _live.Where(c => _phases.LeftBehind(c.Phase)))
            _diag.Dropped($"left behind in phase {call.Phase}", call.Key, call.Expires - _now);

        _phases.Dropping(_live.RemoveAll(c => _phases.LeftBehind(c.Phase)));
    }

    private void Trim()
    {
        if (_live.Count <= LiveCap) return;

        var over = _live.Count - LiveCap;
        foreach (var call in _live.Take(over))
            _diag.Dropped($"over the {LiveCap} call cap", call.Key, call.Expires - _now);

        _live.RemoveRange(0, over);
    }

    public void Tick(double now)
    {
        _now = now;

        if (Notice.Length > 0 && now >= _noticeUntil) Notice = "";

        lock (_gate)
        {
            foreach (var call in _live)
            {
                if (!call.Countdown || call.AnchorStatus == 0) continue;

                var left = StatusRemaining?.Invoke(call.AnchorActor, call.AnchorStatus);
                if (left is not { } remaining) continue;

                var ends = Callout.Resync(call.CountdownEnds, now + remaining + call.AnchorOffset);
                if (ends == call.CountdownEnds) continue;

                var holdsToZero = call.Expires >= call.CountdownEnds;
                call.CountdownEnds = ends;
                if (holdsToZero) call.Expires = Math.Max(call.LingerUntil, ends);
            }

            _live.RemoveAll(c => now >= c.Expires);

            _lines.MinGapSeconds = _config.TtsMinGap;
            _lines.Pump(now, Say);
        }
    }

    public Func<uint, uint, double?>? StatusRemaining { get; set; }

    private bool Say(string line) =>
        _speech.Say(line, _config.TtsRate, _config.TtsVolume, _config.TtsVoice);

    public int SpeechWaiting => _lines.Waiting;

    public int SpeechDropped => _lines.Dropped + _lines.Stale;

    public List<LiveCall> Visible()
    {
        lock (_gate) return [.. _live];
    }

    public int LiveCount
    {
        get { lock (_gate) return _live.Count; }
    }

    public bool HasTest
    {
        get
        {
            lock (_gate)
            {
                foreach (var call in _live)
                    if (call.Test) return true;
                return false;
            }
        }
    }

    public List<CallLog> History()
    {
        lock (_gate) return [.. _history];
    }

    public void ClearHistory()
    {
        lock (_gate) _history.Clear();
    }

    public void Expire(string key)
    {
        if (key.Length == 0) return;
        lock (_gate) _live.RemoveAll(c => c.Key == key);
    }

    public void Clear()
    {
        lock (_gate)
        {
            _live.Clear();
            _lines.Reset();
            _phases.Reset();
        }
    }
}
