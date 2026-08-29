using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private void DrawHome()
    {
        PageHeader("Fren Raid Tools", _plugin.Version);

        DrawHomeTiles();
        ImGui.Spacing();

        DrawReplay();
        DrawHomeActions();
    }

    private static string Where()
    {
        if (Game.InReplay) return "Dancing Mad, replay";
        if (!Game.InTheFight) return "out of the fight";
        return Game.InFight ? "Dancing Mad, pulled" : "Dancing Mad, standing by";
    }

    private void DrawHomeTiles()
    {
        var gap = ImGui.GetStyle().ItemSpacing.X;
        var width = (ImGui.GetContentRegionAvail().X - gap) * 0.5f;
        var height = ImGui.GetTextLineHeightWithSpacing() * 3f + Theme.S(9f) * 2f;

        var here = Game.InTheFight;
        var loaded = Board.Catalog.Count;

        if (Tile("##zone", width, height, FontAwesomeIcon.MapMarkerAlt, Theme.Accent,
                "This zone",
                here ? "Dancing Mad" : Game.ZoneName(),
                here ? Theme.TextBright : Theme.Muted,
                here ? $"{Where()}, {loaded} calls ready" : "Nothing is called here"))
            Show(Nav.Strats);
        Widgets.Tip(here ? "Open the strats" : "Nothing is called here");

        ImGui.SameLine();

        var seat = SeatSync.SeatFor(C);
        var filled = C.Roles.Filled;

        if (Tile("##spot", width, height, FontAwesomeIcon.Users,
                seat.Length > 0 ? Theme.Good : Theme.Warn,
                "Your spot",
                seat.Length > 0 ? seat : "Not in a spot",
                seat.Length > 0 ? Theme.Good : Theme.Warn,
                filled == 0 ? "No names on the Roles page" : $"{filled} of 8 names set"))
            Show(Nav.Roles);
        Widgets.Tip("Open the party list");

        var on = new List<string>();
        var off = new List<string>();
        (C.OverlayOn ? on : off).Add("Overlay");
        (C.TtsOn ? on : off).Add("Voice");

        if (Tile("##screen", width, height, FontAwesomeIcon.Desktop,
                off.Count == 0 ? Theme.Accent : Theme.Warn,
                "On screen",
                on.Count == 0 ? "Nothing is on" : string.Join(", ", on),
                on.Count == 0 ? Theme.Warn : Theme.TextBright,
                off.Count == 0 ? "Both are on" : "Off: " + string.Join(", ", off)))
            Show(C.OverlayOn ? Nav.Overlay : Nav.Voice);
        Widgets.Tip("Open the overlay");

        ImGui.SameLine();

        var trouble = Trouble();

        if (Tile("##look", width, height,
                trouble.Count == 0 ? FontAwesomeIcon.CheckCircle : FontAwesomeIcon.ExclamationTriangle,
                trouble.Count == 0 ? Theme.Good : Theme.Warn,
                "Needs a look",
                trouble.Count == 0 ? "All good" : trouble[0].Text,
                trouble.Count == 0 ? Theme.Good : Theme.Warn,
                trouble.Count > 1 ? string.Join(", ", trouble.Skip(1).Select(t => t.Text)) : ""))
            Show(trouble.Count == 0 ? Nav.Diagnostics : trouble[0].Page);
        Widgets.Tip(trouble.Count == 0 ? "Nothing needs attention" : "Go and fix the first one");
    }

    private readonly record struct Snag(string Text, Nav Page);

    private List<Snag> Trouble()
    {
        var found = new List<Snag>();

        if (_plugin.Runtime.Blind is { } blind) found.Add(new Snag(blind, Nav.Parser));
        if (Seatless() is { } seatless) found.Add(new Snag(seatless, Nav.Roles));
        if (Misspelled() is { } wrong) found.Add(new Snag(wrong, Nav.Roles));
        if (Board.Catalog.Count == 0) found.Add(new Snag("No calls loaded", Nav.Strats));
        if (!C.OverlayOn && !C.TtsOn) found.Add(new Snag("Overlay and voice are both off", Nav.Overlay));

        foreach (var fault in _plugin.Runtime.Faults.Take(FaultsShown))
            found.Add(new Snag(fault, Nav.Diagnostics));

        return found;
    }

    private static bool Tile(string id, float width, float height, FontAwesomeIcon icon,
        uint iconColor, string label, string line, uint lineColor, string sub)
    {
        var p = ImGui.GetCursorScreenPos();
        var size = new Vector2(width, height);

        var clicked = ImGui.InvisibleButton(id, size);
        var hot = ImGui.IsItemHovered();
        if (hot) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        var dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(p, p + size, hot ? Theme.RowHover : Theme.PanelBg, Theme.S(8f));
        dl.AddRect(p, p + size, hot ? Theme.Accent : Theme.Border, Theme.S(8f));

        var pad = Theme.S(9f);
        var lineH = ImGui.GetTextLineHeightWithSpacing();
        var room = width - pad * 2f;
        var iconW = 0f;

        using (Service.PluginInterface.UiBuilder.IconFontHandle.Push())
        {
            var glyph = icon.ToIconString();
            iconW = ImGui.CalcTextSize(glyph).X;
            dl.AddText(p + new Vector2(width - pad - iconW, pad), iconColor, glyph);
        }

        dl.AddText(p + new Vector2(pad, pad), Theme.Muted,
            Widgets.Elide(label, room - iconW - Theme.S(6f)));
        dl.AddText(p + new Vector2(pad, pad + lineH), lineColor, Widgets.Elide(line, room));

        if (sub.Length > 0)
            dl.AddText(p + new Vector2(pad, pad + lineH * 2f), Theme.Muted, Widgets.Elide(sub, room));

        return clicked;
    }

    private void DrawHomeActions()
    {
        Widgets.ListBegin();

        if (Widgets.RowDoor("Open the party list", "Set who is in which spot")) _plugin.Roles.Open();

        if (Widgets.RowDoor("Pick your strats", "One choice a mechanic")) Show(Nav.Strats);

        if (Widgets.RowDoor("Edit the calls", "Change the words or mute one")) Show(Nav.Calls);

        Widgets.ListEnd();

        ImGui.Spacing();

        if (Widgets.AccentButton("Test call")) _plugin.FireSample();
        Widgets.Tip("Sample call on the overlay");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Clear screen")) Board.Clear();
    }

    private void DrawReplay()
    {
        if (!Game.InReplay) return;

        Widgets.SectionHeader("Replay");
        Widgets.ListBegin();
        Widgets.RowNoteWrap($"Reading the game. {_plugin.Runtime.ClientDetail}");
        Widgets.RowNoteWrap(_plugin.Runtime.VfxDetail);
        Widgets.RowNoteWrap(_plugin.Runtime.EffectDetail);
        Widgets.RowNoteWrap(_plugin.Runtime.ControlDetail);
        Widgets.ListEnd();
        ImGui.Spacing();
    }

    private const int FaultsShown = 5;

    private string? Seatless()
    {
        if (!Game.InTheFight) return null;

        var plan = C.PlanFor(PickedFight.Key);
        if (plan.Seat.Length > 0) return null;

        return C.Roles.Filled == 0
            ? "No names on the Roles page"
            : "Your name is not in a spot";
    }

    private string? Misspelled()
    {
        var verdicts = Verdicts();
        if (verdicts is null) return null;

        var off = verdicts.Count(v => v.Check is SpotCheck.NearMiss or SpotCheck.Absent);
        return off switch
        {
            0 => null,
            1 => "A name is not in this party",
            _ => $"{off} names are not in this party",
        };
    }

    private void DrawDiagnosticsPage()
    {
        var diag = _plugin.Diag;

        PageHeader("Diagnostics", diag.On ? "writing" : "off");

        Widgets.SectionHeader("Recording");
        Widgets.ListBegin();

        var recording = diag.On;
        if (Widgets.RowCheckClick("Write a diagnostics file", "", ref recording, id: "diaglog"))
        {
            if (recording) diag.Start();
            else diag.Stop();

            C.DiagOn = recording;
            Touch();
        }

        var armed = C.DiagInReplay;
        if (Widgets.RowCheckClick("Start on a duty replay", "", ref armed, id: "diagreplay"))
        {
            C.DiagInReplay = armed;
            Touch();
        }

        if (diag.On) Widgets.RowNoteWrap(diag.Detail);

        if (diag.On && Game.InReplay)
            Widgets.RowNoteWrap(
                "A replay shows the client feed. The log feed needs a live pull.");

        Widgets.ListEnd();

        var trouble = Trouble();

        Widgets.SectionHeader("Needs a look");
        Widgets.ListBegin();

        if (trouble.Count == 0) Widgets.RowNote("Nothing", Theme.Good);
        else
            foreach (var snag in trouble)
                Widgets.RowNoteWrap(snag.Text, Theme.Warn);

        Widgets.ListEnd();

        Widgets.SectionHeader("Zone");
        Widgets.ListBegin();
        Widgets.RowValue("Where you are", "", Where(), Theme.Muted);
        Widgets.RowValue("Territory", "", Game.Zone.ToString(), Theme.Muted);
        Widgets.RowValue("Calls loaded", "", Board.Catalog.Count.ToString(), Theme.Muted);
        Widgets.RowValue("Fired this session", "", $"{Board.Fired} fired, {Board.Skipped} muted",
            Theme.Muted);
        Widgets.ListEnd();
    }
}
