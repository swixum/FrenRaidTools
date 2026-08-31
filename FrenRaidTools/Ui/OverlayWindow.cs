using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace FrenRaidTools.Ui;

public class OverlayWindow : Window
{
    private static readonly Vector2[] OutlineOffsets =
    [
        new(-1f, 0f), new(1f, 0f), new(0f, -1f), new(0f, 1f),
        new(-0.7f, -0.7f), new(0.7f, -0.7f), new(-0.7f, 0.7f), new(0.7f, 0.7f),
    ];

    private static readonly string[] SampleLines =
    [
        "Spread out of Cones",
        "Take Tower (4.2)",
        "Stack north, tank buster",
    ];

    private const float FadeSeconds = 0.35f;
    private const string DragHint = "Drag me";
    private const double PreviewHold = 0.5;

    private readonly Plugin _plugin;
    private Configuration C => _plugin.Config;

    private bool _snap = true;
    private bool _paintedBackground;
    private bool _draggable;
    private bool _dragged;

    private double _previewUntil = double.NegativeInfinity;

    public void KeepPreview() => _previewUntil = _plugin.Now + PreviewHold;

    public bool Preview => _plugin.Now < _previewUntil;

    public OverlayWindow(Plugin plugin)
        : base("FrenRaidTools##overlay")
    {
        _plugin = plugin;
        RespectCloseHotkey = false;
        DisableWindowSounds = true;
        ForceMainWindow = true;
        IsOpen = true;
    }

    public void Reposition() => _snap = true;

    private bool Placing => Preview || !C.OverlayLocked;

    public override bool DrawConditions()
    {
        if (!C.OverlayOn) return false;
        if (Placing) return true;
        if (_plugin.Board.HasTest) return true;
        if (C.OnlyInFight && !Game.InTheFight) return false;
        return _plugin.Board.LiveCount > 0;
    }

    public override void PreDraw()
    {
        _plugin.Fonts.Warm(C.OverlayFontPx);

        _draggable = !C.OverlayLocked;
        _paintedBackground = C.OverlayBackground || _draggable;

        Flags = ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoSavedSettings
                | ImGuiWindowFlags.NoFocusOnAppearing
                | ImGuiWindowFlags.NoNav
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.AlwaysAutoResize;

        if (!_paintedBackground) Flags |= ImGuiWindowFlags.NoBackground;

        if (!_draggable)
            Flags |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoMouseInputs;

        if (_paintedBackground)
            ImGui.PushStyleColor(ImGuiCol.WindowBg,
                _draggable ? Theme.V(Theme.Wash(Theme.Accent, 0x24)) : C.OverlayBackgroundColor);

        var padding = MathF.Round(C.OverlayPadding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,
            new Vector2(padding, MathF.Round(padding * 0.72f)));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, C.OverlayRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, _draggable ? 2f : 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,
            new Vector2(0f, MathF.Round(C.OverlayLineGap)));

        if (_draggable) ImGui.PushStyleColor(ImGuiCol.Border, Theme.Accent);

        var viewport = ImGui.GetMainViewport();
        var target = viewport.WorkPos + C.OverlayPosition * viewport.WorkSize;
        target = new Vector2(MathF.Round(target.X), MathF.Round(target.Y));

        if (!_draggable || _snap || (!ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_dragged))
        {
            ImGui.SetNextWindowPos(target, ImGuiCond.Always, Anchor);
            _snap = false;
        }
    }

    private Vector2 Anchor => C.OverlayAlign switch
    {
        0 => new Vector2(0f, 0f),
        2 => new Vector2(1f, 0f),
        _ => new Vector2(0.5f, 0f),
    };

    public override void PostDraw()
    {
        if (_draggable) ImGui.PopStyleColor();
        ImGui.PopStyleVar(4);
        if (_paintedBackground) ImGui.PopStyleColor();
    }

    public override void Draw()
    {
        try
        {
            Paint();
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Overlay draw failed.");
        }
    }

    private void Paint()
    {
        SavePositionIfDragged();
        if (_draggable && ImGui.IsMouseDown(ImGuiMouseButton.Left)) _dragged = true;

        var now = _plugin.Now;
        var live = _plugin.Board.Visible();

        using var font = _plugin.Fonts.Push(C.OverlayFontPx);

        var lines = Lines(live, now);

        if (lines.Count == 0)
        {
            DrawLine(new Row(DragHint, Theme.Fade(Theme.Accent, 0.85f), []), 0f);
            return;
        }

        var width = 0f;
        foreach (var row in lines) width = MathF.Max(width, Wide(row));
        foreach (var row in lines) DrawLine(row, width);
    }

    private readonly record struct Row(string Text, uint Color, IReadOnlyList<uint> Icons);

    private List<Row> Lines(List<LiveCall> live, double now)
    {
        var lines = new List<Row>(Math.Max(live.Count, SampleLines.Length));
        var color = Theme.Pack(C.OverlayTextColor);

        if (live.Count == 0)
        {
            if (!Preview) return lines;

            foreach (var sample in SampleLines) lines.Add(new Row(sample, color, []));
            return lines;
        }

        foreach (var call in live)
        {
            var text = call.Rendered(now, C.OverlayCountdown);
            if (text.Length == 0) continue;

            var left = (float)(call.Expires - now);
            var alpha = left >= FadeSeconds ? 1f : MathF.Max(0f, left / FadeSeconds);
            lines.Add(new Row(text, Theme.Fade(color, alpha), call.Icons));
        }

        if (C.OverlayNewestOnTop) lines.Reverse();
        return lines;
    }

    private float IconPx => MathF.Round(C.OverlayFontPx * 0.92f);

    private float IconGap => MathF.Round(C.OverlayFontPx * 0.18f);

    private float IconRun(IReadOnlyList<uint> icons) =>
        !C.OverlayIcons || icons.Count == 0 ? 0f : icons.Count * (IconPx + IconGap);

    private float Wide(Row row) => ImGui.CalcTextSize(row.Text).X + IconRun(row.Icons);

    private void DrawLine(Row row, float columnWidth)
    {
        var size = ImGui.CalcTextSize(row.Text);
        var run = IconRun(row.Icons);
        var slack = MathF.Max(0f, columnWidth - size.X - run);
        var offset = C.OverlayAlign switch
        {
            0 => 0f,
            2 => slack,
            _ => slack * 0.5f,
        };

        if (offset > 0f) ImGui.SetCursorPosX(MathF.Round(ImGui.GetCursorPosX() + offset));

        var raw = ImGui.GetCursorScreenPos();
        var pos = new Vector2(MathF.Round(raw.X), MathF.Round(raw.Y));
        var dl = ImGui.GetWindowDrawList();

        if (run > 0f) DrawIcons(dl, row.Icons, pos, size.Y, (row.Color >> 24) / 255f);

        var at = pos with { X = pos.X + run };

        if (C.OverlayOutline)
        {
            var shadow = Theme.Fade(0xFF000000, (row.Color >> 24) / 255f * 0.85f);
            var thickness = MathF.Max(1f, MathF.Round(C.OverlayFontPx / 22f));
            foreach (var nudge in OutlineOffsets) dl.AddText(at + nudge * thickness, shadow, row.Text);
        }

        dl.AddText(at, row.Color, row.Text);
        ImGui.Dummy(new Vector2(size.X + run, size.Y));
    }

    private void DrawIcons(
        ImDrawListPtr dl, IReadOnlyList<uint> icons, Vector2 pos, float lineHeight, float alpha)
    {
        var side = IconPx;
        var top = MathF.Round(pos.Y + MathF.Max(0f, (lineHeight - side) * 0.5f));
        var tint = Theme.Fade(0xFFFFFFFF, alpha);
        var x = pos.X;

        foreach (var icon in icons)
        {
            var texture = Icons.Texture(icon);
            if (texture is not null)
            {
                var min = new Vector2(MathF.Round(x), top);
                dl.AddImage(texture.Handle, min, min + new Vector2(side, side),
                    Vector2.Zero, Vector2.One, tint);
            }

            x += side + IconGap;
        }
    }

    private void SavePositionIfDragged()
    {
        if (!_draggable) { _dragged = false; return; }
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left)) return;

        var dragged = _dragged;
        _dragged = false;
        if (!dragged) return;

        var viewport = ImGui.GetMainViewport();
        if (viewport.WorkSize.X <= 0f || viewport.WorkSize.Y <= 0f) return;

        var anchor = Anchor;
        var handle = ImGui.GetWindowPos() + new Vector2(ImGui.GetWindowSize().X * anchor.X, 0f);
        var fraction = (handle - viewport.WorkPos) / viewport.WorkSize;
        fraction = new Vector2(Math.Clamp(fraction.X, 0.02f, 0.98f), Math.Clamp(fraction.Y, 0.01f, 0.97f));

        if (Vector2.Distance(fraction, C.OverlayPosition) < 0.0005f) return;

        C.OverlayPosition = fraction;
        C.Save(_plugin.Now);
    }
}
