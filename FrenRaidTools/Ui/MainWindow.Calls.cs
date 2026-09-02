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

    private enum CallGrouping
    {
        Steps,
        Flat,
    }

    private CallFilter _filter = CallFilter.All;
    private CallGrouping _grouping = CallGrouping.Steps;
    private int _shownPhase;
    private readonly HashSet<string> _openEditors = [];
    private readonly HashSet<string> _openSteps = [];

    private const int NoPhase = 0;

    private void DrawCalls()
    {
        var boss = FightPlans.ByKey(C.PlanFight) ?? FightPlans.First;

        var catalog = Fight.Owners.Only(boss.Key, Board.Shown).ToList();

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
        var standing = catalog.Where(e => e.Call.Fightwide).ToList();
        var phased = catalog.Where(e => !e.Call.Fightwide).ToList();

        var phases = phased.Select(e => e.Call.Phase).Distinct()
            .OrderBy(p => p == NoPhase ? int.MaxValue : p).ToList();

        if (phases.Count <= 1 && standing.Count == 0)
        {
            DrawMechanics(matches);
            return;
        }

        if (!ImGui.BeginTabBar("##phases",
                ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.NoCloseWithMiddleMouseButton))
            return;

        DrawStandingTab(standing, matches.Where(e => e.Call.Fightwide).ToList());

        foreach (var phase in phases)
        {
            var all = phased.Where(e => e.Call.Phase == phase).ToList();
            var on = all.Count(e => !C.MutedCalls.Contains(e.Key));
            var name = Fight.PhaseNameFor(boss.Key, phase);
            var shown = matches.Where(e => !e.Call.Fightwide && e.Call.Phase == phase).ToList();

            var label = _search.Length > 0
                ? $"{name}  {shown.Count}###tab{phase}"
                : on == all.Count
                    ? $"{name}  {all.Count}###tab{phase}"
                    : $"{name}  {on}/{all.Count}###tab{phase}";

            if (!ImGui.BeginTabItem(label)) continue;

            _shownPhase = phase;

            if (shown.Count == 0)
                Widgets.EmptyState(
                    _search.Length > 0 ? "Nothing matched in this phase" : "Nothing here",
                    _search.Length > 0 ? "The other tabs show their own counts." : "The filter is hiding them all.");
            else
            {
                if (_search.Length == 0) DrawVoice(phase, all);
                DrawMechanics(shown);
            }

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private const string StandingName = "Tanks and enrages";

    private const int StandingPhase = -1;

    private void DrawStandingTab(List<CatalogEntry> all, List<CatalogEntry> shown)
    {
        if (all.Count == 0) return;

        var on = all.Count(e => !C.MutedCalls.Contains(e.Key));

        var label = _search.Length > 0
            ? $"{StandingName}  {shown.Count}###tabstanding"
            : on == all.Count
                ? $"{StandingName}  {all.Count}###tabstanding"
                : $"{StandingName}  {on}/{all.Count}###tabstanding";

        if (!ImGui.BeginTabItem(label)) return;

        _shownPhase = StandingPhase;

        if (shown.Count == 0)
            Widgets.EmptyState(
                _search.Length > 0 ? "Nothing matched here" : "Nothing here",
                _search.Length > 0 ? "The other tabs show their own counts." : "The filter is hiding them all.");
        else
            DrawKinds(shown);

        ImGui.EndTabItem();
    }

    private void DrawKinds(List<CatalogEntry> entries)
    {
        foreach (var kind in entries.Select(e => e.Call.Step).Distinct())
        {
            var inside = entries.Where(e => e.Call.Step == kind).ToList();
            var on = inside.Count(e => !C.MutedCalls.Contains(e.Key));
            var badge = on == inside.Count ? $"{inside.Count}" : $"{on}/{inside.Count}";

            Fold("kind" + kind, kind, badge, on == inside.Count ? Theme.Muted : Theme.Warn,
                _foldedMechanics, openByDefault: false,
                () => { foreach (var entry in inside) DrawCallRow(entry, kind); },
                forceOpen: _search.Length > 0,
                controlWidth: ImGui.GetFrameHeight(),
                controls: () => AllOn("kind" + kind, inside));
        }
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
            var steps = Steps(inside).Count;

            var badge = on == inside.Count ? $"{inside.Count}" : $"{on}/{inside.Count}";
            if (steps > 1 && _grouping == CallGrouping.Steps)
                badge += steps == 1 ? ", 1 step" : $", {steps} steps";

            Fold(mechanic, mechanic, badge, on == inside.Count ? Theme.Muted : Theme.Warn,
                _foldedMechanics, openByDefault: false,
                () => DrawSteps(mechanic, inside),
                forceOpen: _search.Length > 0,
                controlWidth: ImGui.GetFrameHeight(),
                controls: () => AllOn(mechanic, inside));
        }
    }

    private static List<string> Steps(IReadOnlyList<CatalogEntry> entries) =>
        entries.Select(e => e.Call.Step).Where(s => s.Length > 0).Distinct().ToList();

    private void AllOn(string id, IReadOnlyList<CatalogEntry> entries)
    {
        var on = entries.All(e => !C.MutedCalls.Contains(e.Key));
        if (!Widgets.Check("##on" + id, ref on)) return;

        foreach (var entry in entries) Board.SetMuted(entry.Key, !on);
        Touch();
    }

    private void DrawVoice(int phase, List<CatalogEntry> shown)
    {
        if (CallVoices.ForPhase(phase) is not { } voice) return;

        var universe = voice.Universe(shown).ToList();
        if (universe.Count == 0) return;

        var picked = voice.Matching(universe, key => !C.MutedCalls.Contains(key));

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(Theme.Muted), voice.Label);
        ImGui.SameLine(0, Theme.S(9f));

        Widgets.SegmentBegin();

        for (var i = 0; i < voice.Choices.Count; i++)
        {
            var choice = voice.Choices[i];
            if (i > 0) ImGui.SameLine();

            if (!Widgets.Segment(choice.Label, choice == picked)) continue;

            foreach (var entry in universe)
                Board.SetMuted(entry.Key, !choice.Wants(entry.Call));

            Touch();
        }

        Widgets.SegmentEnd();

        ImGui.SameLine(0, Theme.S(9f));
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(Theme.Muted),
            $"{universe.Count(e => !C.MutedCalls.Contains(e.Key))}/{universe.Count} of the noisy ones on");

        ImGui.Spacing();
    }

    private void DrawSteps(string mechanic, List<CatalogEntry> inside)
    {
        if (_grouping == CallGrouping.Flat || Steps(inside).Count <= 1)
        {
            foreach (var entry in inside) DrawCallRow(entry, mechanic);
            return;
        }

        var index = 0;

        foreach (var step in inside.Select(e => e.Call.Step).Distinct())
        {
            var members = inside.Where(e => e.Call.Step == step).ToList();

            if (step.Length == 0)
            {
                foreach (var entry in members) DrawCallRow(entry, mechanic);
                continue;
            }

            DrawStep(mechanic, step, ++index, members);
        }
    }

    private void DrawStep(string mechanic, string step, int index, List<CatalogEntry> members)
    {
        var id = mechanic + " / " + step;
        var open = _search.Length > 0 || _openSteps.Contains(id);

        var on = members.Count(e => !C.MutedCalls.Contains(e.Key));
        var tag = on == members.Count ? $"{members.Count}" : $"{on}/{members.Count}";

        var testWidth = Widgets.ButtonWidth("Test");
        var gap = Theme.S(5f);

        Widgets.RowBegin($"{index}. {step}", "", testWidth + ImGui.GetFrameHeight() + gap,
            clickable: true,
            icon: open ? FontAwesomeIcon.CaretDown : FontAwesomeIcon.CaretRight,
            iconColor: open ? Theme.Accent : Theme.Muted,
            id: id, edgeColor: on == 0 ? 0u : Theme.Accent, tag: tag);

        var hit = Widgets.RowClicked;

        if (Widgets.GhostButton("Test", new Vector2(testWidth, 0))) Board.Test(members[0].Call);
        Widgets.Tip("Fire the first line");

        ImGui.SameLine(0, gap);
        AllOn(id, members);

        Widgets.RowEnd();

        if (hit && _search.Length == 0)
        {
            if (open) _openSteps.Remove(id);
            else _openSteps.Add(id);
            open = !open;
        }

        if (!open) return;

        foreach (var entry in members) DrawCallRow(entry, mechanic, sub: true);
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

        Widgets.SegmentBegin();
        if (Widgets.Segment("Steps", _grouping == CallGrouping.Steps)) _grouping = CallGrouping.Steps;
        ImGui.SameLine();
        if (Widgets.Segment("Flat", _grouping == CallGrouping.Flat)) _grouping = CallGrouping.Flat;
        Widgets.SegmentEnd();
        Widgets.Tip("Group calls by when they fire");

        ImGui.SameLine(0, Theme.S(10f));
        if (Widgets.SmallGhost("All on"))
        {
            foreach (var entry in Board.Shown) C.MutedCalls.Remove(entry.Key);
            Touch();
        }

        ImGui.SameLine(0, Theme.S(5f));
        if (Widgets.SmallDanger("All off"))
        {
            foreach (var entry in Board.Shown) C.MutedCalls.Add(entry.Key);
            Touch();
        }

        DrawPhaseSwitch(boss);

        var edited = EditedCount();
        if (edited > 0)
        {
            ImGui.SameLine(0, Theme.S(5f));
            if (Widgets.SmallDanger($"Undo {edited} edits"))
            {
                foreach (var entry in Board.Shown) C.DropEdit(entry.Key);
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

    private void DrawPhaseSwitch(PlannedFight boss)
    {
        if (_search.Length > 0 || _shownPhase == NoPhase) return;

        var standing = _shownPhase == StandingPhase;

        var inside = Fight.Owners.Only(boss.Key, Board.Shown)
            .Where(e => standing
                ? e.Call.Fightwide
                : !e.Call.Fightwide && e.Call.Phase == _shownPhase).ToList();
        if (inside.Count == 0) return;

        var name = standing ? StandingName : Fight.PhaseNameFor(boss.Key, _shownPhase);
        var off = inside.All(e => C.MutedCalls.Contains(e.Key));

        ImGui.SameLine(0, Theme.S(5f));

        if (off ? Widgets.SmallGhost($"{name} on") : Widgets.SmallDanger($"{name} off"))
        {
            foreach (var entry in inside) Board.SetMuted(entry.Key, !off);
            Touch();
        }

        Widgets.Tip(off ? $"Turn every {name} call back on" : $"Turn every {name} call off");
    }

    private int EditedCount()
    {
        var n = 0;
        foreach (var entry in Board.Shown)
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

    private void DrawCallRow(CatalogEntry entry, string mechanic = "", bool sub = false)
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

        Widgets.RowBegin(name, hint, total, sub: sub, id: entry.Key,
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
