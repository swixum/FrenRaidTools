using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Feed;

public sealed unsafe class VfxLink : IDisposable
{
    public const string ActorVfxCreateSignature =
        "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? "
        + "48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8";

    public const int MaxPathBytes = 256;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint ActorVfxCreateDelegate(
        byte* path, nint caster, nint target, float a4, byte a5, ushort a6, byte a7);

    private readonly ParserActorBook _book;
    private readonly Action<GameEvent> _publish;
    private readonly Func<double> _now;
    private Hook<ActorVfxCreateDelegate>? _hook;

    public VfxLink(ParserActorBook book, Action<GameEvent> publish, Func<double> now)
    {
        _book = book;
        _publish = publish;
        _now = now;
    }

    public bool Attached => _hook is not null;

    public string? Fault { get; private set; }

    public long Seen { get; private set; }

    public long Told { get; private set; }

    public string Detail =>
        Fault is not null ? Fault
        : !Attached ? "Not started."
        : Told > 0 || Marks > 0
            ? $"Reading {Told} boss tells and {Marks} head markers of {Seen} seen."
            : $"Watching, {Seen} seen, no tell or marker yet.";

    public void Start()
    {
        if (_hook is not null || Fault is not null) return;

        try
        {
            _hook = Service.GameInterop.HookFromSignature<ActorVfxCreateDelegate>(
                ActorVfxCreateSignature, Created);
            _hook.Enable();
        }
        catch (Exception ex)
        {
            Fault = $"The phase 4 boss tells cannot be read: {ex.Message}";
            Service.Log.Error(ex, "Could not hook the vfx create function.");
        }
    }

    private nint Created(
        byte* path, nint caster, nint target, float a4, byte a5, ushort a6, byte a7)
    {
        var result = _hook!.Original(path, caster, target, a4, a5, a6, a7);

        try
        {
            Take(path, caster);
        }
        catch (Exception ex)
        {
            Fault ??= $"A boss tell could not be read: {ex.Message}";
            Service.Log.Error(ex, "Reading a vfx spawn threw.");
        }

        return result;
    }

    private void Take(byte* path, nint caster)
    {
        if (path is null) return;

        Seen++;

        if (!Starts(path, HeadMarkers.Lead)) return;

        var text = Marshal.PtrToStringUTF8((nint)path, MaxPathBytes);
        if (text is null) return;

        Marker(text, caster);
    }

    public long Marks { get; private set; }

    private void Marker(string path, nint caster)
    {
        var id = HeadMarkers.For(path);
        if (id is null) return;

        var owner = (GameObject*)caster;
        if (owner is null) return;

        Marks++;
        _publish(new GameEvent
        {
            Kind = EventKind.HeadMarker,
            Id = id.Value,
            At = _now(),
            Target = _book.Find(owner->EntityId)
                     ?? new Actor { ObjectId = owner->EntityId, BaseId = owner->BaseId },
        });
    }

    private static bool Starts(byte* path, string lead)
    {
        for (var i = 0; i < lead.Length; i++)
            if (path[i] != (byte)lead[i]) return false;

        return true;
    }

    public void Dispose()
    {
        _hook?.Disable();
        _hook?.Dispose();
        _hook = null;
    }
}
