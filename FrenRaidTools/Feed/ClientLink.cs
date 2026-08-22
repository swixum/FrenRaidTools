using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Feed;

public sealed class ClientLink
{
    public const int MaxTracked = 256;

    private readonly ParserActorBook _book;
    private readonly Action<GameEvent> _publish;

    private readonly Dictionary<uint, uint> _casting = [];
    private readonly Dictionary<uint, HashSet<ulong>> _held = [];
    private readonly HashSet<ulong> _open = [];
    private readonly List<ulong> _lost = [];
    private readonly Dictionary<uint, HashSet<uint>> _tethers = [];
    private readonly HashSet<uint> _alive = [];
    private readonly List<uint> _gone = [];

    public ClientLink(ParserActorBook book, Action<GameEvent> publish)
    {
        _book = book;
        _publish = publish;
    }

    public bool On { get; set; }

    public long Actors { get; private set; }

    public long Casts { get; private set; }

    public long Statuses { get; private set; }

    public long Tethers { get; private set; }

    public string Detail =>
        !On ? "Off."
        : Actors == 0 ? "Watching, nothing in range yet."
        : $"Reading {Actors} actors, {Casts} casts, {Statuses} status changes, {Tethers} tethers.";

    public void Clear()
    {
        _casting.Clear();
        _held.Clear();
        _tethers.Clear();
        _alive.Clear();
        Actors = 0;
        Casts = 0;
        Statuses = 0;
        Tethers = 0;
    }

    public void Tick(double now)
    {
        if (!On) return;

        var table = Service.ObjectTable;
        if (table is null) return;

        _alive.Clear();
        var seen = 0;

        foreach (var obj in table)
        {
            if (obj is null) continue;
            if (obj.ObjectKind is not (ObjectKind.Pc or ObjectKind.BattleNpc)) continue;

            var id = obj.EntityId;
            if (id == 0) continue;

            _alive.Add(id);
            seen++;

            var actor = Read(obj);
            _book.Note(actor);

            if (obj is IBattleChara fighter)
            {
                Cast(fighter, actor, now);
                Status(fighter, actor, now);
            }

            if (obj is ICharacter tethered) Tether(tethered, actor, now);

            if (seen >= MaxTracked) break;
        }

        Actors = seen;
        Drop(now);
    }

    private Actor Read(IGameObject obj)
    {
        var job = obj is ICharacter character ? Jobs.Name(character.ClassJob.RowId) : "";

        return new Actor
        {
            ObjectId = obj.EntityId,
            BaseId = obj.BaseId,
            Name = obj.Name.TextValue,
            IsPlayer = obj.ObjectKind == ObjectKind.Pc,
            IsYou = obj.EntityId == (Game.You?.EntityId ?? 0),
            Job = job,
            Pos = new Position(obj.Position.X, obj.Position.Z, obj.Position.Y),
            Heading = obj.Rotation,
        };
    }

    private void Cast(IBattleChara fighter, Actor actor, double now)
    {
        var was = _casting.GetValueOrDefault(actor.ObjectId);

        if (!fighter.IsCasting || fighter.CastActionId == 0)
        {
            if (was != 0) _casting.Remove(actor.ObjectId);
            return;
        }

        var id = fighter.CastActionId;
        if (was == id) return;

        _casting[actor.ObjectId] = id;
        Casts++;

        _publish(new GameEvent
        {
            Kind = EventKind.CastStart,
            Id = id,
            At = now,
            Duration = fighter.TotalCastTime,
            Source = actor,
            Target = Target(fighter.CastTargetObjectId),
        });
    }

    private static ulong Held(uint statusId, uint sourceId) =>
        ((ulong)statusId << 32) | sourceId;

    private static uint StatusOf(ulong held) => (uint)(held >> 32);

    private static uint SourceOf(ulong held) => (uint)held;

    private void Status(IBattleChara fighter, Actor actor, double now)
    {
        if (!_held.TryGetValue(actor.ObjectId, out var was))
            _held[actor.ObjectId] = was = [];

        _open.Clear();

        foreach (var status in fighter.StatusList)
        {
            if (status is null || status.StatusId == 0) continue;

            var loop = status.StatusId == NetworkLine.LoopVfxStatus;
            var key = loop
                ? Held(status.StatusId, status.Param)
                : Held(status.StatusId, status.SourceId);
            _open.Add(key);
            if (!was.Add(key)) continue;

            Statuses++;

            if (loop)
            {
                _publish(new GameEvent
                {
                    Kind = EventKind.StatusLoopVfx,
                    Id = status.Param,
                    At = now,
                    Target = actor,
                });
                continue;
            }

            _publish(new GameEvent
            {
                Kind = EventKind.StatusGain,
                Id = status.StatusId,
                At = now,
                Duration = status.RemainingTime,
                Target = actor,
                Source = Target(status.SourceId),
            });
        }

        _lost.Clear();
        foreach (var key in was)
            if (!_open.Contains(key)) _lost.Add(key);

        foreach (var key in _lost)
        {
            was.Remove(key);
            Statuses++;
            _publish(new GameEvent
            {
                Kind = EventKind.StatusLose,
                Id = StatusOf(key),
                At = now,
                Target = actor,
                Source = Target(SourceOf(key)),
            });
        }
    }

    private unsafe void Tether(ICharacter source, Actor actor, double now)
    {
        var raw = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)source.Address;
        if (raw is null) return;

        var live = _tethers.TryGetValue(actor.ObjectId, out var was) ? was : null;
        var open = new HashSet<uint>();

        var span = raw->Vfx.Tethers;
        for (var i = 0; i < span.Length; i++)
        {
            var tether = span[i];
            if (tether.Id == 0) continue;

            var target = (uint)tether.TargetId.ObjectId;
            if (target == 0) continue;

            open.Add(target);
            if (live is not null && live.Contains(target)) continue;

            Tethers++;
            _publish(new GameEvent
            {
                Kind = EventKind.Tether,
                Id = tether.Id,
                At = now,
                Source = actor,
                Target = _book.Find(target),
            });
        }

        if (open.Count == 0) _tethers.Remove(actor.ObjectId);
        else _tethers[actor.ObjectId] = open;
    }

    private void Drop(double now)
    {
        _gone.Clear();
        foreach (var id in _held.Keys)
            if (!_alive.Contains(id)) _gone.Add(id);

        foreach (var id in _gone)
        {
            if (_held.TryGetValue(id, out var was))
                foreach (var held in was)
                    _publish(new GameEvent
                    {
                        Kind = EventKind.StatusLose,
                        Id = StatusOf(held),
                        At = now,
                        Target = _book.Find(id),
                    });

            _held.Remove(id);
            _casting.Remove(id);
            _tethers.Remove(id);
        }
    }

    private Actor? Target(ulong id) => id is 0 or 0xE0000000 ? null : _book.Find((uint)id);
}
