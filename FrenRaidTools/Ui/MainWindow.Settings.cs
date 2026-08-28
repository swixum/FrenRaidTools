using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private bool _overlayPreview = true;

    private static readonly string[] AlignNames = ["Left", "Center", "Right"];

    private void DrawOverlaySettings()
    {
        PageHeader("Overlay", C.OverlayLocked ? "locked" : "unlocked, drag it");

        DrawOverlayPlacement();

        Widgets.SectionHeader("On screen");
        Widgets.ListBegin();

        var on = C.OverlayOn;
        if (Widgets.RowCheckClick("Show overlay", "Calls on screen", ref on))
        {
            C.OverlayOn = on;
            Touch();
        }

        var background = C.OverlayBackground;
        if (Widgets.RowCheckClick("Backing box", "Panel behind the text", ref background))
        {
            C.OverlayBackground = background;
            Touch();
        }

        if (background)
        {
            var color = C.OverlayBackgroundColor;
            if (Widgets.RowColor("Box color", "", ref color, sub: true))
            {
                C.OverlayBackgroundColor = color;
                Touch();
            }
        }

        var text = C.OverlayTextColor;
        if (Widgets.RowColor("Text color", "", ref text))
        {
            C.OverlayTextColor = text;
            Touch();
        }

        var outline = C.OverlayOutline;
        if (Widgets.RowCheckClick("Outline", "Dark edge on the text", ref outline))
        {
            C.OverlayOutline = outline;
            Touch();
        }

        var icons = C.OverlayIcons;
        if (Widgets.RowCheckClick("Debuff icons", "Game icon on debuff calls", ref icons))
        {
            C.OverlayIcons = icons;
            Touch();
        }

        Widgets.ListEnd();

        Widgets.SectionHeader("Size and timing");
        Widgets.ListBegin();

        var px = C.OverlayFontPx;
        if (Widgets.RowDragInt("Text size", "Drawn at this size", ref px, 12, 72, "%dpx"))
        {
            C.OverlayFontPx = Fonts.Snap(px);
            Touch();
        }

        var lines = C.OverlayMaxLines;
        if (Widgets.RowDragInt("Lines", "Calls at once", ref lines, 1, 8))
        {
            C.OverlayMaxLines = lines;
            Touch();
        }

        var linger = C.OverlayLingerScale;
        if (Widgets.RowDrag("Hold time", "Multiplier on the built-in time", ref linger, 0.4f, 3f, "%.2fx"))
        {
            C.OverlayLingerScale = linger;
            Touch();
        }

        var countdown = C.OverlayCountdown;
        if (Widgets.RowCheckClick("Countdown", "Seconds left on the cast", ref countdown))
        {
            C.OverlayCountdown = countdown;
            Touch();
        }

        var newestTop = C.OverlayNewestOnTop;
        if (Widgets.RowCheckClick("Newest on top", "", ref newestTop))
        {
            C.OverlayNewestOnTop = newestTop;
            Touch();
        }

        Widgets.ListEnd();

        Widgets.SectionHeader("Shape");
        Widgets.ListBegin();

        var align = C.OverlayAlign;
        if (Widgets.RowCombo("Line up", "", ref align, AlignNames, 140f))
        {
            C.OverlayAlign = align;
            _plugin.Overlay.Reposition();
            Touch();
        }

        var padding = C.OverlayPadding;
        if (Widgets.RowDrag("Padding", "Room around the text", ref padding, 0f, 40f, "%.0f"))
        {
            C.OverlayPadding = padding;
            Touch();
        }

        var rounding = C.OverlayRounding;
        if (Widgets.RowDrag("Corners", "", ref rounding, 0f, 20f, "%.0f"))
        {
            C.OverlayRounding = rounding;
            Touch();
        }

        var lineGap = C.OverlayLineGap;
        if (Widgets.RowDrag("Line gap", "Space between calls", ref lineGap, 0f, 24f, "%.0f"))
        {
            C.OverlayLineGap = lineGap;
            Touch();
        }

        Widgets.ListEnd();

        ImGui.Spacing();
        if (Widgets.AccentButton("Test call")) _plugin.FireSample();

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.DangerButton("Clear screen")) Board.Clear();
    }

    private void DrawOverlayPlacement()
    {
        if (_overlayPreview && C.OverlayOn) _plugin.Overlay.KeepPreview();

        Widgets.SectionHeader("Where it sits");
        Widgets.ListBegin();

        var locked = C.OverlayLocked;
        if (Widgets.RowCheckClick("Locked", locked ? "Unlock to move it" : "Drag it anywhere",
                ref locked))
        {
            C.OverlayLocked = locked;
            _plugin.Overlay.Reposition();
            Touch();
        }

        var preview = _overlayPreview;
        if (Widgets.RowCheckClick("Show sample lines", "Stays up while you place it", ref preview))
            _overlayPreview = preview;

        var position = C.OverlayPosition;

        Widgets.RowBegin("Across", "From the left", Theme.S(150f));
        var x = position.X * 100f;
        if (ImGui.DragFloat("##ovx", ref x, 0.25f, 2f, 98f, "%.1f%%", ImGuiSliderFlags.AlwaysClamp))
        {
            C.OverlayPosition = new Vector2(x / 100f, C.OverlayPosition.Y);
            _plugin.Overlay.Reposition();
            Touch();
        }
        Widgets.RowEnd();

        Widgets.RowBegin("Down", "From the top", Theme.S(150f));
        var y = position.Y * 100f;
        if (ImGui.DragFloat("##ovy", ref y, 0.25f, 1f, 97f, "%.1f%%", ImGuiSliderFlags.AlwaysClamp))
        {
            C.OverlayPosition = new Vector2(C.OverlayPosition.X, y / 100f);
            _plugin.Overlay.Reposition();
            Touch();
        }
        Widgets.RowEnd();

        Widgets.ListEnd();

        ImGui.Spacing();
        foreach (var (label, spot) in Spots)
        {
            if (Widgets.GhostButton(label))
            {
                C.OverlayPosition = spot;
                _plugin.Overlay.Reposition();
                Touch();
            }
            ImGui.SameLine(0, Theme.S(5f));
        }

        if (Widgets.DangerButton("Default"))
        {
            C.OverlayPosition = new Vector2(0.5f, 0.24f);
            C.OverlayAlign = 1;
            _plugin.Overlay.Reposition();
            Touch();
        }

        ImGui.Spacing();
        Widgets.Banner(C.OverlayLocked ? Theme.Accent : Theme.Warn,
            C.OverlayLocked
                ? "Locked. Clicks go through it."
                : "Unlocked. Drag the teal box, then lock it.");
    }

    private static readonly (string Label, Vector2 Spot)[] Spots =
    [
        ("Top", new Vector2(0.5f, 0.08f)),
        ("Above middle", new Vector2(0.5f, 0.24f)),
        ("Middle", new Vector2(0.5f, 0.44f)),
        ("Low", new Vector2(0.5f, 0.66f)),
    ];

    private void DrawVoice()
    {
        PageHeader("Voice", _plugin.Speech.Status);

        Widgets.SectionHeader("Read out loud");
        Widgets.ListBegin();

        var on = C.TtsOn;
        if (Widgets.RowCheckClick("Voice", "Speak every call", ref on))
        {
            C.TtsOn = on;
            Touch();
        }

        var voices = _plugin.Speech.Voices;
        if (voices.Count > 0)
        {
            var names = new string[voices.Count + 1];
            names[0] = "System default";
            for (var i = 0; i < voices.Count; i++) names[i + 1] = voices[i];

            var index = 0;
            for (var i = 0; i < voices.Count; i++)
                if (string.Equals(voices[i], C.TtsVoice, StringComparison.OrdinalIgnoreCase))
                {
                    index = i + 1;
                    break;
                }

            if (Widgets.RowCombo("Voice", "Windows voices", ref index, names, 240f))
            {
                C.TtsVoice = index <= 0 ? "" : names[index];
                Touch();
            }
        }
        else
        {
            Widgets.RowNote("No Windows voices found.", Theme.Warn);
        }

        var rate = C.TtsRate;
        if (Widgets.RowDragInt("Speed", "", ref rate, -10, 10))
        {
            C.TtsRate = rate;
            Touch();
        }

        var volume = C.TtsVolume;
        if (Widgets.RowDragInt("Volume", "", ref volume, 0, 100))
        {
            C.TtsVolume = volume;
            Touch();
        }

        var gap = C.TtsMinGap;
        if (Widgets.RowDrag("Min gap", "Least time between spoken calls", ref gap, 0f, 5f, "%.1fs"))
        {
            C.TtsMinGap = gap;
            Touch();
        }

        var onlyInFight = C.TtsOnlyInFight;
        if (Widgets.RowCheckClick("Only in combat", "Quiet outside a pull", ref onlyInFight))
        {
            C.TtsOnlyInFight = onlyInFight;
            Touch();
        }

        Widgets.ListEnd();

        ImGui.Spacing();
        if (Widgets.AccentButton("Test voice"))
            _plugin.Speech.Say("Stack north, tank buster", C.TtsRate, C.TtsVolume, C.TtsVoice);

        ImGui.SameLine(0, Theme.S(8f));
        ImGui.AlignTextToFramePadding();
        var dropped = _plugin.Speech.Dropped;
        ImGui.TextColored(Theme.V(dropped > 0 ? Theme.Warn : Theme.Muted),
            dropped > 0 ? $"{dropped} dropped, queue full" : _plugin.Speech.Status);
    }

    private void DrawLook()
    {
        PageHeader("Look", "");

        Widgets.SectionHeader("Base");
        Widgets.ListBegin();

        var skin = C.Skin;
        if (Widgets.RowCombo("Background", "", ref skin, Theme.SkinNames, 150f))
        {
            C.Skin = skin;
            Touch();
        }

        Widgets.ListEnd();

        Widgets.SectionHeader("Accent");

        var room = ImGui.GetContentRegionAvail().X;
        var used = 0f;
        foreach (var (name, color) in Theme.Swatches)
        {
            var width = Widgets.SwatchChipWidth(name);
            if (used > 0f && used + width > room)
            {
                used = 0f;
            }
            else if (used > 0f)
            {
                ImGui.SameLine(0, Theme.S(5f));
                used += Theme.S(5f);
            }

            if (Widgets.SwatchChip(name, color, C.AccentColor == color))
            {
                C.AccentColor = color;
                Touch();
            }

            used += width;
        }

        ImGui.Spacing();
        Widgets.ListBegin();

        var accent = Theme.V(C.AccentColor);
        if (Widgets.RowColor("Pick your own", "", ref accent))
        {
            C.AccentColor = Theme.Pack(accent);
            Touch();
        }

        var scale = C.UiScale;
        if (Widgets.RowDrag("Window size", "Text and spacing", ref scale, 0.8f, 1.6f, "%.2fx"))
        {
            C.UiScale = scale;
            Touch();
        }

        var colorblind = C.Colorblind;
        if (Widgets.RowCheckClick("Colorblind safe", "No red and green pairs", ref colorblind))
        {
            C.Colorblind = colorblind;
            Touch();
        }

        Widgets.ListEnd();

        ImGui.Spacing();
        if (Widgets.DangerButton("Reset look"))
        {
            C.AccentColor = Theme.DefaultAccent;
            C.UiScale = 1f;
            C.Colorblind = false;
            C.Skin = 0;
            Touch();
        }
    }
}
