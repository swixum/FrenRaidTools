namespace FrenRaidTools.Engine;

public sealed class ParserActorBook
{
    public const int MaxActors = 4096;

    private readonly Dictionary<uint, Actor> _known = [];
    private readonly Queue<uint> _order = new();

    public uint YouId { get; set; }

    public string YouName { get; set; } = "";

    public int Count => _known.Count;

    public int Forgotten { get; private set; }

    public void Learn(string[] fields)
    {
        var id = NetworkLine.Hex(fields, 2);
        if (id == 0) return;

        var actor = new Actor
        {
            ObjectId = id,
            Name = NetworkLine.Text(fields, 3),
            NameId = NetworkLine.Decimal(fields, 9),
            BaseId = NetworkLine.Decimal(fields, 10),
            IsPlayer = NetworkLine.IsPlayerId(id),
            IsYou = IsYou(id, NetworkLine.Text(fields, 3)),
            Job = Jobs.Name(NetworkLine.Hex(fields, 4)),
            Pos = new Position(
                (float)NetworkLine.Number(fields, 17),
                (float)NetworkLine.Number(fields, 18),
                (float)NetworkLine.Number(fields, 19)),
            Heading = (float)NetworkLine.Number(fields, 20),
        };

        Put(actor);
    }

    public void KnowYou(uint id, string name, string job = "")
    {
        if (id == 0) return;

        var moved = id != YouId || !string.Equals(name, YouName, StringComparison.Ordinal);
        var held = Find(id);

        if (!moved && held is { IsYou: true } && (job.Length == 0 || held.Job.Length > 0)) return;

        YouId = id;
        YouName = name;

        Put(held is null
            ? new Actor { ObjectId = id, Name = name, IsPlayer = true, IsYou = true, Job = job }
            : held with
            {
                Name = name.Length > 0 ? name : held.Name,
                Job = held.Job.Length > 0 ? held.Job : job,
                IsPlayer = true,
                IsYou = true,
            });

        if (!moved) return;

        foreach (var (known, actor) in _known)
        {
            var mine = IsYou(known, actor.Name);
            if (actor.IsYou != mine) _known[known] = actor with { IsYou = mine };
        }
    }

    public void Forget(uint id) => _known.Remove(id);

    public void Identify(uint id, uint baseId)
    {
        if (!_known.TryGetValue(id, out var actor) || actor.BaseId == baseId) return;
        _known[id] = actor with { BaseId = baseId };
    }

    public void Move(uint id, Position pos, float heading)
    {
        if (!_known.TryGetValue(id, out var actor)) return;
        _known[id] = actor with { Pos = pos, Heading = heading };
    }

    public Actor Resolve(uint id, string name)
    {
        if (_known.TryGetValue(id, out var known))
        {
            if (name.Length > 0 && known.Name != name)
            {
                known = known with { Name = name };
                _known[id] = known;
            }
            return known;
        }

        var actor = new Actor
        {
            ObjectId = id,
            Name = name,
            IsPlayer = NetworkLine.IsPlayerId(id),
            IsYou = IsYou(id, name),
        };

        Put(actor);
        return actor;
    }

    public Actor? Find(uint id) => _known.GetValueOrDefault(id);

    public IEnumerable<Actor> Players => _known.Values.Where(a => a.IsPlayer);

    public IEnumerable<Actor> Npcs => _known.Values.Where(a => !a.IsPlayer);

    public Actor? You => YouId != 0 ? Find(YouId) : _known.Values.FirstOrDefault(a => a.IsYou);

    public void Note(Actor actor)
    {
        if (actor.ObjectId == 0) return;
        Put(actor with { IsYou = actor.IsYou || IsYou(actor.ObjectId, actor.Name) });
    }

    public void Clear()
    {
        _known.Clear();
        _order.Clear();
    }

    private bool IsYou(uint id, string name) =>
        (YouId != 0 && id == YouId) ||
        (YouName.Length > 0 && name.Length > 0 &&
         string.Equals(name, YouName, StringComparison.Ordinal));

    private void Put(Actor actor)
    {
        var fresh = !_known.ContainsKey(actor.ObjectId);

        _known[actor.ObjectId] = actor;

        if (!fresh) return;

        _order.Enqueue(actor.ObjectId);
        Trim();
    }

    private void Trim()
    {
        var spins = _order.Count;

        while (_order.Count > MaxActors)
        {
            var oldest = _order.Dequeue();

            if (!_known.TryGetValue(oldest, out var held)) continue;

            if (spins-- > 0 && (held.IsPlayer || held.IsYou))
            {
                _order.Enqueue(oldest);
                continue;
            }

            _known.Remove(oldest);
            Forgotten++;
        }
    }
}

public static class Jobs
{
    private static readonly string[] ByIndex =
    [
        "", "GLA", "PGL", "MRD", "LNC", "ARC", "CNJ", "THM",
        "CRP", "BSM", "ARM", "GSM", "LTW", "WVR", "ALC", "CUL",
        "MIN", "BTN", "FSH", "PLD", "MNK", "WAR", "DRG", "BRD",
        "WHM", "BLM", "ACN", "SMN", "SCH", "ROG", "NIN", "MCH",
        "DRK", "AST", "SAM", "RDM", "BLU", "GNB", "DNC", "RPR",
        "SGE", "VPR", "PCT",
    ];

    public static string Name(uint job) => job < ByIndex.Length ? ByIndex[job] : "";
}
