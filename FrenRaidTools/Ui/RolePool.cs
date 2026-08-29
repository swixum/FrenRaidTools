using System.Numerics;
using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

internal sealed class RolePool
{
    private readonly string _id;
    private List<PartyMember> _pool = [];
    private int _frame = -10;

    public RolePool(string id) => _id = id;

    public void Forget()
    {
        _pool = [];
        _frame = -10;
    }

    public string Draw(Configuration config, double now)
    {
        var frame = ImGui.GetFrameCount();
        var note = frame - _frame > 2 ? Look(quiet: true) : "";
        _frame = frame;

        if (Widgets.GhostButton("Find party")) note = Look(quiet: false);
        Widgets.Tip("Read your party again");

        var pulling = DragParty.Active && DragParty.From != DragParty.FromPool;

        ImGui.SameLine(0, Theme.S(8f));
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(pulling ? Theme.Accent : Theme.Muted),
            pulling ? "Drop here to empty the spot"
            : _pool.Count == 0 ? "No party found"
            : "Drag a name onto a spot");

        ImGui.Spacing();
        if (_pool.Count == 0) return note;

        var top = ImGui.GetCursorPos();
        var corner = ImGui.GetCursorScreenPos();
        var room = ImGui.GetContentRegionAvail().X;
        var tall = Reserved(room, config.Roles);

        ImGui.InvisibleButton("##out" + _id, new Vector2(room, tall));
        ImGui.SetItemAllowOverlap();
        if (DragParty.DropOut(config.Roles, out var hovering)) config.Save(now);
        ImGui.SetCursorPos(top);

        var used = 0f;
        var loose = 0;

        for (var i = 0; i < _pool.Count; i++)
        {
            var member = _pool[i];
            if (config.Roles.SlotOf(member.Name) >= 0) continue;

            var width = Widgets.MemberChipWidth(member.Job, member.Name, "");

            if (used > 0f && used + width > room)
            {
                used = 0f;
            }
            else if (used > 0f)
            {
                ImGui.SameLine(0, Theme.S(5f));
                used += Theme.S(5f);
            }

            Widgets.MemberChip(member.Job, member.Name, JobLook.Color(member.Job), "", _id + i);
            DragParty.Offer(member);
            used += width;
            loose++;
        }

        if (loose == 0 && !pulling)
            ImGui.TextColored(Theme.V(Theme.Muted), "Everyone is in a spot");

        ImGui.SetCursorPos(new Vector2(top.X, top.Y + tall));

        if (hovering) Glow(corner, new Vector2(room, tall));

        ImGui.Spacing();
        return note;
    }

    private static void Glow(Vector2 corner, Vector2 span)
    {
        var beat = Motion.Pulse(ImGui.GetTime(), 0.12f, 0.30f);
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(corner, corner + span, Theme.Fade(Theme.Accent, beat * 0.5f), Theme.S(6f));
        dl.AddRect(corner, corner + span, Theme.Fade(Theme.Accent, 0.6f + beat), Theme.S(6f),
            ImDrawFlags.None, Theme.S(1.6f));
    }

    private float Reserved(float room, Roster roles)
    {
        var gap = Theme.S(5f);
        var lines = 1;
        var used = 0f;

        foreach (var member in _pool)
        {
            if (roles.SlotOf(member.Name) >= 0) continue;

            var width = Widgets.MemberChipWidth(member.Job, member.Name, "");

            if (used > 0f && used + width > room)
            {
                lines++;
                used = width;
            }
            else
            {
                used += (used > 0f ? gap : 0f) + width;
            }
        }

        return lines * Widgets.MemberChipHeight()
               + (lines - 1) * ImGui.GetStyle().ItemSpacing.Y;
    }

    private string Look(bool quiet)
    {
        _pool = [.. Party.Read()
            .OrderBy(m => JobKinds.Rank(m.Kind))
            .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase)];

        if (quiet) return "";
        return _pool.Count == 0 ? "No party to read." : $"Found {_pool.Count}.";
    }
}
