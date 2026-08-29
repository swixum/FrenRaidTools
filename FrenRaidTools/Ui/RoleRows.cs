using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

internal static class RoleRows
{
    public static bool Row(Roster roster, int slot, string id, List<int> duplicates,
        string you, SpotVerdict? verdict, List<PartyMember> glance)
    {
        var name = roster.Players[slot];
        var isYou = name.Length > 0 && string.Equals(name, you, StringComparison.OrdinalIgnoreCase);
        var duplicate = duplicates.Contains(slot);

        var hint = "";
        if (duplicate) hint = "This name is in two spots";
        else if (verdict is { Check: SpotCheck.NearMiss } close) hint = $"Did you mean {close.Suggestion}?";
        else if (verdict is { Check: SpotCheck.Absent }) hint = "Not in this party";

        var tag = hint.Length > 0 ? "" : isYou ? "you" : Tag(slot);

        var (icon, iconColor) = verdict?.Check switch
        {
            SpotCheck.Confirmed => (FontAwesomeIcon.Check, Theme.Good),
            SpotCheck.NearMiss => (FontAwesomeIcon.ExclamationTriangle, Theme.Warn),
            SpotCheck.Absent => (FontAwesomeIcon.Times, Theme.Danger),
            _ => (FontAwesomeIcon.None, 0u),
        };

        var fixing = verdict is { Check: SpotCheck.NearMiss };

        var nameWidth = Theme.S(206f);
        var gap = Theme.S(5f);
        var gripWidth = Theme.S(13f);
        var total = gripWidth + nameWidth + ImGui.GetFrameHeight() + Widgets.ButtonWidth("x")
                    + gap * 3f;
        if (fixing) total += Widgets.ButtonWidth("Fix") + gap;

        var top = ImGui.GetCursorPos();
        var corner = ImGui.GetCursorScreenPos();
        var span = new Vector2(ImGui.GetContentRegionAvail().X,
            Widgets.RowHeightFor(hint.Length > 0));

        ImGui.InvisibleButton("##drop" + id, span);
        ImGui.SetItemAllowOverlap();
        DragParty.Offer(new PartyMember(name, roster.Jobs[slot]), slot, id);
        var took = DragParty.TakeOn(roster, slot, out var hovering);
        ImGui.SetCursorPos(top);

        Widgets.RowBegin(Slots.Names[slot], hint, total, id: id,
            icon: icon, iconColor: iconColor,
            edgeColor: JobLook.SlotColor(slot), tag: tag,
            hintColor: duplicate ? Theme.Danger : verdict?.Check switch
            {
                SpotCheck.NearMiss => Theme.Warn,
                SpotCheck.Absent => Theme.Danger,
                _ => 0u,
            });

        var changed = took;

        Grip(roster, slot, id, name, gripWidth);
        ImGui.SameLine(0, gap);
        ImGui.SetNextItemWidth(nameWidth);

        var color = duplicate ? Theme.Danger
            : isYou ? Theme.Accent
            : name.Length > 0 ? Theme.TextBright
            : Theme.Muted;

        ImGui.PushStyleColor(ImGuiCol.Text, color);
        var buffer = name;
        if (ImGui.InputTextWithHint("##name", "Empty", ref buffer, 48))
        {
            roster.Set(slot, buffer, "");
            changed = true;
        }
        ImGui.PopStyleColor();

        ImGui.SameLine(0, gap);
        if (ImGui.ArrowButton("##pick", ImGuiDir.Down)) ImGui.OpenPopup("##party");
        Widgets.Tip("Pick from your party");
        changed |= Picker(roster, slot);

        ImGui.SameLine(0, gap);
        if (Widgets.DangerButton("x"))
        {
            roster.Set(slot, "", "");
            changed = true;
        }
        Widgets.Tip("Empty this spot");

        if (fixing && verdict is { } miss)
        {
            ImGui.SameLine(0, gap);
            if (Widgets.AccentButton("Fix"))
            {
                var member = glance.FirstOrDefault(m =>
                    string.Equals(m.Name, miss.Suggestion, StringComparison.OrdinalIgnoreCase));
                roster.Set(slot, miss.Suggestion, member.Job ?? "");
                changed = true;
            }
            Widgets.Tip($"Set this spot to {miss.Suggestion}");
        }

        Widgets.RowEnd();

        if (hovering) Glow(corner, span);
        else if (DragParty.LiftedFrom(id)) Lifted(corner, span);

        return changed;
    }

    private static void Glow(Vector2 corner, Vector2 span)
    {
        var beat = Motion.Pulse(ImGui.GetTime(), 0.12f, 0.30f);
        var round = Theme.S(4f);
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(corner, corner + span, Theme.Fade(Theme.Accent, beat * 0.6f), round);
        dl.AddRect(corner, corner + span, Theme.Fade(Theme.Accent, 0.6f + beat), round,
            ImDrawFlags.None, Theme.S(1.6f));
    }

    private static void Lifted(Vector2 corner, Vector2 span)
    {
        var round = Theme.S(4f);
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(corner, corner + span, Theme.Fade(Theme.Accent, 0.06f), round);
        dl.AddRect(corner, corner + span, Theme.Fade(Theme.Accent, 0.4f), round,
            ImDrawFlags.None, Theme.S(1f));
    }

    private static void Grip(Roster roster, int slot, string id, string name, float width)
    {
        var height = ImGui.GetFrameHeight();
        var at = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##grip" + id, new Vector2(width, height));

        var live = name.Length > 0;
        var hovered = live && ImGui.IsItemHovered();
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        if (live)
        {
            DragParty.Offer(new PartyMember(name, roster.Jobs[slot]), slot, id);
            Widgets.Tip("Drag to move this name");
        }

        var dot = Theme.S(1.6f);
        var stepX = Theme.S(5f);
        var stepY = Theme.S(5f);
        var color = !live ? Theme.Fade(Theme.Muted, 0.25f)
            : hovered || DragParty.LiftedFrom(id) ? Theme.Accent
            : Theme.Muted;

        var left = at.X + (width - stepX) * 0.5f;
        var topY = at.Y + height * 0.5f - stepY;
        var dl = ImGui.GetWindowDrawList();

        for (var row = 0; row < 3; row++)
        for (var col = 0; col < 2; col++)
            dl.AddCircleFilled(new Vector2(left + col * stepX, topY + row * stepY), dot, color);
    }

    private static string Tag(int slot) => slot switch
    {
        2 => "Pure",
        3 => "Shield",
        6 => "Phys",
        7 => "Caster",
        _ => "",
    };

    private static bool Picker(Roster roster, int slot)
    {
        if (!ImGui.BeginPopup("##party")) return false;

        var members = Party.Read();

        if (members.Count == 0)
        {
            ImGui.TextColored(Theme.V(Theme.Muted), "No party found.");
            ImGui.EndPopup();
            return false;
        }

        var picked = false;

        foreach (var member in members
                     .OrderBy(m => JobKinds.Rank(m.Kind))
                     .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
            picked |= Pick(roster, slot, member);

        ImGui.EndPopup();
        return picked;
    }

    private static bool Pick(Roster roster, int slot, PartyMember member)
    {
        var label = member.Job.Length > 0 ? $"{member.Job}   {member.Name}" : member.Name;

        ImGui.PushStyleColor(ImGuiCol.Text, JobLook.Color(member.Job));
        var hit = ImGui.Selectable(label);
        ImGui.PopStyleColor();

        if (hit) roster.Set(slot, member.Name, member.Job);
        return hit;
    }
}
