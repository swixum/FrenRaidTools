using System.Numerics;
using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private readonly GroupBar _groups = new("roles");

    private List<PartyMember> PartyGlance() => RosterGlance.Members(_now);

    private SpotVerdict[]? Verdicts() => RosterGlance.Verdicts(C, _now);

    private void DrawRoles()
    {
        var shown = C.Roles;

        PageHeader("Roles", shown.Complete ? "full party" : $"{shown.Filled} of {Slots.Count} set");

        Widgets.PageNote("Who is MT, M1, R2 when a call names a spot.");

        DrawSetupBar();

        var roster = C.Roles;
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

    private void DrawSetupBar()
    {
        Widgets.SectionHeader("Group");
        _groups.Draw(C, _now);
    }

    private void DrawRoleRows(Roster roster)
    {
        Widgets.SectionHeader("Party");

        var found = _rolePool.Draw(C, _plugin.Now);
        if (found.Length > 0) Board.Note(found);

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

        if (DragParty.Settle(roster)) Touch();
    }

    private readonly RolePool _rolePool = new RolePool("roles");

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
