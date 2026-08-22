using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Network;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Feed;

public sealed unsafe class ControlLink : IDisposable
{
    private readonly ParserActorBook _book;
    private readonly Action<GameEvent> _publish;
    private readonly Func<double> _now;

    private Hook<PacketDispatcher.Delegates.HandleActorControlPacket>? _hook;

    public ControlLink(ParserActorBook book, Action<GameEvent> publish, Func<double> now)
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
        : $"Reading actor controls, {Seen} seen.";

    public void Start()
    {
        if (_hook is not null || Fault is not null) return;

        try
        {
            _hook = Service.GameInterop.HookFromAddress<PacketDispatcher.Delegates.HandleActorControlPacket>(
                PacketDispatcher.MemberFunctionPointers.HandleActorControlPacket, Received);
            _hook.Enable();
        }
        catch (Exception ex)
        {
            Fault = $"Actor controls cannot be read: {ex.Message}";
            Service.Log.Error(ex, "Could not hook the actor control packet handler.");
        }
    }

    private void Received(
        uint entityId, uint category,
        uint arg1, uint arg2, uint arg3, uint arg4,
        uint arg5, uint arg6, uint arg7, uint arg8,
        GameObjectId targetId, bool isRecorded)
    {
        _hook!.Original(
            entityId, category, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, targetId, isRecorded);

        if (!On || entityId == 0) return;

        try
        {
            Take(entityId, category, arg1, arg2, arg3, arg4);
        }
        catch (Exception ex)
        {
            Fault ??= $"An actor control could not be read: {ex.Message}";
            Service.Log.Error(ex, "Reading an actor control threw.");
        }
    }

    private void Take(uint entityId, uint category, uint arg1, uint arg2, uint arg3, uint arg4)
    {
        Seen++;

        var actor = _book.Resolve(entityId, "");

        if (Service.ObjectTable.SearchByEntityId(entityId) is { } obj)
        {
            _book.Move(entityId, new Position(obj.Position.X, obj.Position.Z, obj.Position.Y), obj.Rotation);
            actor = _book.Find(entityId) ?? actor;
        }

        _publish(new GameEvent
        {
            Kind = EventKind.ActorControl,
            Id = category,
            At = _now(),
            Target = actor,
            Arg1 = arg1,
            Arg2 = arg2,
            Arg3 = arg3,
            Arg4 = arg4,
        });
    }

    public void Dispose()
    {
        _hook?.Disable();
        _hook?.Dispose();
        _hook = null;
    }
}
