using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace FrenRaidTools.Ui;

internal static partial class Widgets
{
    private static Vector2 _listTop;
    private static float _listWidth;
    private static int _rowIndex;
    private static Vector2 _rowNext;
    private static bool _rowClicked;

    public static float RowPad => Theme.S(11f);

    private static bool _listOpen;

    public static void ListBegin()
    {
        if (_listOpen) ListEnd();

        _listTop = ImGui.GetCursorScreenPos();
        _listWidth = ImGui.GetContentRegionAvail().X;
        _rowIndex = 0;
        _listOpen = true;

        var dl = ImGui.GetWindowDrawList();
        dl.ChannelsSplit(2);
        dl.ChannelsSetCurrent(1);
    }

    public static void ListEnd()
    {
        if (!_listOpen) return;
        _listOpen = false;

        var dl = ImGui.GetWindowDrawList();
        var max = new Vector2(_listTop.X + _listWidth, ImGui.GetCursorScreenPos().Y);
        dl.ChannelsSetCurrent(0);
        dl.AddRectFilled(_listTop, max, Theme.ListBg, Theme.S(7f));
        dl.AddRect(_listTop, max, Theme.Border, Theme.S(7f));
        dl.ChannelsMerge();
    }

    public static bool FoldBegin(string id, string title, string badge, uint badgeColor, ref bool open)
    {
        ListBegin();

        var height = ImGui.GetFrameHeight() + Theme.S(5f);
        var width = ImGui.GetContentRegionAvail().X;
        var start = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton("##fold" + id, new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        var dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(start, start + new Vector2(width, height),
            hovered ? Theme.RowHover : Theme.SubBg, Theme.S(7f),
            open ? ImDrawFlags.RoundCornersTop : ImDrawFlags.RoundCornersAll);

        Caret(dl, start, height, open);

        var line = ImGui.GetTextLineHeight();
        dl.AddText(new Vector2(start.X + RowPad, start.Y + (height - line) * 0.5f),
            open ? Theme.TextBright : Theme.Heading, title.ToUpperInvariant());

        if (badge.Length > 0)
        {
            var size = ImGui.CalcTextSize(badge);
            dl.AddText(new Vector2(start.X + width - RowPad - size.X, start.Y + (height - size.Y) * 0.5f),
                badgeColor == 0 ? Theme.Muted : badgeColor, badge);
        }

        _rowIndex = 1;
        if (clicked) open = !open;
        return open;
    }

    private static void Caret(ImDrawListPtr dl, Vector2 start, float height, bool open)
    {
        var size = Theme.S(4f);
        var mid = new Vector2(start.X + Theme.S(5.5f), start.Y + height * 0.5f);

        if (open)
            dl.AddTriangleFilled(
                mid + new Vector2(-size, -size * 0.6f),
                mid + new Vector2(size, -size * 0.6f),
                mid + new Vector2(0f, size * 0.8f), Theme.Accent);
        else
            dl.AddTriangleFilled(
                mid + new Vector2(-size * 0.6f, -size),
                mid + new Vector2(-size * 0.6f, size),
                mid + new Vector2(size * 0.8f, 0f), Theme.Muted);
    }

    public static void FoldEnd() => ListEnd();

    public static bool Crumb(string parent)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, 0u);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.AccentFaint);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Theme.AccentSoft);
        ImGui.PushStyleColor(ImGuiCol.Text, Theme.Muted);
        var clicked = ImGui.SmallButton("<  " + parent);
        ImGui.PopStyleColor(4);
        if (ImGui.IsItemHovered()) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return clicked;
    }

    public static void ListAbort()
    {
        DropRowIds();

        if (!_listOpen) return;
        _listOpen = false;

        var dl = ImGui.GetWindowDrawList();
        dl.ChannelsSetCurrent(0);
        dl.ChannelsMerge();
    }

    private static float RowTextHeight(bool hasHint) =>
        hasHint
            ? ImGui.GetTextLineHeight() * 2f + ImGui.GetStyle().ItemSpacing.Y
            : ImGui.GetTextLineHeight();

    private static float RowHeight(bool hasHint) =>
        MathF.Max(ImGui.GetFrameHeight(), RowTextHeight(hasHint)) + Theme.S(8f) * 2f;

    public static void RowBegin(string name, string hint, float controlWidth,
        bool sub = false, float controlHeight = 0f, bool clickable = false,
        FontAwesomeIcon icon = FontAwesomeIcon.None, uint iconColor = 0, string id = "",
        uint edgeColor = 0, string tag = "", uint hintColor = 0)
    {
        if (id.Length == 0) id = name;

        ImGui.PushID(id);
        _rowIds++;

        var hasHint = !string.IsNullOrEmpty(hint);
        var rowH = RowHeight(hasHint);
        var frameH = controlHeight > 0f ? controlHeight : ImGui.GetFrameHeight();
        var textH = RowTextHeight(hasHint);

        var start = ImGui.GetCursorPos();
        var screen = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var dl = ImGui.GetWindowDrawList();

        var mouse = ImGui.GetMousePos();
        var hot = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows)
                  && mouse.X >= screen.X && mouse.X <= screen.X + width
                  && mouse.Y >= screen.Y && mouse.Y <= screen.Y + rowH;

        _rowClicked = false;
        if (clickable)
        {
            _rowClicked = ImGui.InvisibleButton("##hit" + id, new Vector2(width, rowH));
            if (ImGui.IsItemHovered()) { hot = true; ImGui.SetMouseCursor(ImGuiMouseCursor.Hand); }
            ImGui.SetItemAllowOverlap();
            ImGui.SetCursorPos(start);
        }

        if (sub) dl.AddRectFilled(screen, screen + new Vector2(width, rowH), Theme.SubBg);
        if (hot) dl.AddRectFilled(screen, screen + new Vector2(width, rowH), Theme.RowHover);
        if (_rowIndex > 0) dl.AddLine(screen, screen + new Vector2(width, 0), Theme.RowLine);
        if (edgeColor != 0)
            dl.AddRectFilled(screen + new Vector2(0, Theme.S(6f)),
                screen + new Vector2(Theme.S(2.5f), rowH - Theme.S(6f)), edgeColor, 2f);
        _rowIndex++;

        var textX = start.X + RowPad + (sub ? Theme.S(17f) : 0f);
        textX += DrawRowIcon(icon, iconColor, textX, start.Y, rowH);

        var room = MathF.Max(Theme.S(40f),
            width - RowPad - controlWidth - (textX - start.X) - Theme.S(6f));

        ImGui.SetCursorPos(new Vector2(textX, start.Y + (rowH - textH) * 0.5f));

        if (tag.Length > 0)
        {
            var gap = Theme.S(7f);
            ImGui.TextUnformatted(Elide(name, room - ImGui.CalcTextSize(tag).X - gap));
            ImGui.SameLine(0, gap);
            ImGui.TextColored(Theme.V(Theme.Muted), tag);
        }
        else
        {
            ImGui.TextUnformatted(Elide(name, room));
        }

        if (hasHint)
        {
            ImGui.SetCursorPosX(textX);
            ImGui.TextColored(Theme.V(hintColor == 0 ? Theme.Muted : hintColor), Elide(hint, room));
        }

        ImGui.SetCursorPos(new Vector2(start.X + width - RowPad - controlWidth,
            start.Y + (rowH - frameH) * 0.5f));
        ImGui.SetNextItemWidth(controlWidth);
        _rowNext = new Vector2(start.X, start.Y + rowH);
    }

    private static int _rowIds;

    public static void RowEnd()
    {
        if (_rowIds > 0)
        {
            ImGui.PopID();
            _rowIds--;
        }

        ImGui.SetCursorPos(_rowNext);
    }

    private static void DropRowIds()
    {
        while (_rowIds > 0)
        {
            ImGui.PopID();
            _rowIds--;
        }
    }

    private static float DrawRowIcon(FontAwesomeIcon icon, uint color, float textX, float startY, float rowH)
    {
        if (icon == FontAwesomeIcon.None) return 0f;

        var slot = Theme.S(20f);
        using (Service.PluginInterface.UiBuilder.IconFontHandle.Push())
        {
            var glyph = icon.ToIconString();
            var size = ImGui.CalcTextSize(glyph);
            ImGui.SetCursorPos(new Vector2(textX + (slot - size.X) * 0.5f, startY + (rowH - size.Y) * 0.5f));
            ImGui.TextColored(Theme.V(color == 0 ? Theme.Muted : color), glyph);
        }

        return slot + Theme.S(7f);
    }

    public static bool RowCheckClick(string name, string hint, ref bool v,
        FontAwesomeIcon icon = FontAwesomeIcon.None, uint iconColor = 0, string id = "", bool sub = false)
    {
        if (id.Length == 0) id = name;
        RowBegin(name, hint, ImGui.GetFrameHeight(), sub, clickable: true,
            icon: icon, iconColor: iconColor, id: id);
        var hit = Check("##rc" + id, ref v);
        if (_rowClicked) { v = !v; hit = true; }
        RowEnd();
        return hit;
    }

    public static void RowText(string name, string hint, bool sub = false, string id = "")
    {
        RowBegin(name, hint, 0f, sub, id: id);
        RowEnd();
    }

    public static void RowValue(string name, string hint, string value, uint color,
        bool sub = false, string id = "", string tip = "")
    {
        RowBegin(name, hint, ImGui.CalcTextSize(value).X, sub, id: id);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(color), value);
        Tip(tip);
        RowEnd();
    }

    public static bool RowDrag(string name, string hint, ref float v, float min, float max,
        string fmt, float width = 110f, bool sub = false)
    {
        RowBegin(name, hint, Theme.S(width), sub);
        var hit = ImGui.DragFloat("##rd" + name, ref v, MathF.Max(0.001f, (max - min) / 200f),
            min, max, fmt, ImGuiSliderFlags.AlwaysClamp);
        Tip("Drag or double click to type");
        RowEnd();
        return hit;
    }

    public static bool RowDragInt(string name, string hint, ref int v, int min, int max,
        string fmt = "%d", float width = 110f, bool sub = false)
    {
        RowBegin(name, hint, Theme.S(width), sub);
        var hit = ImGui.DragInt("##ri" + name, ref v, MathF.Max(0.05f, (max - min) / 200f),
            min, max, fmt, ImGuiSliderFlags.AlwaysClamp);
        Tip("Drag or double click to type");
        RowEnd();
        return hit;
    }

    public static bool RowCombo(string name, string hint, ref int index, string[] items,
        float width = 190f, bool sub = false)
    {
        RowBegin(name, hint, Theme.S(width), sub);
        var hit = ImGui.Combo("##rk" + name, ref index, items, items.Length);
        RowEnd();
        return hit;
    }

    public static bool RowColor(string name, string hint, ref Vector4 color, bool sub = false)
    {
        RowBegin(name, hint, ImGui.GetFrameHeight(), sub);
        var hit = ImGui.ColorEdit4("##rw" + name, ref color, ImGuiColorEditFlags.NoInputs);
        RowEnd();
        return hit;
    }

    public static void RowNote(string text, uint color = 0)
    {
        var height = ImGui.GetTextLineHeight() + Theme.S(8f) * 2f;
        var start = ImGui.GetCursorPos();
        var screen = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        if (_rowIndex > 0)
            ImGui.GetWindowDrawList().AddLine(screen, screen + new Vector2(width, 0), Theme.RowLine);
        _rowIndex++;

        ImGui.SetCursorPos(new Vector2(start.X + RowPad, start.Y + Theme.S(8f)));
        ImGui.TextColored(Theme.V(color == 0 ? Theme.Muted : color), Elide(text, width - RowPad * 2f));
        ImGui.SetCursorPos(new Vector2(start.X, start.Y + height));
    }

    public static void RowNoteWrap(string text, uint color = 0)
    {
        var pad = Theme.S(8f);
        var start = ImGui.GetCursorPos();
        var screen = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        if (_rowIndex > 0)
            ImGui.GetWindowDrawList().AddLine(screen, screen + new Vector2(width, 0), Theme.RowLine);
        _rowIndex++;

        ImGui.SetCursorPos(new Vector2(start.X + RowPad, start.Y + pad));
        ImGui.PushTextWrapPos(start.X + width - RowPad);
        ImGui.TextColored(Theme.V(color == 0 ? Theme.Muted : color), text);
        ImGui.PopTextWrapPos();

        var bottom = ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y + pad;
        ImGui.SetCursorPos(new Vector2(start.X, bottom));
    }

    public static bool ConfirmPopup(string id, string question, string yes, string no)
    {
        if (!ImGui.BeginPopup(id)) return false;

        ImGui.TextUnformatted(question);
        ImGui.Spacing();

        var hit = DangerButton(yes);
        if (hit) ImGui.CloseCurrentPopup();

        ImGui.SameLine(0, Theme.S(6f));
        if (GhostButton(no)) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
        return hit;
    }

    public static void Banner(uint color, string text)
    {
        var padding = new Vector2(Theme.S(10f), Theme.S(6f));
        var size = new Vector2(ImGui.GetContentRegionAvail().X,
            ImGui.GetTextLineHeight() + padding.Y * 2f);
        var p = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(p, p + size, Theme.Wash(color, 0x22), Theme.S(6f));
        dl.AddRectFilled(p, p + new Vector2(Theme.S(3f), size.Y), color, 2f);
        dl.AddText(p + new Vector2(RowPad, padding.Y), color, text);

        ImGui.Dummy(size);
        ImGui.Dummy(new Vector2(0, Theme.S(2f)));
    }

    public static bool RowDoor(string name, string hint, uint edgeColor = 0)
    {
        var hasHint = !string.IsNullOrEmpty(hint);
        var rowH = RowHeight(hasHint);
        var textH = RowTextHeight(hasHint);
        var lineH = ImGui.GetTextLineHeight();

        var start = ImGui.GetCursorPos();
        var screen = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        var dl = ImGui.GetWindowDrawList();
        if (_rowIndex > 0) dl.AddLine(screen, screen + new Vector2(width, 0), Theme.RowLine);
        _rowIndex++;

        var hit = ImGui.Selectable("##door" + name, false, ImGuiSelectableFlags.None, new Vector2(width, rowH));
        var hovered = ImGui.IsItemHovered();
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            dl.AddRectFilled(screen, screen + new Vector2(width, rowH), Theme.RowHover);
        }

        if (edgeColor != 0)
            dl.AddRectFilled(screen + new Vector2(0, Theme.S(6f)),
                screen + new Vector2(Theme.S(2.5f), rowH - Theme.S(6f)), edgeColor, 2f);

        ImGui.SetCursorPos(new Vector2(start.X + RowPad, start.Y + (rowH - textH) * 0.5f));
        ImGui.TextUnformatted(name);
        if (hasHint)
        {
            ImGui.SetCursorPosX(start.X + RowPad);
            ImGui.TextColored(Theme.V(Theme.Muted), hint);
        }

        const string chevron = ">";
        ImGui.SetCursorPos(new Vector2(start.X + width - RowPad - ImGui.CalcTextSize(chevron).X,
            start.Y + (rowH - lineH) * 0.5f));
        ImGui.TextColored(Theme.V(Theme.Accent), chevron);

        ImGui.SetCursorPos(new Vector2(start.X, start.Y + rowH));
        return hit;
    }

    public static bool RowPickFold(string name, string hint, bool picked, ref bool open)
    {
        if (!PickRow(name, hint, picked, open)) return false;

        if (picked)
        {
            open = !open;
            return false;
        }

        open = true;
        return true;
    }

    private static bool PickRow(string name, string hint, bool picked, bool open)
    {
        var hasHint = !string.IsNullOrEmpty(hint);
        var rowH = RowHeight(hasHint);
        var textH = RowTextHeight(hasHint);
        var lineH = ImGui.GetTextLineHeight();

        var start = ImGui.GetCursorPos();
        var screen = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        var dl = ImGui.GetWindowDrawList();
        if (_rowIndex > 0) dl.AddLine(screen, screen + new Vector2(width, 0), Theme.RowLine);
        _rowIndex++;

        var hit = ImGui.Selectable("##pick" + name, false, ImGuiSelectableFlags.None,
            new Vector2(width, rowH));
        var hovered = ImGui.IsItemHovered();
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            dl.AddRectFilled(screen, screen + new Vector2(width, rowH), Theme.RowHover);
        }

        if (picked)
        {
            dl.AddRectFilled(screen, screen + new Vector2(width, rowH), Theme.AccentFaint);
            dl.AddRectFilled(screen + new Vector2(0, Theme.S(6f)),
                screen + new Vector2(Theme.S(2.5f), rowH - Theme.S(6f)), Theme.Accent, 2f);
        }

        var textX = start.X + RowPad + Theme.S(12f);
        Caret(dl, screen + new Vector2(Theme.S(6f), 0f), rowH, open);

        ImGui.SetCursorPos(new Vector2(textX, start.Y + (rowH - textH) * 0.5f));
        if (picked) ImGui.TextColored(Theme.V(Theme.TextBright), name);
        else ImGui.TextUnformatted(name);

        if (hasHint)
        {
            ImGui.SetCursorPosX(textX);
            ImGui.TextColored(Theme.V(Theme.Muted), hint);
        }

        var mark = picked ? "running" : "";
        if (mark.Length > 0)
        {
            ImGui.SetCursorPos(new Vector2(start.X + width - RowPad - ImGui.CalcTextSize(mark).X,
                start.Y + (rowH - lineH) * 0.5f));
            ImGui.TextColored(Theme.V(Theme.Accent), mark);
        }

        ImGui.SetCursorPos(new Vector2(start.X, start.Y + rowH));
        return hit;
    }
}
