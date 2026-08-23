namespace FrenRaidTools.Engine;

public sealed class NetworkLineReader
{
    private readonly ParserActorBook _book;
    private readonly IClock _clock;

    public NetworkLineReader(ParserActorBook book, IClock clock)
    {
        _book = book;
        _clock = clock;
    }

    public ParserActorBook Book => _book;

    public long Read { get; private set; }

    public long Understood { get; private set; }

    public bool ReadsPositions { get; set; }

    public long Moves { get; private set; }

    public const double PlacedGraceSeconds = 1.0;

    public const int MaxPlaced = 64;

    private readonly Dictionary<uint, double> _placed = [];

    public long Ignored { get; private set; }

    private readonly LogStamps _stamps = new();

    public LogStamps Stamps => _stamps;

    public void Restart()
    {
        _stamps.Reset();
        _placed.Clear();
    }

    public int PlacedCount => _placed.Count;

    private void NotePlaced(uint id, double at)
    {
        if (_placed.Count >= MaxPlaced && !_placed.ContainsKey(id))
        {
            foreach (var (key, when) in _placed.ToList())
                if (at - when > PlacedGraceSeconds) _placed.Remove(key);

            while (_placed.Count >= MaxPlaced)
            {
                var oldest = _placed.First();
                foreach (var pair in _placed)
                    if (pair.Value < oldest.Value) oldest = pair;
                _placed.Remove(oldest.Key);
            }
        }

        _placed[id] = at;
    }

    private bool JustPlaced(uint id, double at) =>
        _placed.TryGetValue(id, out var when) && at - when <= PlacedGraceSeconds;

    public GameEvent? Parse(string line)
    {
        if (string.IsNullOrEmpty(line)) return null;

        Read++;
        var fields = NetworkLine.Split(line);
        var kind = NetworkLine.Kind(fields);
        var at = _stamps.At(NetworkLine.Stamp(fields), _clock.Now);

        switch (kind)
        {
            case NetworkLine.AddCombatant:
            {
                _book.Learn(fields);
                var actor = _book.Find(NetworkLine.Hex(fields, 2));
                if (actor is null) return null;
                Understood++;
                return new GameEvent { Kind = EventKind.ActorMoved, At = at, Target = actor };
            }

            case NetworkLine.RemoveCombatant:
                _book.Forget(NetworkLine.Hex(fields, 2));
                return null;

            case NetworkLine.CastStart:
            {
                Understood++;
                MoveFrom(fields, NetworkLine.Hex(fields, 2), NetworkLine.CastSourcePosField, at);
                return new GameEvent
                {
                    Kind = EventKind.CastStart,
                    At = at,
                    Id = NetworkLine.Hex(fields, 4),
                    Source = _book.Resolve(NetworkLine.Hex(fields, 2), NetworkLine.Text(fields, 3)),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 6), NetworkLine.Text(fields, 7)),
                    Duration = NetworkLine.Number(fields, 8),
                };
            }

            case NetworkLine.Ability:
            case NetworkLine.AreaAbility:
            {
                Understood++;
                var index = fields.Length > NetworkLine.AbilityTargetIndexField
                    ? NetworkLine.Decimal(fields, NetworkLine.AbilityTargetIndexField)
                    : 0;

                MoveFrom(fields, NetworkLine.Hex(fields, 2), NetworkLine.AbilitySourcePosField, at);
                MoveFrom(fields, NetworkLine.Hex(fields, 6), NetworkLine.AbilityTargetPosField, at);

                return new GameEvent
                {
                    Kind = EventKind.AbilityHit,
                    At = at,
                    Id = NetworkLine.Hex(fields, 4),
                    Source = _book.Resolve(NetworkLine.Hex(fields, 2), NetworkLine.Text(fields, 3)),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 6), NetworkLine.Text(fields, 7)),
                    FirstTarget = index == 0,
                };
            }

            case NetworkLine.StatusAdd:
            {
                Understood++;

                if (NetworkLine.Hex(fields, 2) == NetworkLine.LoopVfxStatus)
                    return new GameEvent
                    {
                        Kind = EventKind.StatusLoopVfx,
                        At = at,
                        Id = NetworkLine.Hex(fields, 9),
                        Target = _book.Resolve(NetworkLine.Hex(fields, 7), NetworkLine.Text(fields, 8)),
                    };

                return new GameEvent
                {
                    Kind = EventKind.StatusGain,
                    At = at,
                    Id = NetworkLine.Hex(fields, 2),
                    Duration = NetworkLine.Number(fields, 4),
                    Source = _book.Resolve(NetworkLine.Hex(fields, 5), NetworkLine.Text(fields, 6)),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 7), NetworkLine.Text(fields, 8)),
                    Stacks = (byte)NetworkLine.Hex(fields, 9),
                };
            }

            case NetworkLine.StatusRemove:
            {
                Understood++;
                return new GameEvent
                {
                    Kind = EventKind.StatusLose,
                    At = at,
                    Id = NetworkLine.Hex(fields, 2),
                    Source = _book.Resolve(NetworkLine.Hex(fields, 5), NetworkLine.Text(fields, 6)),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 7), NetworkLine.Text(fields, 8)),
                    Stacks = (byte)NetworkLine.Hex(fields, 9),
                };
            }

            case NetworkLine.HeadMarker:
            {
                Understood++;
                return new GameEvent
                {
                    Kind = EventKind.HeadMarker,
                    At = at,
                    Id = NetworkLine.Hex(fields, 6),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 2), NetworkLine.Text(fields, 3)),
                };
            }

            case NetworkLine.Tether:
            {
                Understood++;
                return new GameEvent
                {
                    Kind = EventKind.Tether,
                    At = at,
                    Id = NetworkLine.Hex(fields, 8),
                    Source = _book.Resolve(NetworkLine.Hex(fields, 2), NetworkLine.Text(fields, 3)),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 4), NetworkLine.Text(fields, 5)),
                };
            }

            case NetworkLine.ActorControlExtra:
            {
                Understood++;
                return new GameEvent
                {
                    Kind = EventKind.ActorControl,
                    At = at,
                    Id = NetworkLine.Hex(fields, 3),
                    Target = _book.Resolve(NetworkLine.Hex(fields, 2), ""),
                    Arg1 = NetworkLine.Hex(fields, 4),
                    Arg2 = NetworkLine.Hex(fields, 5),
                    Arg3 = NetworkLine.Hex(fields, 6),
                    Arg4 = NetworkLine.Hex(fields, 7),
                };
            }

            case NetworkLine.ActorSetPos:
            {
                var id = NetworkLine.Hex(fields, 2);
                var pos = new Position(
                    (float)NetworkLine.Number(fields, 6),
                    (float)NetworkLine.Number(fields, 7),
                    (float)NetworkLine.Number(fields, 8));
                var heading = (float)NetworkLine.Number(fields, 3);

                _book.Move(id, pos, heading);
                NotePlaced(id, at);
                var actor = _book.Find(id);
                if (actor is null) return null;

                Understood++;
                return new GameEvent { Kind = EventKind.ActorMoved, At = at, Target = actor };
            }

            case NetworkLine.StatusEffects:
            {
                MoveFrom(fields, NetworkLine.Hex(fields, 2), NetworkLine.StatusEffectsPosField, at);
                return null;
            }

            case NetworkLine.ActorMove:
            {
                if (!ReadsPositions) return null;
                Shift(
                    NetworkLine.Hex(fields, 2),
                    new Position(
                        (float)NetworkLine.Number(fields, 6),
                        (float)NetworkLine.Number(fields, 7),
                        (float)NetworkLine.Number(fields, 8)),
                    (float)NetworkLine.Number(fields, 3));
                return null;
            }

            case NetworkLine.CombatantMemory:
            {
                if (ReadsPositions) Remember(fields);
                return null;
            }

            default:
                return null;
        }
    }

    private void MoveFrom(string[] fields, uint id, int field, double at)
    {
        if (!ReadsPositions || id == 0) return;
        if (!NetworkLine.HasRun(fields, field, 4)) return;

        if (JustPlaced(id, at))
        {
            Ignored++;
            return;
        }

        Shift(
            id,
            new Position(
                (float)NetworkLine.Number(fields, field),
                (float)NetworkLine.Number(fields, field + 1),
                (float)NetworkLine.Number(fields, field + 2)),
            (float)NetworkLine.Number(fields, field + 3));
    }

    private void Shift(uint id, Position pos, float heading)
    {
        if (_book.Find(id) is null) return;
        _book.Move(id, pos, heading);
        Moves++;
    }

    private void Remember(string[] fields)
    {
        if (NetworkLine.Text(fields, 2) is not ("Change" or "Add")) return;

        var id = NetworkLine.Hex(fields, 3);
        if (id == 0) return;
        var actor = _book.Find(id);
        if (actor is null) return;

        var pos = actor.Pos;
        var heading = actor.Heading;
        var touched = false;

        for (var i = NetworkLine.MemoryPairsField; i + 1 < fields.Length; i += 2)
        {
            var value = (float)NetworkLine.Number(fields, i + 1);
            switch (fields[i])
            {
                case "PosX": pos = pos with { X = value }; touched = true; break;
                case "PosY": pos = pos with { Y = value }; touched = true; break;
                case "PosZ": pos = pos with { Z = value }; touched = true; break;
                case "Heading": heading = value; touched = true; break;
            }
        }

        if (!touched) return;
        _book.Move(id, pos, heading);
        Moves++;
    }
}
