using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private void DrawStatusStrip()
    {
        var here = Game.InTheFight;
        Widgets.WindowHeader("FREN RAID TOOLS", _plugin.Version);

        var toggle = C.CallsOn ? "Calls on" : "Calls off";
        const string testLabel = "Test";
        var right = Widgets.ButtonWidth(toggle, testLabel) + Theme.S(10f);
        ImGui.SameLine(MathF.Max(ImGui.GetCursorPosX(), ImGui.GetContentRegionMax().X - right));

        if (Widgets.SegmentTall(toggle, C.CallsOn))
        {
            C.CallsOn = !C.CallsOn;
            Touch();
        }
        Widgets.Tip(C.CallsOn ? "Turn calls off" : "Turn calls on");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton(testLabel)) _plugin.FireSample();
        Widgets.Tip("Sample call on the overlay");

        ImGui.Spacing();

        Widgets.Chip("Zone", here ? "Dancing Mad" : Game.ZoneName(), here ? Theme.Accent : Theme.Muted);
        ImGui.SameLine(0, Theme.S(6f));

        var loaded = Board.Shown.Count;
        Widgets.Chip("Calls", loaded > 0 ? $"{loaded} ready" : "none yet",
            loaded > 0 ? Theme.Good : Theme.Warn);
        ImGui.SameLine(0, Theme.S(6f));

        var live = Board.LiveCount;
        Widgets.Chip("On screen", live > 0 ? $"{live} up" : "nothing", live > 0 ? Theme.Accent : Theme.Muted);
        ImGui.SameLine(0, Theme.S(6f));

        var roles = C.Roles.Filled;
        Widgets.Chip("Roles", $"{roles}/8", roles == 8 ? Theme.Good : roles == 0 ? Theme.Muted : Theme.Warn);
    }

    private void DrawSidebar()
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Theme.PanelBg);
        if (ImGui.BeginChild("##nav", new Vector2(Theme.S(SidebarWidth), 0), true))
        {
            ImGui.Dummy(new Vector2(0, Theme.S(4f)));

            NavRow(Nav.Home, "Home", FontAwesomeIcon.Home, "");

            NavGroup("Fights");
            foreach (var category in FightCategories())
            {
                var count = FrenRaidTools.Engine.FightPlans.All.Count(f => f.Category == category);
                var picked = _nav is Nav.Fights or Nav.Calls
                             && _navCategory == category && _search.Length == 0;
                if (!NavPick(CategoryIcon(category), category, picked, count.ToString())) continue;
                _nav = Nav.Fights;
                _navCategory = category;
                _search = "";
            }

            NavGroup("For the fight");
            NavRow(Nav.Roles, "Roles", FontAwesomeIcon.Users, $"{C.Roles.Filled}/8");
            NavRow(Nav.Strats, "Strats", FontAwesomeIcon.Sitemap, "");

            NavGroup("On screen");
            NavRow(Nav.Overlay, "Overlay", FontAwesomeIcon.Desktop, C.OverlayOn ? "" : "off");
            NavRow(Nav.Voice, "Voice", FontAwesomeIcon.VolumeUp, C.TtsOn ? "" : "off");

            NavGroup("Setup");
            NavRow(Nav.Parser, "Parser", FontAwesomeIcon.Plug, "", _plugin.Parser.Dot);
            NavRow(Nav.Diagnostics, "Diagnostics", FontAwesomeIcon.Stethoscope, "",
                mark: _plugin.Diag.On ? FontAwesomeIcon.Eye : FontAwesomeIcon.None,
                markColor: Theme.Good);
            NavRow(Nav.Appearance, "Appearance", FontAwesomeIcon.Palette, "");
        }
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    private string _navCategory = "Ultimate";

    private static IEnumerable<string> FightCategories()
    {
        var seen = new List<string>();
        foreach (var fight in FrenRaidTools.Engine.FightPlans.All)
            if (!seen.Contains(fight.Category)) seen.Add(fight.Category);
        return seen;
    }

    private static FontAwesomeIcon CategoryIcon(string category) => category switch
    {
        "Ultimate" => FontAwesomeIcon.Crown,
        "Savage" => FontAwesomeIcon.SkullCrossbones,
        "Extreme" => FontAwesomeIcon.Fire,
        _ => FontAwesomeIcon.Dungeon,
    };

    private bool NavPick(FontAwesomeIcon icon, string label, bool picked, string badge)
    {
        var height = ImGui.GetFrameHeight() + Theme.S(6f);
        var width = ImGui.GetContentRegionAvail().X;
        var start = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton("##pick" + label, new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        var dl = ImGui.GetWindowDrawList();
        if (picked || hovered)
            dl.AddRectFilled(start, start + new Vector2(width, height),
                picked ? Theme.AccentSoft : Theme.RowHover, Theme.S(5f));
        if (picked)
            dl.AddRectFilled(start + new Vector2(0, Theme.S(5f)),
                start + new Vector2(Theme.S(2.5f), height - Theme.S(5f)), Theme.Accent, 2f);

        var iconX = start.X + Theme.S(12f);
        using (Service.PluginInterface.UiBuilder.IconFontHandle.Push())
        {
            var glyph = icon.ToIconString();
            var size = ImGui.CalcTextSize(glyph);
            dl.AddText(new Vector2(iconX, start.Y + (height - size.Y) * 0.5f),
                picked ? Theme.Accent : Theme.Muted, glyph);
        }

        var textSize = ImGui.CalcTextSize(label);
        dl.AddText(new Vector2(iconX + Theme.S(24f), start.Y + (height - textSize.Y) * 0.5f),
            picked ? Theme.TextBright : Theme.NavText, label);

        if (badge.Length > 0)
        {
            var size = ImGui.CalcTextSize(badge);
            dl.AddText(new Vector2(start.X + width - Theme.S(10f) - size.X,
                start.Y + (height - size.Y) * 0.5f), Theme.Muted, badge);
        }

        return clicked;
    }

    private static void Fold(string id, string title, string badge, uint badgeColor,
        HashSet<string> remembered, bool openByDefault, Action body, bool forceOpen = false,
        float controlWidth = 0f, Action? controls = null)
    {
        var open = forceOpen || openByDefault != remembered.Contains(id);
        var was = open;

        if (Widgets.FoldBegin(id, title, badge, badgeColor, ref open, controlWidth, controls)) body();
        Widgets.FoldEnd();

        if (forceOpen || open == was) return;
        if (open == openByDefault) remembered.Remove(id);
        else remembered.Add(id);
    }

    private static void NavGroup(string label)
    {
        ImGui.Dummy(new Vector2(0, Theme.S(7f)));
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Theme.S(3f));
        ImGui.TextColored(Theme.V(Theme.Muted), label.ToUpperInvariant());
        ImGui.Dummy(new Vector2(0, Theme.S(2f)));
    }

    private const float MarkScale = 0.68f;

    private void NavRow(Nav page, string label, FontAwesomeIcon icon, string badge, uint dot = 0,
        FontAwesomeIcon mark = FontAwesomeIcon.None, uint markColor = 0)
    {
        var on = _nav == page && _search.Length == 0;
        var height = ImGui.GetFrameHeight() + Theme.S(6f);
        var width = ImGui.GetContentRegionAvail().X;
        var start = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton("##nav" + label, new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        var dl = ImGui.GetWindowDrawList();
        if (on || hovered)
            dl.AddRectFilled(start, start + new Vector2(width, height),
                on ? Theme.AccentSoft : Theme.RowHover, Theme.S(5f));
        if (on)
            dl.AddRectFilled(start + new Vector2(0, Theme.S(5f)),
                start + new Vector2(Theme.S(2.5f), height - Theme.S(5f)), Theme.Accent, 2f);

        var iconX = start.X + Theme.S(12f);
        using (Service.PluginInterface.UiBuilder.IconFontHandle.Push())
        {
            var glyph = icon.ToIconString();
            var size = ImGui.CalcTextSize(glyph);
            dl.AddText(new Vector2(iconX, start.Y + (height - size.Y) * 0.5f),
                on ? Theme.Accent : Theme.Muted, glyph);
        }

        var textSize = ImGui.CalcTextSize(label);
        dl.AddText(new Vector2(iconX + Theme.S(24f), start.Y + (height - textSize.Y) * 0.5f),
            on ? Theme.TextBright : Theme.NavText, label);

        var markMiddle = new Vector2(start.X + width - Theme.S(14f), start.Y + height * 0.5f);

        if (mark != FontAwesomeIcon.None)
        {
            using (Service.PluginInterface.UiBuilder.IconFontHandle.Push())
            {
                var glyph = mark.ToIconString();
                var full = ImGui.GetFontSize();
                var small = full * MarkScale;
                var size = ImGui.CalcTextSize(glyph) * (small / full);

                dl.AddText(ImGui.GetFont(), small, markMiddle - size * 0.5f,
                    markColor == 0 ? Theme.Muted : markColor, glyph);
            }
        }
        else if (badge.Length > 0)
        {
            var badgeSize = ImGui.CalcTextSize(badge);
            dl.AddText(new Vector2(start.X + width - Theme.S(10f) - badgeSize.X,
                start.Y + (height - badgeSize.Y) * 0.5f), Theme.Muted, badge);
        }
        else if (dot != 0)
        {
            dl.AddCircleFilled(markMiddle, Theme.S(6f), Theme.Wash(dot, 0x33));
            dl.AddCircleFilled(markMiddle, Theme.S(3.2f), dot);
        }

        if (!clicked) return;
        _nav = page;
        _search = "";
    }
}
