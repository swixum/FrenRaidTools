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

        var hint = Slots.Hint(slot);
        if (duplicate) hint = "This name is in two spots";
        else if (verdict is { Check: SpotCheck.NearMiss } close) hint = $"Did you mean {close.Suggestion}?";
        else if (verdict is { Check: SpotCheck.Absent }) hint = "Not in this party";
        else if (isYou) hint = $"{Slots.Hint(slot)}, you";

        var (icon, iconColor) = verdict?.Check switch
        {
            SpotCheck.Confirmed => (FontAwesomeIcon.Check, Theme.Good),
            SpotCheck.NearMiss => (FontAwesomeIcon.ExclamationTriangle, Theme.Warn),
            SpotCheck.Absent => (FontAwesomeIcon.Times, Theme.Danger),
            _ => (FontAwesomeIcon.None, 0u),
        };

        var fixing = verdict is { Check: SpotCheck.NearMiss };

        var nameWidth = Theme.S(254f);
        var gap = Theme.S(5f);
        var total = nameWidth + ImGui.GetFrameHeight() + Widgets.ButtonWidth("x") + gap * 2f;
        if (fixing) total += Widgets.ButtonWidth("Fix") + gap;

        Widgets.RowBegin(Slots.Names[slot], hint, total, id: id,
            icon: icon, iconColor: iconColor,
            edgeColor: JobLook.SlotColor(slot),
            hintColor: duplicate ? Theme.Danger : verdict?.Check switch
            {
                SpotCheck.NearMiss => Theme.Warn,
                SpotCheck.Absent => Theme.Danger,
                _ => 0u,
            });

        var changed = false;

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
        return changed;
    }

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
