using System.Numerics;
using Dalamud.Bindings.ImGui;
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

        Widgets.PageNote("Who is MT, M1, R2 when a call names a spot.");

        DrawSetupBar(roster);
        DrawJobSpots();
        DrawRoleRows(roster);
        DrawRoleActions(roster);
    }

    private bool _jobSpotsOpen;

    private void DrawJobSpots()
    {
        var spots = C.JobSpots;

        ImGui.Dummy(new Vector2(0, Theme.S(4f)));

        var open = Widgets.FoldBegin("jobspots", "Job and Role Priority", JobSpotRows.Badge(spots),
            spots.Any ? Theme.Accent : 0u, ref _jobSpotsOpen);

        if (!open)
        {
            Widgets.FoldEnd();
            return;
        }

        Widgets.RowNoteWrap(JobSpotRows.Note);

        if (JobSpotRows.List(spots, "js")) Touch();

        if (JobSpotRows.ResetRow(spots, "jobspotsreset"))
        {
            Board.Note("Priority back to auto.");
            Touch();
        }

        Widgets.FoldEnd();
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
            Widgets.RowBegin("Group", "", Theme.S(290f));
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

        if (Widgets.GhostButton("Add"))
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
        if (!canDelete) Widgets.Tip("The last group stays");

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
            Widgets.Banner(Theme.Danger, "Same name in two spots. Calls take the first.");

        var glance = PartyGlance();

        Widgets.ListBegin();
        for (var slot = 0; slot < Slots.Count; slot++)
            if (RoleRows.Row(roster, slot, "slot" + slot, duplicates, you, verdicts?[slot], glance))
                Touch();
        Widgets.ListEnd();
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
                var placed = roster.Fill(members, keepExisting: false, C.JobSpots, Party.YouName());
                Board.Note(placed > 0
                    ? $"Set {TextLines.Spots(placed)} from your party."
                    : "Nobody matched a spot.");
                Touch();
            }
        }
        Widgets.Tip("Clear, then seat your party");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Fill blanks"))
        {
            var placed = roster.Fill(Party.Read(), keepExisting: true, C.JobSpots, Party.YouName());
            Board.Note(placed > 0 ? $"Set {TextLines.Spots(placed)}." : "Nothing left to fill.");
            Touch();
        }
        Widgets.Tip("Fill the empty spots only");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Swap")) ImGui.OpenPopup("##swaps");
        Widgets.Tip("Flip a pair");
        DrawSwapPopup(roster);

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.DangerButton("Clear all")) ImGui.OpenPopup(ClearPopup);
        Widgets.Tip("Empty all eight spots");

        if (Widgets.ConfirmPopup(ClearPopup, "Empty all eight spots?", "Clear all", "Keep them"))
        {
            roster.Clear();
            Board.Note("Cleared all eight spots.");
            Touch();
        }

        ImGui.Spacing();
        Widgets.ListBegin();

        var refill = C.FillRolesOnJoin;
        if (Widgets.RowCheckClick("Refill on zone in", "Fill blanks when you load in", ref refill))
        {
            C.FillRolesOnJoin = refill;
            Touch();
        }

        var ask = C.AskOnEntry;
        if (Widgets.RowCheckClick("Check in on entry", "Party list on zone in", ref ask))
        {
            C.AskOnEntry = ask;
            Touch();
        }

        Widgets.ListEnd();
    }

    private const string ClearPopup = "##rolesclear";

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
}
