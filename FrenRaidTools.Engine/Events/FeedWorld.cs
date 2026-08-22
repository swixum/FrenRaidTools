namespace FrenRaidTools.Engine;

public sealed class FeedWorld : IWorld
{
    public const int MaxStatuses = 512;
    public const int MaxCasts = 128;
    public const double CastGraceSeconds = 2.0;
    public const double StatusGraceSeconds = 2.0;

    private readonly ParserActorBook _book;
    private readonly IClock _clock;

    private readonly Dictionary<(uint Status, uint Target), GameEvent> _statuses = [];
    private readonly Dictionary<(uint Source, uint Ability), GameEvent> _casts = [];

    public FeedWorld(ParserActorBook book, IClock clock)
    {
        _book = book;
        _clock = clock;
    }

    public ParserActorBook Book => _book;

    public Func<string, string?>? Options { get; set; }

    public string? Chosen(string optionKey) => Options?.Invoke(optionKey);

    public Func<Actor?>? Buddy { get; set; }

    public Actor? Partner() => Buddy?.Invoke();

    public Func<Actor, int>? Seat { get; set; }

    public int SeatOf(Actor actor) => Seat?.Invoke(actor) ?? -1;

    public int StatusCount => _statuses.Count;

    public int CastCount => _casts.Count;

    public int Forgotten { get; private set; }

    public void Take(GameEvent e)
    {
        switch (e.Kind)
        {
            case EventKind.StatusGain:
                Gain(e);
                break;

            case EventKind.StatusLose:
                if (e.Target is { } lost) _statuses.Remove((e.Id, lost.ObjectId));
                break;

            case EventKind.CastStart:
                Cast(e);
                break;

            case EventKind.AbilityHit:
                if (e.Source is { } from) _casts.Remove((from.ObjectId, e.Id));
                break;

            case EventKind.ZoneChange:
            case EventKind.CombatStart:
            case EventKind.CombatEnd:
                Clear();
                break;
        }
    }

    private void Gain(GameEvent e)
    {
        if (e.Target is not { } target) return;

        if (!_statuses.ContainsKey((e.Id, target.ObjectId)))
            MakeRoom(_statuses, MaxStatuses);

        _statuses[(e.Id, target.ObjectId)] = e;
    }

    private void Cast(GameEvent e)
    {
        if (e.Source is not { } source) return;

        if (!_casts.ContainsKey((source.ObjectId, e.Id)))
            MakeRoom(_casts, MaxCasts);

        _casts[(source.ObjectId, e.Id)] = e;
    }

    public void Prune()
    {
        var now = _clock.Now;

        foreach (var (key, cast) in _casts.ToList())
        {
            if (now < cast.At + cast.Duration + CastGraceSeconds) continue;
            _casts.Remove(key);
            Forgotten++;
        }

        foreach (var (key, status) in _statuses.ToList())
        {
            if (status.Duration <= 0) continue;
            if (now < status.At + status.Duration + StatusGraceSeconds) continue;
            _statuses.Remove(key);
            Forgotten++;
        }
    }

    private void MakeRoom<TKey>(Dictionary<TKey, GameEvent> held, int cap) where TKey : notnull
    {
        Prune();
        if (held.Count < cap) return;

        var oldest = held.First();
        foreach (var pair in held)
            if (pair.Value.At < oldest.Value.At) oldest = pair;

        held.Remove(oldest.Key);
        Forgotten++;
    }

    public void Clear()
    {
        _statuses.Clear();
        _casts.Clear();
    }

    public IReadOnlyList<GameEvent> ActiveCasts() => [.. _casts.Values];

    public IReadOnlyList<GameEvent> ActiveStatuses() => [.. _statuses.Values];

    public Actor? You => _book.You;

    public IReadOnlyList<Actor> Party => [.. _book.Players];

    public Actor? Latest(Actor actor) => _book.Find(actor.ObjectId) ?? actor;

    public IReadOnlyList<Actor> NpcsById(uint baseId) =>
        [.. _book.Npcs.Where(n => n.BaseId == baseId)];
}
