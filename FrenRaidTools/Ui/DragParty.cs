using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

internal static class DragParty
{
    public const string Payload = "frtmember";

    public const int FromPool = -1;

    private static readonly byte[] Marker = [1];

    public static PartyMember Held { get; private set; }

    public static int From { get; private set; } = FromPool;

    private static string _fromKey = "";

    private static bool _wasLive;

    private static int _liveFrame = -10;

    private static bool _landed;

    public static unsafe bool Active => ImGui.GetDragDropPayload().Handle is not null;

    public static bool LiftedFrom(string key) =>
        Active && key.Length > 0 && string.Equals(_fromKey, key, StringComparison.Ordinal);

    public static void Offer(PartyMember member) => Offer(member, FromPool, "");

    public static void Offer(PartyMember member, int from, string key)
    {
        if (member.Name.Length == 0) return;
        if (!ImGui.BeginDragDropSource()) return;

        Held = member;
        From = from;
        _fromKey = key;
        ImGui.SetDragDropPayload(Payload, Marker);

        if (member.Job.Length > 0)
        {
            ImGui.TextColored(Theme.V(JobLook.Color(member.Job)), member.Job);
            ImGui.SameLine(0, Theme.S(6f));
        }

        ImGui.TextUnformatted(member.Name);
        ImGui.EndDragDropSource();
    }

    public static unsafe bool TakeOn(Roster roster, int slot, out bool hovering)
    {
        hovering = false;
        if (!ImGui.BeginDragDropTarget()) return false;

        hovering = From != slot;
        var payload = ImGui.AcceptDragDropPayload(Payload);
        var placed = hovering && payload.Handle is not null
                     && roster.Place(slot, Held.Name, Held.Job);

        if (payload.Handle is not null) _landed = true;

        ImGui.EndDragDropTarget();
        return placed;
    }

    public static unsafe bool DropOut(Roster roster, out bool hovering)
    {
        hovering = false;
        if (!ImGui.BeginDragDropTarget()) return false;

        hovering = From != FromPool;
        var payload = ImGui.AcceptDragDropPayload(Payload);
        var emptied = hovering && payload.Handle is not null;

        if (payload.Handle is not null) _landed = true;
        if (emptied) roster.Set(From, "", "");

        ImGui.EndDragDropTarget();
        return emptied;
    }

    public static bool Settle(Roster roster)
    {
        var frame = ImGui.GetFrameCount();

        if (Active)
        {
            _wasLive = true;
            _liveFrame = frame;
            return false;
        }

        if (!_wasLive)
        {
            _landed = false;
            return false;
        }

        _wasLive = false;

        var watched = frame - _liveFrame <= 1;
        var dropped = !ImGui.IsMouseDown(ImGuiMouseButton.Left);
        var loose = watched && dropped && !_landed && From != FromPool;
        var from = From;

        _landed = false;
        From = FromPool;
        _fromKey = "";

        if (!loose) return false;

        roster.Set(from, "", "");
        return true;
    }
}
