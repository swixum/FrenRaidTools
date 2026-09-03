using FrenRaidTools.Engine;
using FrenRaidTools.Feed;

namespace FrenRaidTools;

public sealed class TickClock : IClock
{
    public double Now { get; set; }
}

public sealed class Runtime : IDisposable
{
    public const int LinesPerTick = 512;
    public const double PruneSeconds = 2.0;

    private readonly Configuration _config;
    private readonly CallBoard _board;
    private readonly Fight _fight;
    private readonly Diag _diag;

    private readonly TickClock _clock = new();
    private readonly RunGate _gate = new();
    private readonly CombatWatch _combat = new();
    private readonly ParserSocket _socket = new();
    private readonly IinactGate _iinact = new();
    private readonly ParserActorBook _book = new();
    private readonly NetworkLineReader _reader;
    private readonly FeedWorld _world;
    private readonly EventFeed _feed;
    private readonly SequenceHost _host;
    private readonly VfxLink _vfx;
    private readonly ClientLink _client;
    private readonly EffectLink _effects;
    private readonly ControlLink _controls;

    private bool _installed;
    private bool _replaying;
    private double _nextPrune;
    private ushort _zone;

    public Runtime(Configuration config, CallBoard board, Fight fight, Diag diag)
    {
        _config = config;
        _board = board;
        _fight = fight;
        _diag = diag;

        _reader = new NetworkLineReader(_book, _clock) { ReadsPositions = true };
        _world = new FeedWorld(_book, _clock);
        _host = new SequenceHost(_clock, _world, Sink);
        _feed = new EventFeed(_clock, Deliver);
        _vfx = new VfxLink(_book, e => _feed.Publish(EventSource.Client, e), () => _clock.Now);
        _client = new ClientLink(_book, e => _feed.Publish(EventSource.Client, e));
        _effects = new EffectLink(_book, e => _feed.Publish(EventSource.Client, e), () => _clock.Now);
        _controls = new ControlLink(_book, e => _feed.Publish(EventSource.Client, e), () => _clock.Now);
    }

    public bool Running { get; private set; }

    public long Lines => _reader.Read;

    public long Understood => _reader.Understood;

    public long Events => _feed.Delivered;

    public int Actors => _book.Count;

    public int Sequences => _host.Sequences.Count;

    public int Live => _host.RunningCount;

    public IReadOnlyList<string> Faults =>
        [.. _fight.Faults,
         .. _host.Faults,
         .. new[] { _vfx.Fault, _effects.Fault, _controls.Fault }.OfType<string>()];

    public bool Replaying => _replaying;

    public bool KnowsYou => _world.You is not null;

    public string? Blind =>
        !Running || KnowsYou
            ? null
            : "Cannot tell which character is you. Calls about your own debuffs stay quiet.";

    public bool SocketConnected => _socket.Connected;

    public bool VfxAttached => _vfx.Attached;

    public string VfxDetail => _vfx.Detail;

    public string ClientDetail => _client.Detail;

    public string EffectDetail => _effects.Detail;

    public string ControlDetail => _controls.Detail;

    public string SocketDetail =>
        _socket.Connected
            ? _socket.Dropped > 0
                ? $"Connected to {_socket.Endpoint}. {_socket.Dropped:n0} lines fell behind and were dropped."
                : _reader.Stamps.Lag > 1.0
                    ? $"Connected to {_socket.Endpoint}. Running {_reader.Stamps.Lag:0.0}s behind."
                    : $"Connected to {_socket.Endpoint}."
        : _socket.LastError is { } error ? error.EndsWith('.') ? error : error + "."
        : _socket.Enabled ? "Looking for a parser." : "Off.";

    public bool FeedUp => _iinact.Subscribed || _socket.Connected;

    public string FeedDetail =>
        _iinact.Subscribed ? "Connected to IINACT in-process."
        : _socket.Enabled || _socket.Connected ? SocketDetail
        : !_config.ParserOn ? "Off."
        : _iinact.LastError is { } error ? error.EndsWith('.') ? error : error + "."
        : "Looking for IINACT.";

    public void RetryFeed()
    {
        _iinact.Stop();
        _nextIpcTry = 0;
        _socket.Kick();
    }

    public const double IpcTrySeconds = 3.0;

    private double _nextIpcTry;

    public void Tick(double now)
    {
        _clock.Now = now;

        Watch();
        Install();
        Follow();
        Choices();

        _gate.ParserLive = _feed.ParserLive;
        _gate.HooksBroken = HooksBroken;
        _gate.SourceIpcOnly = _config.ParserSource == Configuration.SourceIinact;
        _gate.SourceSocketOnly = _config.ParserSource == Configuration.SourceAct;

        if (_gate.WantsIpc)
        {
            _iinact.Watch(_clock.Now);
            if (!_iinact.Subscribed && _clock.Now >= _nextIpcTry)
            {
                _nextIpcTry = _clock.Now + IpcTrySeconds;
                _iinact.Start();
            }
        }
        else if (_iinact.Subscribed)
        {
            _iinact.Stop();
        }

        _gate.IpcFeeding = _iinact.Subscribed;

        if (_gate.WantsSocket) _socket.Start(_config.ParserAddress);
        else if (_socket.Enabled) _socket.Stop();

        if (_gate.InTheFight)
        {
            _vfx.Start();
            _effects.Start();
            _controls.Start();
        }

        _client.On = _gate.ClientReadsActors;
        _effects.On = _client.On;
        _controls.On = Running;
        _client.Tick(_clock.Now);

        _feed.ClientOwnsEverything = _gate.ClientOwnsEverything;
        _feed.ParserAttached = _socket.Connected || _iinact.Subscribed;
        _socket.Drain(Line, LinesPerTick);
        _iinact.Drain(Line, LinesPerTick);

        if (now >= _nextPrune)
        {
            _nextPrune = now + PruneSeconds;
            _world.Prune();
        }

        _host.Tick();
    }

    private void Sink(Callout callout, GameEvent? on, IReadOnlyDictionary<string, object?> args)
    {
        var keyed = _fight.Catalog.WithKey(callout);
        _diag.Plan($"fire {keyed.Key} args {Diag.Args(args)}");

        if (_fight.Plan is { } plan) plan.Deliver(keyed, on, args, _board.Fire);
        else _board.Fire(keyed, on, args);
    }

    public bool HooksBroken => _vfx.Fault is not null || _controls.Fault is not null;

    private bool _watching;
    private float _speed = Game.NormalSpeed;
    private readonly HashSet<string> _faultsTold = [];

    private void Watch()
    {
        if (Game.You is { } you)
            _book.KnowYou(you.EntityId, you.Name.TextValue, Jobs.Name(you.ClassJob.RowId));

        if (_diag.On != _watching)
        {
            _watching = _diag.On;
            ArenaPos.Trace = _watching ? _diag.Bearing : null;
            _told = _watching ? Shape() : "";
            _nextSettle = _clock.Now + SettleEverySeconds;
        }

        if (!_diag.On) return;

        if (_faultsTold.Count > SequenceHost.MaxFaults) _faultsTold.Clear();

        foreach (var fault in Faults)
            if (_faultsTold.Add(fault))
                _diag.Note("FAULT", fault);

        Resettle();

        var speed = Game.InReplay ? Game.Speed() : Game.NormalSpeed;
        if (Math.Abs(speed - _speed) < 0.01f) return;

        _speed = speed;
        _diag.Note("replay", speed <= 0f ? "paused" : $"speed {speed:0.##}x");
    }

    public const int UnseenKindsShown = 12;

    public string Unseen()
    {
        var kinds = _reader.UnreadByKind
            .OrderByDescending(p => p.Value)
            .Take(UnseenKindsShown)
            .Select(p => $"{p.Key}={p.Value}");

        return $"unread {_reader.Unread} over {_reader.UnreadByKind.Count} kinds" +
               (_reader.UnreadOffKinds > 0 ? $" +{_reader.UnreadOffKinds} past the cap" : "") +
               $", skipped moves {_reader.Ignored}, stalls {_host.Stalls.Count}" +
               (_reader.UnreadByKind.Count > 0 ? "  [" + string.Join(" ", kinds) + "]" : "");
    }

    private void NoteUnseen() => _diag.Note("unseen", Unseen());

    public IReadOnlyList<SequenceStall> Stalls => _host.Stalls;

    public const double SettleEverySeconds = 2.0;

    private double _nextSettle;
    private string _told = "";

    private string Shape() =>
        string.Join('/',
            _zone, _replaying, Running, _feed.ParserLive, _socket.Connected, _iinact.Subscribed,
            HooksBroken, _world.You?.Name ?? "", _world.Party.Count, SeatSync.SeatFor(_config));

    private void Resettle()
    {
        if (_clock.Now < _nextSettle) return;
        _nextSettle = _clock.Now + SettleEverySeconds;

        var shape = Shape();
        if (shape == _told) return;

        _told = shape;
        foreach (var line in Setup().Split('\n'))
            if (line.Length > 0) _diag.Note("setup", line);
    }

    public string Setup()
    {
        var owner = _replaying ? "the game, because this is a duty replay"
            : _feed.ParserLive ? "the log for casts, hits, statuses, tethers and markers"
            : "the game, because no parser is reading";

        return string.Join('\n',
        [
            $"zone {Game.Zone} ({Game.ZoneName()})",
            $"replay {(_replaying ? "yes" : "no")}, running {(Running ? "yes" : "no")}",
            $"events come from {owner}",
            $"parser {FeedDetail}",
            $"hooks {(HooksBroken ? "BROKEN, the parser is covering what it can" : "attached")}",
            $"you {_world.You?.Name ?? "unknown"}, party {_world.Party.Count}, seat {(SeatSync.SeatFor(_config) is { Length: > 0 } seat ? seat : "none")}",
            $"roster names come from {Party.Source()}",
            "event lines are tagged [log] or [game] for where they came from",
        ]);
    }

    public void FightChanged()
    {
        _installed = false;
        _host.Clear();
    }

    private void Install()
    {
        if (_installed) return;
        if (!_fight.PlanReady) return;

        _host.ExpireCall = _board.Expire;
        _host.OnStall = stall => _diag.Note("STALL", stall.Line());
        _world.Options = _fight.Chosen;
        _world.Buddy = Buddy;
        _world.Seat = actor => _config.Roles.SlotOf(actor.Name);
        _world.Preset = () => WaymarkPresets.For(_fight.Key ?? "");
        if (_fight.RunsDancingMad) _fight.DancingMad.Install(_host);

        foreach (var part in _fight.Local)
        {
            try
            {
                if (part.Build is not null) _host.Add(part.Build());
                if (part.Extra is not null)
                    foreach (var extra in part.Extra(_world)) _host.Add(extra);
            }
            catch (Exception ex)
            {
                _fight.Faults.Add($"{part.Group}: {ex.Message}");
                Service.Log.Error(ex, "Local fight sequence would not install.");
            }
        }

        _installed = true;
    }

    private void Choices()
    {
        _fight.DancingMad.Earthquake.CleanseCall = _config.CleanseCallMode;
    }

    private Actor? Buddy()
    {
        var you = Party.YouName();
        if (you.Length == 0) return null;

        var buddy = _config.Roles.PartnerName(you);
        if (string.IsNullOrWhiteSpace(buddy)) return null;

        foreach (var actor in _book.Players)
            if (string.Equals(actor.Name, buddy, StringComparison.OrdinalIgnoreCase)) return actor;

        return null;
    }

    private void Follow()
    {
        var zone = (ushort)Game.Zone;
        var replay = Game.InReplay;

        if (zone != _zone)
        {
            _zone = zone;
            Wipe();
        }

        if (replay != _replaying)
        {
            _replaying = replay;
            Wipe();
            if (replay && _config.DiagInReplay && !_diag.On) _diag.Start();
        }

        _gate.Installed = _installed;
        _gate.Zone = zone;
        _gate.FightZone = _fight.Territory;
        Game.FightZone = _fight.Territory;
        Game.FightName = _fight.Name;
        _gate.Replaying = replay;
        _gate.ParserOn = _config.ParserOn;
        Running = _gate.Running;

        if (_combat.Take(Game.PartyFighting(), _clock.Now) is { } boundary)
        {
            _diag.Note("pull", boundary.Kind == EventKind.CombatStart ? "started" : "ended");
            if (boundary.Kind == EventKind.CombatEnd) NoteUnseen();
            _feed.Publish(EventSource.Client, boundary);
        }
    }

    private void Line(string line)
    {
        _feed.NoteParserLine();

        var e = _reader.Parse(line);
        if (e is null) return;

        _feed.Publish(EventSource.Parser, e);
    }

    private void Deliver(GameEvent e, EventSource from)
    {
        _diag.Event(e, from);
        _world.Take(e);

        if (e.Kind is EventKind.CombatStart or EventKind.CombatEnd or EventKind.ZoneChange)
        {
            _fight.Plan?.Reset();
            _host.Reset();
        }

        if (e.Kind is EventKind.CombatStart && _zone == _fight.Territory)
            _board.Clear();

        if (Running) _host.Feed(e);
    }

    public void Wipe()
    {
        _diag.Note("wipe", $"zone={_zone} replay={_replaying}");
        _combat.Forget();
        _reader.Restart();
        _fight.Plan?.Reset();
        _host.Reset();
        _world.Clear();
        _book.Clear();
        _feed.Reset();
        _client.Clear();
        _board.Clear();
    }

    public void Dispose()
    {
        ArenaPos.Trace = null;
        _socket.Dispose();
        _iinact.Dispose();
        _vfx.Dispose();
        _effects.Dispose();
        _controls.Dispose();
        _host.Reset();
    }
}
