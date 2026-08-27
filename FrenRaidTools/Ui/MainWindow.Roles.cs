using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private string _renaming = "";
    private int _renameIndex = -1;

    private List<PartyMember> PartyGlance() => RosterGlance.Members(_now);

    private SpotVerdict[]? Verdicts() => RosterGlance.Verdicts(C, _now);

    private void DrawRoles()
    {
        var roster = C.Roles;

        PageHeader("Roles", roster.Complete ? "full party" : $"{roster.Filled} of 8 set");

        Widgets.PageNote("Who is MT, M1, R2 when a call names a spot. Job does not matter.");

        DrawSetupBar(roster);
        DrawRoleRows(roster);
        DrawRoleActions(roster);
    }

    private void DrawSetupBar(Roster roster)
    {
        Widgets.SectionHeader("Group");
        Widgets.ListBegin();

        if (_renameIndex == C.ActiveSetup)
        {
            Widgets.RowBegin("Name", "Enter to keep it", Theme.S(290f));
            ImGui.SetNextItemWidth(Theme.S(210f));
            var buffer = _renaming;
            var done = ImGui.InputText("##rename", ref buffer, 40, ImGuiInputTextFlags.EnterReturnsTrue);
            _renaming = buffer;

            ImGui.SameLine(0, Theme.S(6f));
            if (Widgets.AccentButton("Save") || done)
            {
                var name = _renaming.Trim();
                if (name.Length > 0) roster.Name = name;
                _renameIndex = -1;
                Touch();
            }
            Widgets.RowEnd();
        }
        else
        {
            var names = new string[C.Setups.Count];
            for (var i = 0; i < C.Setups.Count; i++)
            {
                var label = C.Setups[i].Name ?? "";
                names[i] = label.Length > 0 ? label : $"Group {i + 1}";
            }

            var index = C.ActiveSetup;
            Widgets.RowBegin("Group", "Swap between statics", Theme.S(290f));
            ImGui.SetNextItemWidth(Theme.S(190f));
            if (ImGui.Combo("##setup", ref index, names, names.Length))
            {
                C.ActiveSetup = Math.Clamp(index, 0, C.Setups.Count - 1);
                Touch();
            }

            ImGui.SameLine(0, Theme.S(6f));
            if (Widgets.GhostButton("Rename"))
            {
                _renaming = roster.Name ?? "";
                _renameIndex = C.ActiveSetup;
            }
            Widgets.RowEnd();
        }

        Widgets.RowBegin("Saved groups", $"{C.Setups.Count} on file", Theme.S(290f));

        if (Widgets.GhostButton("New"))
        {
            C.Setups.Add(new Roster { Name = $"Group {C.Setups.Count + 1}" });
            C.ActiveSetup = C.Setups.Count - 1;
            _renameIndex = -1;
            Touch();
        }

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Copy"))
        {
            var copy = roster.Copy();
            copy.Name = (roster.Name ?? "Group") + " copy";
            C.Setups.Add(copy);
            C.ActiveSetup = C.Setups.Count - 1;
            _renameIndex = -1;
            Touch();
        }

        ImGui.SameLine(0, Theme.S(6f));
        var canDelete = C.Setups.Count > 1;
        if (!canDelete) ImGui.BeginDisabled();
        if (Widgets.DangerButton("Delete"))
        {
            C.Setups.RemoveAt(C.ActiveSetup);
            C.ActiveSetup = Math.Clamp(C.ActiveSetup, 0, C.Setups.Count - 1);
            _renameIndex = -1;
            Touch();
        }
        if (!canDelete) ImGui.EndDisabled();
        if (!canDelete) Widgets.Tip("The last group stays.");

        Widgets.RowEnd();
        Widgets.ListEnd();
    }

    private void DrawRoleRows(Roster roster)
    {
        Widgets.SectionHeader("Party");

        var duplicates = roster.Duplicates();
        var you = Party.YouName();
        var verdicts = Verdicts();

        if (duplicates.Count > 0)
            Banner(Theme.Danger, "Same name in two spots. A call would pick the first one.");

        Widgets.ListBegin();
        for (var slot = 0; slot < Slots.Count; slot++)
            DrawRoleRow(roster, slot, duplicates, you, verdicts?[slot]);
        Widgets.ListEnd();
    }

    private void DrawRoleRow(
        Roster roster, int slot, List<int> duplicates, string you, SpotVerdict? verdict)
    {
        var name = roster.Players[slot];
        var isYou = name.Length > 0 && string.Equals(name, you, StringComparison.OrdinalIgnoreCase);

        var hint = Slots.Hint(slot);
        if (duplicates.Contains(slot)) hint = "This name is in two spots";
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
        var arrowWidth = ImGui.GetFrameHeight();
        var clearWidth = Widgets.ButtonWidth("x");
        var gap = Theme.S(5f);
        var total = nameWidth + arrowWidth + clearWidth + gap * 2f;
        if (fixing) total += Widgets.ButtonWidth("Fix") + gap;

        Widgets.RowBegin(Slots.Names[slot], hint, total, id: "slot" + slot,
            icon: icon, iconColor: iconColor,
            edgeColor: JobLook.SlotColor(slot),
            hintColor: verdict?.Check switch
            {
                SpotCheck.NearMiss => Theme.Warn,
                SpotCheck.Absent => Theme.Danger,
                _ => 0u,
            });

        ImGui.SetNextItemWidth(nameWidth);

        var color = duplicates.Contains(slot) ? Theme.Danger
            : isYou ? Theme.Accent
            : name.Length > 0 ? Theme.TextBright
            : Theme.Muted;

        ImGui.PushStyleColor(ImGuiCol.Text, color);
        var buffer = name;
        if (ImGui.InputTextWithHint("##name" + slot, "Empty", ref buffer, 48))
        {
            roster.Set(slot, buffer, "");
            Touch();
        }
        ImGui.PopStyleColor();

        ImGui.SameLine(0, gap);
        if (ImGui.ArrowButton("##pick" + slot, ImGuiDir.Down)) ImGui.OpenPopup("##party" + slot);
        Widgets.Tip("Pick from your party, in a duty or out in the world.");
        DrawPartyPicker(roster, slot);

        ImGui.SameLine(0, gap);
        if (Widgets.DangerButton("x"))
        {
            roster.Set(slot, "", "");
            Touch();
        }
        Widgets.Tip("Empty this spot.");

        if (fixing && verdict is { } miss)
        {
            ImGui.SameLine(0, gap);
            if (Widgets.AccentButton("Fix"))
            {
                var member = PartyGlance().FirstOrDefault(m =>
                    string.Equals(m.Name, miss.Suggestion, StringComparison.OrdinalIgnoreCase));
                roster.Set(slot, miss.Suggestion, member.Job ?? "");
                Touch();
            }
            Widgets.Tip($"Set this spot to {miss.Suggestion}.");
        }

        Widgets.RowEnd();
    }

    private void DrawPartyPicker(Roster roster, int slot)
    {
        if (!ImGui.BeginPopup("##party" + slot)) return;

        var members = Party.Read();

        if (members.Count == 0)
        {
            ImGui.TextColored(Theme.V(Theme.Muted), "No party found.");
            ImGui.EndPopup();
            return;
        }

        foreach (var member in members
                     .OrderBy(m => JobKinds.Rank(m.Kind))
                     .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
            PickName(roster, slot, member);

        ImGui.EndPopup();
    }

    private void PickName(Roster roster, int slot, PartyMember member)
    {
        var label = member.Job.Length > 0 ? $"{member.Job}   {member.Name}" : member.Name;

        ImGui.PushStyleColor(ImGuiCol.Text, JobLook.Color(member.Job));
        var picked = ImGui.Selectable(label);
        ImGui.PopStyleColor();

        if (!picked) return;

        roster.Set(slot, member.Name, member.Job);
        Touch();
    }

    private void DrawRoleActions(Roster roster)
    {
        ImGui.Spacing();

        if (Widgets.AccentButton("Pull party"))
        {
            var members = Party.Read();
            if (members.Count == 0)
            {
                Board.Note("No party to read.");
            }
            else
            {
                var placed = roster.Fill(members, keepExisting: false);
                Board.Note(placed > 0 ? $"Set {placed} spots from your party." : "Nobody matched a spot.");
                Touch();
            }
        }
        Widgets.Tip("Wipe the board and lay out the party you are in.");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Fill blanks"))
        {
            var placed = roster.Fill(Party.Read(), keepExisting: true);
            Board.Note(placed > 0 ? $"Set {placed} spots." : "Nothing left to fill.");
            Touch();
        }
        Widgets.Tip("Keep what is there, fill the empty spots.");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Swap")) ImGui.OpenPopup("##swaps");
        Widgets.Tip("Flip a pair.");
        DrawSwapPopup(roster);

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.DangerButton("Clear all"))
        {
            roster.Clear();
            Touch();
        }

        ImGui.Spacing();
        Widgets.ListBegin();

        var refill = C.FillRolesOnJoin;
        if (Widgets.RowCheckClick("Refill on zone in", "Fill blank spots when you load into the fight", ref refill))
        {
            C.FillRolesOnJoin = refill;
            Touch();
        }

        var ask = C.AskOnEntry;
        if (Widgets.RowCheckClick("Check in on entry", "Show this list when you zone into the fight, so a wrong name gets caught before the pull", ref ask))
        {
            C.AskOnEntry = ask;
            Touch();
        }

        Widgets.ListEnd();
    }

    private void DrawSwapPopup(Roster roster)
    {
        if (!ImGui.BeginPopup("##swaps")) return;

        for (var pair = 0; pair < Slots.Count / 2; pair++)
        {
            var a = pair * 2;
            var b = a + 1;
            if (!ImGui.Selectable($"{Slots.Names[a]} and {Slots.Names[b]}")) continue;

            roster.Swap(a, b);
            Touch();
        }

        ImGui.EndPopup();
    }

    private static void Banner(uint color, string text)
    {
        var padding = new Vector2(Theme.S(10f), Theme.S(6f));
        var size = new Vector2(ImGui.GetContentRegionAvail().X,
            ImGui.GetTextLineHeight() + padding.Y * 2f);
        var p = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(p, p + size, Theme.Wash(color, 0x22), Theme.S(6f));
        dl.AddRectFilled(p, p + new Vector2(Theme.S(3f), size.Y), color, 2f);
        dl.AddText(p + new Vector2(Widgets.RowPad, padding.Y), color, text);

        ImGui.Dummy(size);
        ImGui.Dummy(new Vector2(0, Theme.S(2f)));
    }
}
