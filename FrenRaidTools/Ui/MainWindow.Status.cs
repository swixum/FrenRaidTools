using System.Numerics;
using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private void DrawStatus()
    {
        PageHeader("Status", Where());

        DrawReplay();
        DrawQuickToggles();
        DrawRecent();
        DrawDiagnostics();
    }

    private static string Where()
    {
        if (Game.InReplay) return "Dancing Mad, replay";
        if (!Game.InTheFight) return "out of the fight";
        return Game.InFight ? "Dancing Mad, pulled" : "Dancing Mad, standing by";
    }

    private void DrawReplay()
    {
        if (!Game.InReplay) return;

        Widgets.SectionHeader("Replay");
        Widgets.ListBegin();
        Widgets.RowNoteWrap($"Reading the game itself. {_plugin.Runtime.ClientDetail}");
        Widgets.RowNoteWrap(_plugin.Runtime.VfxDetail);
        Widgets.RowNoteWrap(_plugin.Runtime.EffectDetail);
        Widgets.RowNoteWrap(_plugin.Runtime.ControlDetail);
        Widgets.ListEnd();
    }

    private void DrawQuickToggles()
    {
        Widgets.SectionHeader("Switches");
        Widgets.ListBegin();

        var only = C.OnlyInFight;
        if (Widgets.RowCheckClick("Only in Dancing Mad", "Stay quiet in every other zone", ref only))
        {
            C.OnlyInFight = only;
            Touch();
        }

        var overlay = C.OverlayOn;
        if (Widgets.RowCheckClick("Overlay", "Text on screen", ref overlay))
        {
            C.OverlayOn = overlay;
            Touch();
        }

        var tts = C.TtsOn;
        if (Widgets.RowCheckClick("Voice", "Calls read out loud", ref tts))
        {
            C.TtsOn = tts;
            Touch();
        }

        Widgets.ListEnd();
    }

    private const int FaultsShown = 5;

    private string? Seatless()
    {
        if (!Game.InTheFight) return null;

        var plan = C.PlanFor(PickedFight.Key);
        if (!plan.Enabled) return "Your spot is off, so no call tells you where to stand. Strats page.";

        if (plan.Seat.Length > 0) return null;

        return C.Roles.Filled == 0
            ? "Nobody is on the Roles page, so no call tells you where to stand."
            : "You are not in a spot on the Roles page, so no call tells you where to stand.";
    }

    private string? Misspelled()
    {
        var verdicts = Verdicts();
        if (verdicts is null) return null;

        var off = verdicts.Count(v => v.Check is SpotCheck.NearMiss or SpotCheck.Absent);
        return off switch
        {
            0 => null,
            1 => "A name on the Roles page is not in this party. Roles page.",
            _ => $"{off} names on the Roles page are not in this party. Roles page.",
        };
    }

    private void DrawDiagnostics()
    {
        var diag = _plugin.Diag;

        Widgets.SectionHeader("Diagnostics");
        Widgets.ListBegin();

        var recording = diag.On;
        if (Widgets.RowCheckClick("Write a diagnostics file",
                "Everything the fight sees and says, for working out a bad call",
                ref recording, id: "diaglog"))
        {
            if (recording) diag.Start();
            else diag.Stop();

            C.DiagOn = recording;
            Touch();
        }

        var armed = C.DiagInReplay;
        if (Widgets.RowCheckClick("Start it for a duty replay",
                "Turns on by itself when a replay starts, so the file has the opening",
                ref armed, id: "diagreplay"))
        {
            C.DiagInReplay = armed;
            Touch();
        }

        if (_plugin.Runtime.Blind is { } blind) Widgets.RowNoteWrap(blind, Theme.Danger);

        if (Seatless() is { } seatless) Widgets.RowNoteWrap(seatless, Theme.Warn);

        if (Misspelled() is { } wrong) Widgets.RowNoteWrap(wrong, Theme.Warn);

        foreach (var fault in _plugin.Runtime.Faults.Take(FaultsShown))
            Widgets.RowNoteWrap(fault, Theme.Warn);

        if (diag.On) Widgets.RowNoteWrap(diag.Detail);

        if (diag.On && Game.InReplay)
            Widgets.RowNoteWrap(
                "A replay reads the game directly, so this file shows the client feed. "
                + "The log feed only runs on a live pull.");

        Widgets.ListEnd();
    }

    private void DrawRecent()
    {
        var history = Board.History();

        Widgets.SectionHeader("Recent");

        if (history.Count == 0)
        {
            Widgets.ListBegin();
            Widgets.RowNote("Nothing yet. Hit Test up top to see one land.");
            Widgets.ListEnd();
            return;
        }

        if (Widgets.GhostButton("Clear log")) Board.ClearHistory();
        ImGui.SameLine(0, Theme.S(8f));
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(Theme.Muted), $"{Board.Fired} fired, {Board.Skipped} muted");

        ImGui.Spacing();
        Widgets.ListBegin();

        var shown = 0;
        for (var i = history.Count - 1; i >= 0 && shown < 25; i--, shown++)
        {
            var entry = history[i];
            var ago = _now - entry.At;
            var when = ago < 60 ? $"{ago:0}s ago" : $"{ago / 60:0}m ago";
            var said = entry.Text.Length > 0 ? entry.Text : entry.Description;

            Widgets.RowBegin(entry.Muted ? "muted" : said, LoggedUnder(entry), Theme.S(90f),
                id: "log" + i,
                edgeColor: entry.Muted ? Theme.Muted : entry.Test ? Theme.Warn : Theme.Accent,
                hintColor: Theme.Muted);
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Theme.V(Theme.Muted), when);
            Widgets.RowEnd();
        }

        Widgets.ListEnd();
    }

    private string LoggedUnder(CallLog entry)
    {
        var found = Board.Catalog.FirstOrDefault(e => e.Call.Description == entry.Description);
        var mechanic = found is null ? "" : MechanicOf(found);
        var name = found is null
            ? entry.Description
            : CallText.Head(found.Call, mechanic).Name;

        return mechanic.Length == 0 ? name : $"{mechanic}, {name}";
    }
}
