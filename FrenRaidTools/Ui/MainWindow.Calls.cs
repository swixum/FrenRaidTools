using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private enum CallFilter
    {
        All,
        On,
        Off,
    }

    private CallFilter _filter = CallFilter.All;
    private readonly HashSet<string> _openEditors = [];

    private const int NoPhase = 0;

    private void DrawCalls()
    {
        var boss = FightPlans.ByKey(C.PlanFight) ?? FightPlans.First;

        var catalog = Fight.Owners.Only(boss.Key, Board.Catalog).ToList();

        DrawBackToCategory();

        PageHeader(boss.FullName,
            catalog.Count == 0 ? "no calls loaded" : $"{catalog.Count} calls, {boss.Expansion}");

        if (catalog.Count == 0)
        {
            Widgets.EmptyState("No calls loaded", "Phases appear with a fight");
            return;
        }

        DrawCallToolbar();

        var matches = catalog.Where(Matches).ToList();

        if (_search.Length > 0 && matches.Count == 0)
        {
            Widgets.EmptyState("Nothing matched", "Try a shorter word.");
            return;
        }

        if (_search.Length > 0) Widgets.SectionHeader($"{matches.Count} found");

        DrawPhaseTabs(boss, catalog, matches);
    }

    private void DrawPhaseTabs(PlannedFight boss, IReadOnlyList<CatalogEntry> catalog,
        List<CatalogEntry> matches)
    {
        var phases = catalog.Select(e => e.Call.Phase).Distinct()
            .OrderBy(p => p == NoPhase ? int.MaxValue : p).ToList();

        if (phases.Count <= 1)
        {
            DrawMechanics(matches);
            return;
        }

        if (!ImGui.BeginTabBar("##phases",
                ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.NoCloseWithMiddleMouseButton))
            return;

        foreach (var phase in phases)
        {
            var all = catalog.Where(e => e.Call.Phase == phase).ToList();
            var on = all.Count(e => !C.MutedCalls.Contains(e.Key));
            var name = Fight.PhaseNameFor(boss.Key, phase);
            var shown = matches.Where(e => e.Call.Phase == phase).ToList();

            var label = _search.Length > 0
                ? $"{name}  {shown.Count}###tab{phase}"
                : on == all.Count
                    ? $"{name}  {all.Count}###tab{phase}"
                    : $"{name}  {on}/{all.Count}###tab{phase}";

            if (!ImGui.BeginTabItem(label)) continue;

            if (shown.Count == 0)
                Widgets.EmptyState(
                    _search.Length > 0 ? "Nothing matched in this phase" : "Nothing here",
                    _search.Length > 0 ? "The other tabs show their own counts." : "The filter is hiding them all.");
            else
                DrawMechanics(shown);

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private void DrawMechanics(List<CatalogEntry> entries)
    {
        if (entries.Count == 0)
        {
            Widgets.EmptyState("Nothing here", "The filter is hiding them all.");
            return;
        }

        foreach (var mechanic in entries.Select(MechanicOf).Distinct())
        {
            var inside = entries.Where(e => MechanicOf(e) == mechanic).ToList();
            var on = inside.Count(e => !C.MutedCalls.Contains(e.Key));
            var badge = on == inside.Count ? $"{inside.Count}" : $"{on}/{inside.Count}";

            Fold(mechanic, mechanic, badge, on == inside.Count ? Theme.Muted : Theme.Warn,
                _foldedMechanics, openByDefault: true,
                () => { foreach (var entry in inside) DrawCallRow(entry, mechanic); },
                forceOpen: _search.Length > 0);
        }
    }

    private string MechanicOf(CatalogEntry entry) => _plugin.Fight.FoldFor(entry);

    private readonly HashSet<string> _foldedMechanics = [];

    private void DrawCallToolbar()
    {
        var boss = FightPlans.ByKey(C.PlanFight) ?? FightPlans.First;

        ImGui.SetNextItemWidth(Theme.S(210f));
        var search = _search;
        if (ImGui.InputTextWithHint("##callsearch", "Search calls", ref search, 64))
            _search = search;

        ImGui.SameLine(0, Theme.S(10f));

        Widgets.SegmentBegin();
        if (Widgets.Segment("All", _filter == CallFilter.All)) _filter = CallFilter.All;
        ImGui.SameLine();
        if (Widgets.Segment("On", _filter == CallFilter.On)) _filter = CallFilter.On;
        ImGui.SameLine();
        if (Widgets.Segment("Off", _filter == CallFilter.Off)) _filter = CallFilter.Off;
        Widgets.SegmentEnd();

        ImGui.SameLine(0, Theme.S(10f));
        if (Widgets.SmallGhost("All on"))
        {
            foreach (var entry in Board.Catalog) C.MutedCalls.Remove(entry.Key);
            Touch();
        }

        ImGui.SameLine(0, Theme.S(5f));
        if (Widgets.SmallDanger("All off"))
        {
            foreach (var entry in Board.Catalog) C.MutedCalls.Add(entry.Key);
            Touch();
        }

        var edited = EditedCount();
        if (edited > 0)
        {
            ImGui.SameLine(0, Theme.S(5f));
            if (Widgets.SmallDanger($"Undo {edited} edits"))
            {
                foreach (var entry in Board.Catalog) C.DropEdit(entry.Key);
                Touch();
            }
            Widgets.Tip($"Put every {boss.FullName} call back");
        }

        if (_search.Length > 0)
        {
            ImGui.SameLine(0, Theme.S(5f));
            if (Widgets.SmallGhost("Clear")) _search = "";
        }

        ImGui.Spacing();
    }

    private int EditedCount()
    {
        var n = 0;
        foreach (var entry in Board.Catalog)
            if (C.EditFor(entry.Key) is not null) n++;
        return n;
    }

    private bool Matches(CatalogEntry entry)
    {
        if (_filter == CallFilter.On && C.MutedCalls.Contains(entry.Key)) return false;
        if (_filter == CallFilter.Off && !C.MutedCalls.Contains(entry.Key)) return false;
        if (_search.Length == 0) return true;

        return entry.Call.Description.Contains(_search, StringComparison.OrdinalIgnoreCase)
               || entry.Call.Text.Contains(_search, StringComparison.OrdinalIgnoreCase)
               || entry.Call.Speech.Contains(_search, StringComparison.OrdinalIgnoreCase)
               || CallText.Says(entry.Call.Speech, entry.Call.Text)
                   .Contains(_search, StringComparison.OrdinalIgnoreCase)
               || CallText.Head(entry.Call, MechanicOf(entry)).Name
                   .Contains(_search, StringComparison.OrdinalIgnoreCase)
               || entry.Group.Contains(_search, StringComparison.OrdinalIgnoreCase);
    }

    private void DrawCallRow(CatalogEntry entry, string mechanic = "")
    {
        var call = entry.Call;
        var muted = C.MutedCalls.Contains(entry.Key);
        var edit = C.EditFor(entry.Key);
        var open = _openEditors.Contains(entry.Key);

        var speech = edit is { Speech.Length: > 0 } ? edit.Speech : call.Speech;
        var text = edit is { Text.Length: > 0 } ? edit.Text : call.Text;

        var says = CallText.Says(speech, text);
        var screen = CallText.Screen(speech, text);
        var hint = screen.Length == 0 ? says : $"{says}      on screen: {screen}";
        var (name, tags) = CallText.Head(call, mechanic);

        if (CallText.RepeatsTheMechanic(name, mechanic))
        {
            name = says;
            hint = screen;
        }

        var editWidth = MathF.Max(Widgets.ButtonWidth("Close"), Widgets.ButtonWidth("Edit"));
        var testWidth = Widgets.ButtonWidth("Test");
        var gap = Theme.S(5f);
        var total = editWidth + testWidth + ImGui.GetFrameHeight() + gap * 2f;

        var planned = _plugin.Fight.Plan?.Starred(call) ?? call.FromPlan;

        Widgets.RowBegin(name, hint, total, id: entry.Key,
            icon: planned ? FontAwesomeIcon.Star : FontAwesomeIcon.None,
            iconColor: muted ? Theme.Muted : Theme.Accent,
            edgeColor: muted ? 0u : edit is not null ? Theme.Warn : Theme.Accent,
            tag: tags, hintColor: muted ? Theme.Muted : Theme.Said);

        if (Widgets.GhostButton(open ? "Close" : "Edit", new Vector2(editWidth, 0)))
        {
            if (open) _openEditors.Remove(entry.Key);
            else _openEditors.Add(entry.Key);
        }
        Widgets.Tip("Change the wording");

        ImGui.SameLine(0, gap);
        if (Widgets.GhostButton("Test", new Vector2(testWidth, 0))) Board.Test(call);
        Widgets.Tip("Fire it now");

        ImGui.SameLine(0, gap);
        var on = !muted;
        if (Widgets.Check("##on" + entry.Key, ref on))
        {
            Board.SetMuted(entry.Key, !on);
            Touch();
        }

        Widgets.RowEnd();

        if (open) DrawCallEditor(entry);
    }

    private void DrawCallEditor(CatalogEntry entry)
    {
        var call = entry.Call;
        var slot = C.EditSlot(entry.Key);
        var width = MathF.Max(Theme.S(200f), ImGui.GetContentRegionAvail().X - Theme.S(150f));

        Widgets.RowBegin("Says", "", width, sub: true, id: entry.Key + "#say");
        var speech = slot.Speech;
        if (ImGui.InputTextWithHint("##say", call.Speech, ref speech, 200))
        {
            slot.Speech = speech;
            Clean(entry.Key);
            Touch();
        }
        Widgets.Tip($"Out loud. Written as: {call.Speech}");
        Widgets.RowEnd();

        Widgets.RowBegin("On screen", "", width, sub: true, id: entry.Key + "#text");
        var text = slot.Text;
        if (ImGui.InputTextWithHint("##text", call.Text, ref text, 200))
        {
            slot.Text = text;
            Clean(entry.Key);
            Touch();
        }
        Widgets.Tip($"On the overlay. Written as: {call.Text}");
        Widgets.RowEnd();

        if (call.NeedsParams)
            Widgets.RowNote("Keep the {braces}. They swap for names.", Theme.Warn);

        foreach (var line in CallNotes.Lines(call.Notes))
            Widgets.RowNoteWrap(line, Theme.Muted);

        var resetWidth = Widgets.ButtonWidth("Put it back", "Copy the wording");
        Widgets.RowBegin("", "", resetWidth, sub: true, id: entry.Key + "#acts");

        var changed = C.EditFor(entry.Key) is not null;
        if (!changed) ImGui.BeginDisabled();
        if (Widgets.DangerButton("Put it back"))
        {
            C.DropEdit(entry.Key);
            Touch();
        }
        if (!changed) ImGui.EndDisabled();

        ImGui.SameLine(0, Theme.S(5f));
        if (Widgets.GhostButton("Copy the wording"))
        {
            slot.Speech = call.Speech;
            slot.Text = call.Text;
            Touch();
        }

        Widgets.RowEnd();
    }

    private void Clean(string key)
    {
        if (C.CallEdits.TryGetValue(key, out var edit) && !edit.Any) C.DropEdit(key);
    }
}
