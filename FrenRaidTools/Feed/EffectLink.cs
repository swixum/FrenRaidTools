using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Feed;

public sealed unsafe class EffectLink : IDisposable
{
    private readonly ParserActorBook _book;
    private readonly Action<GameEvent> _publish;
    private readonly Func<double> _now;

    private Hook<ActionEffectHandler.Delegates.Receive>? _hook;

    public EffectLink(ParserActorBook book, Action<GameEvent> publish, Func<double> now)
    {
        _book = book;
        _publish = publish;
        _now = now;
    }

    public bool On { get; set; }

    public bool Attached => _hook is not null;

    public string? Fault { get; private set; }

    public long Seen { get; private set; }

    public string Detail =>
        Fault is not null ? Fault
        : !Attached ? "Not started."
        : $"Reading ability hits, {Seen} seen.";

    public void Start()
    {
        if (_hook is not null || Fault is not null) return;

        try
        {
            _hook = Service.GameInterop.HookFromAddress<ActionEffectHandler.Delegates.Receive>(
                ActionEffectHandler.MemberFunctionPointers.Receive, Received);
            _hook.Enable();
        }
        catch (Exception ex)
        {
            Fault = $"Ability hits cannot be read: {ex.Message}";
            Service.Log.Error(ex, "Could not hook the action effect handler.");
        }
    }

    private void Received(
        uint casterId, Character* caster, System.Numerics.Vector3* pos,
        ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targets)
    {
        _hook!.Original(casterId, caster, pos, header, effects, targets);

        if (!On || header is null) return;

        try
        {
            Take(casterId, header, targets);
        }
        catch (Exception ex)
        {
            Fault ??= $"An ability hit could not be read: {ex.Message}";
            Service.Log.Error(ex, "Reading an action effect threw.");
        }
    }

    private void Take(uint casterId, ActionEffectHandler.Header* header, GameObjectId* targets)
    {
        Seen++;

        var count = header->NumTargets;
        var source = _book.Find(casterId);
        var at = _now();

        if (count == 0 || targets is null)
        {
            _publish(Hit(header->ActionId, source, null, at, first: true));
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var id = (uint)targets[i].ObjectId;
            _publish(Hit(header->ActionId, source, _book.Find(id), at, i == 0));
        }
    }

    private static GameEvent Hit(uint id, Actor? source, Actor? target, double at, bool first) => new()
    {
        Kind = EventKind.AbilityHit,
        Id = id,
        At = at,
        Source = source,
        Target = target,
        FirstTarget = first,
    };

    public void Dispose()
    {
        _hook?.Disable();
        _hook?.Dispose();
        _hook = null;
    }
}
