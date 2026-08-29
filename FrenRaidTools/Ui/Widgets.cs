using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace FrenRaidTools.Ui;

internal static partial class Widgets
{
    private const double TooltipDelay = 0.35;

    private static Vector2 _tipPos;
    private static double _tipSince;
    private static int _tipFrame;

    public static bool HoveredDelayed(ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
    {
        if (!ImGui.IsItemHovered(flags)) return false;

        var pos = ImGui.GetItemRectMin();
        var now = ImGui.GetTime();
        var frame = ImGui.GetFrameCount();
        if (pos != _tipPos || frame - _tipFrame > 2) { _tipPos = pos; _tipSince = now; }
        _tipFrame = frame;
        return now - _tipSince >= TooltipDelay;
    }

    public static void Tip(string text)
    {
        if (text.Length > 0 && HoveredDelayed()) ImGui.SetTooltip(text);
    }

    public static string Elide(string text, float maxWidth)
    {
        if (text.Length == 0 || maxWidth <= 0f) return text;
        if (ImGui.CalcTextSize(text).X <= maxWidth) return text;

        const string tail = "...";
        var room = maxWidth - ImGui.CalcTextSize(tail).X;
        if (room <= 0f) return tail;

        int lo = 0, hi = text.Length;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (ImGui.CalcTextSize(text[..mid]).X <= room) lo = mid; else hi = mid - 1;
        }

        return lo <= 0 ? tail : text[..lo].TrimEnd() + tail;
    }

    public static void WindowHeader(string title, string detail = "")
    {
        var p = ImGui.GetCursorScreenPos();
        var h = ImGui.GetFrameHeight();
        ImGui.GetWindowDrawList().AddRectFilled(
            p + new Vector2(0, 2), p + new Vector2(Theme.S(3f), h - 2), Theme.Accent, 2f);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Theme.S(10f));
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(Theme.Accent), title);
        if (detail.Length == 0) return;
        ImGui.SameLine(0, Theme.S(10f));
        ImGui.TextColored(Theme.V(Theme.Muted), detail);
    }

    public static void SectionHeader(string text)
    {
        ImGui.Dummy(new Vector2(0, Theme.S(4f)));
        var dl = ImGui.GetWindowDrawList();
        var p = ImGui.GetCursorScreenPos();
        var h = ImGui.GetTextLineHeight();
        dl.AddRectFilled(p + new Vector2(0, 1), p + new Vector2(Theme.S(3f), h), Theme.Accent, 2f);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Theme.S(10f));
        ImGui.TextColored(Theme.V(Theme.SectionText), text.ToUpperInvariant());
        ImGui.Spacing();
    }

    public static void PageNote(string text)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + RowPad);
        ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - RowPad);
        ImGui.TextColored(Theme.V(Theme.Muted), text);
        ImGui.PopTextWrapPos();
        ImGui.Spacing();
    }

    private static Vector2 _cardTop;
    private static float _cardWidth;

    public static void CardBegin()
    {
        _cardTop = ImGui.GetCursorScreenPos();
        _cardWidth = ImGui.GetContentRegionAvail().X;

        var dl = ImGui.GetWindowDrawList();
        dl.ChannelsSplit(2);
        dl.ChannelsSetCurrent(1);

        ImGui.Dummy(new Vector2(0, Theme.S(11f) - ImGui.GetStyle().ItemSpacing.Y));
        ImGui.Indent(RowPad);
    }

    public static void CardEnd()
    {
        ImGui.Unindent(RowPad);
        ImGui.Dummy(new Vector2(0, Theme.S(11f) - ImGui.GetStyle().ItemSpacing.Y));

        var end = new Vector2(_cardTop.X + _cardWidth, ImGui.GetCursorScreenPos().Y);
        var dl = ImGui.GetWindowDrawList();
        dl.ChannelsSetCurrent(0);
        dl.AddRectFilled(_cardTop, end, Theme.ListBg, Theme.S(8f));
        dl.AddRect(_cardTop, end, Theme.Border, Theme.S(8f));
        dl.ChannelsMerge();
    }

    public static bool Check(string label, ref bool v)
    {
        var on = v;
        if (on)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Theme.CheckOn);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, Theme.CheckOnHover);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, Theme.CheckOn);
            ImGui.PushStyleColor(ImGuiCol.CheckMark, Theme.CheckMark);
        }

        var changed = ImGui.Checkbox(label, ref v);
        if (on) ImGui.PopStyleColor(4);
        return changed;
    }

    private static Vector2 ChipPad => new Vector2(8, 3) * Theme.Scale;
    private static float ChipGap => Theme.S(5f);

    private static Vector2 ChipSize(Vector2 label, Vector2 value, bool hasLabel) =>
        new(label.X + value.X + (hasLabel ? ChipGap : 0f) + ChipPad.X * 2,
            ImGui.GetTextLineHeight() + ChipPad.Y * 2);

    public static void Chip(string label, string value, uint valueColor)
    {
        var pad = ChipPad;
        var hasLabel = label.Length > 0;
        var labelSize = ImGui.CalcTextSize(label);
        var size = ChipSize(labelSize, ImGui.CalcTextSize(value), hasLabel);
        var p = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(p, p + size, Theme.PanelBg, Theme.S(5f));
        dl.AddRect(p, p + size, Theme.Border, Theme.S(5f));
        if (hasLabel) dl.AddText(p + pad, Theme.Muted, label);
        dl.AddText(p + pad + new Vector2(hasLabel ? labelSize.X + ChipGap : 0f, 0), valueColor, value);
        ImGui.Dummy(size);
    }

    public static bool SwatchChip(string name, uint color, bool on)
    {
        var pad = ChipPad;
        var square = ImGui.GetTextLineHeight() * 0.72f;
        var textSize = ImGui.CalcTextSize(name);
        var size = new Vector2(square + ChipGap + textSize.X + pad.X * 2,
            ImGui.GetTextLineHeight() + pad.Y * 2);
        var p = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton("##sw" + name, size);
        var hovered = ImGui.IsItemHovered();
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        var dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(p, p + size,
            on ? Theme.AccentSoft : hovered ? Theme.FrameBg : Theme.PanelBg, Theme.S(5f));
        dl.AddRect(p, p + size, on ? Theme.Accent : Theme.Border, Theme.S(5f));

        var y = p.Y + (size.Y - square) * 0.5f;
        dl.AddRectFilled(new Vector2(p.X + pad.X, y), new Vector2(p.X + pad.X + square, y + square), color, 2f);
        dl.AddText(new Vector2(p.X + pad.X + square + ChipGap, p.Y + pad.Y), Theme.TextBright, name);
        return clicked;
    }

    public static float SwatchChipWidth(string name) =>
        ImGui.GetTextLineHeight() * 0.72f + ChipGap + ImGui.CalcTextSize(name).X + ChipPad.X * 2;

    public static bool AccentButton(string label, Vector2 size = default)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Theme.Accent);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.AccentHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Theme.AccentHover);
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.ReadableOn(Theme.Accent));
        var clicked = ImGui.Button(label, size);
        ImGui.PopStyleColor(4);
        return clicked;
    }

    public static bool GhostButton(string label, Vector2 size = default)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Theme.AccentFaint);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.AccentSoft);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Theme.AccentSoft);
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Accent);
        var clicked = ImGui.Button(label, size);
        ImGui.PopStyleColor(4);
        return clicked;
    }

    public static bool DangerButton(string label, Vector2 size = default)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Theme.Wash(Theme.Danger, 0x28));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.Wash(Theme.Danger, 0x4C));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Theme.Wash(Theme.Danger, 0x6E));
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Danger);
        var clicked = ImGui.Button(label, size);
        ImGui.PopStyleColor(4);
        return clicked;
    }

    public static bool SmallGhost(string label)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Theme.AccentFaint);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.AccentSoft);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Theme.AccentSoft);
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Accent);
        var clicked = ImGui.SmallButton(label);
        ImGui.PopStyleColor(4);
        return clicked;
    }

    public static bool SmallDanger(string label)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Theme.Wash(Theme.Danger, 0x28));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.Wash(Theme.Danger, 0x4C));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Theme.Wash(Theme.Danger, 0x6E));
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Danger);
        var clicked = ImGui.SmallButton(label);
        ImGui.PopStyleColor(4);
        return clicked;
    }

    private static float _segmentLeft;

    public static void SegmentBegin()
    {
        _segmentLeft = ImGui.GetCursorScreenPos().X;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1f, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
    }

    public static bool Segment(string label, bool on) => Segment(label, on, Theme.Accent);

    public static bool Segment(string label, bool on, uint onColor)
    {
        if (on)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, onColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.Lighten(onColor, 0.22f));
            ImGui.PushStyleColor(ImGuiCol.Text, Theme.ReadableOn(onColor));
        }

        var clicked = ImGui.SmallButton(label);
        if (on) ImGui.PopStyleColor(3);
        return clicked;
    }

    public static bool SegmentTall(string label, bool on)
    {
        if (on)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Theme.Accent);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.AccentHover);
            ImGui.PushStyleColor(ImGuiCol.Text, Theme.ReadableOn(Theme.Accent));
        }

        var clicked = ImGui.Button(label);
        if (on) ImGui.PopStyleColor(3);
        return clicked;
    }

    public static void SegmentEnd()
    {
        ImGui.PopStyleVar(2);
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        ImGui.GetWindowDrawList().AddRect(new Vector2(_segmentLeft, min.Y), max, Theme.Border, 4f);
    }

    public static float ButtonWidth(params string[] labels)
    {
        var pad = ImGui.GetStyle().FramePadding.X * 2f;
        var w = 0f;
        foreach (var label in labels) w += ImGui.CalcTextSize(label).X + pad + 1f;
        return w;
    }

    public static void EmptyState(string title, string hint)
    {
        ImGui.Dummy(new Vector2(0, Theme.S(24f)));
        var width = ImGui.GetContentRegionAvail().X;
        var titleSize = ImGui.CalcTextSize(title);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (width - titleSize.X) * 0.5f));
        ImGui.TextColored(Theme.V(Theme.Heading), title);
        if (hint.Length == 0) return;
        var hintSize = ImGui.CalcTextSize(hint);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (width - hintSize.X) * 0.5f));
        ImGui.TextColored(Theme.V(Theme.Muted), hint);
    }
}
