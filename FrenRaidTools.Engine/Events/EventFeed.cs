namespace FrenRaidTools.Engine;

public enum EventSource
{
    Client,
    Parser,
}

public sealed class EventFeed
{
    public static readonly IReadOnlySet<EventKind> ParserOwned = new HashSet<EventKind>
    {
        EventKind.CastStart,
        EventKind.AbilityHit,
        EventKind.StatusGain,
        EventKind.StatusLose,
        EventKind.Tether,
        EventKind.HeadMarker,
        EventKind.StatusLoopVfx,
    };

    public static readonly IReadOnlySet<EventKind> ClientOnly = new HashSet<EventKind>
    {
        EventKind.ActorControl,
        EventKind.ActorMoved,
        EventKind.ZoneChange,
        EventKind.CombatStart,
        EventKind.CombatEnd,
    };

    public const double ParserStaleSeconds = 10.0;

    private readonly IClock _clock;
    private readonly Action<GameEvent, EventSource> _deliver;

    private double _lastParserAt = double.NegativeInfinity;

    public EventFeed(IClock clock, Action<GameEvent, EventSource> deliver)
    {
        _clock = clock;
        _deliver = deliver;
    }

    public EventFeed(IClock clock, Action<GameEvent> deliver)
        : this(clock, (e, _) => deliver(e))
    {
    }

    public bool ParserAttached { get; set; }

    public bool ParserLive =>
        ParserAttached && _clock.Now - _lastParserAt <= ParserStaleSeconds;

    public long Delivered { get; private set; }

    public long SuppressedFromClient { get; private set; }

    public long DroppedFromParser { get; private set; }

    public bool ClientOwnsEverything { get; set; }

    public void NoteParserLine() => _lastParserAt = _clock.Now;

    public void Publish(EventSource from, GameEvent e)
    {
        if (from == EventSource.Parser)
        {
            _lastParserAt = _clock.Now;

            if (ClientOwnsEverything || ClientOnly.Contains(e.Kind))
            {
                DroppedFromParser++;
                return;
            }

            Deliver(e, from);
            return;
        }

        if (!ClientOwnsEverything && ParserLive && ParserOwned.Contains(e.Kind))
        {
            SuppressedFromClient++;
            return;
        }

        Deliver(e, from);
    }

    private void Deliver(GameEvent e, EventSource from)
    {
        Delivered++;
        _deliver(e, from);
    }

    public void Reset()
    {
        _lastParserAt = double.NegativeInfinity;
        Delivered = 0;
        SuppressedFromClient = 0;
        DroppedFromParser = 0;
    }

    public string Explain(EventKind kind) =>
        ClientOnly.Contains(kind)
            ? "the client only; no log line carries it"
            : ParserOwned.Contains(kind)
                ? ParserLive
                    ? "the parser, while it is reading"
                    : "the client, until a parser attaches"
                : "the client";
}
