using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace FrenRaidTools.Ui;

internal sealed class Palette
{
    public required uint WindowBg { get; init; }
    public required uint PanelBg { get; init; }
    public required uint ListBg { get; init; }
    public required uint SubBg { get; init; }
    public required uint PopupBg { get; init; }
    public required uint Border { get; init; }
    public required uint RowLine { get; init; }
    public required uint RowHover { get; init; }
    public required uint TitleBg { get; init; }
    public required uint TitleBgActive { get; init; }
    public required uint TextBright { get; init; }
    public required uint Muted { get; init; }
    public required uint Heading { get; init; }
    public required uint FrameBg { get; init; }
    public required uint FrameHover { get; init; }
    public required uint FrameActive { get; init; }
    public required uint Button { get; init; }
    public required uint ButtonHover { get; init; }
    public required uint ButtonActive { get; init; }
    public required uint Header { get; init; }
    public required uint HeaderHover { get; init; }
    public required uint HeaderActive { get; init; }
    public required uint Tab { get; init; }
    public required uint Separator { get; init; }
    public required uint ScrollGrab { get; init; }
    public required uint ScrollGrabHover { get; init; }
}

internal static class Theme
{
    public const uint DefaultAccent = Ice;

    private const uint Ice = 0xFFE8C24C;
    private const uint Teal = 0xFFA8D614;

    public static uint Accent = DefaultAccent;
    public static float Scale = 1f;
    public static bool Colorblind;
    public static int Skin;

    public static uint AccentHover => Lighten(Accent, 0.26f);
    public static uint AccentSoft => Wash(Accent, 0x2A);
    public static uint AccentFaint => Wash(Accent, 0x14);

    public static readonly string[] SkinNames = ["Black", "Purple", "Warm"];

    private static readonly Palette Black = new()
    {
        WindowBg = 0xFF0A0A0A,
        PanelBg = 0xFF111111,
        ListBg = 0xFF101010,
        SubBg = 0xFF0E0E0E,
        PopupBg = 0xFF161616,
        Border = 0xFF303030,
        RowLine = 0xFF242424,
        RowHover = 0xFF1B1B1B,
        TitleBg = 0xFF0D0D0D,
        TitleBgActive = 0xFF161616,
        TextBright = 0xFFE8E8E8,
        Muted = 0xFF8C8C8C,
        Heading = 0xFFB7B7B7,
        FrameBg = 0xFF1A1A1A,
        FrameHover = 0xFF242424,
        FrameActive = 0xFF2E2E2E,
        Button = 0xFF1F1F1F,
        ButtonHover = 0xFF2B2B2B,
        ButtonActive = 0xFF383838,
        Header = 0xFF242424,
        HeaderHover = 0xFF303030,
        HeaderActive = 0xFF3C3C3C,
        Tab = 0xFF1B1B1B,
        Separator = 0xFF262626,
        ScrollGrab = 0xFF333333,
        ScrollGrabHover = 0xFF454545,
    };

    private static readonly Palette Purple = new()
    {
        WindowBg = 0xFF150A0F,
        PanelBg = 0xFF211017,
        ListBg = 0xFF1D0E14,
        SubBg = 0xFF190B11,
        PopupBg = 0xFF26121A,
        Border = 0xFF40212C,
        RowLine = 0xFF341A24,
        RowHover = 0xFF29141D,
        TitleBg = 0xFF1B0D13,
        TitleBgActive = 0xFF29141D,
        TextBright = 0xFFF0E4E9,
        Muted = 0xFF936E7B,
        Heading = 0xFFC49BA9,
        FrameBg = 0xFF29151D,
        FrameHover = 0xFF381D28,
        FrameActive = 0xFF472532,
        Button = 0xFF341922,
        ButtonHover = 0xFF46222E,
        ButtonActive = 0xFF582C3A,
        Header = 0xFF3C1B26,
        HeaderHover = 0xFF4F2533,
        HeaderActive = 0xFF622F40,
        Tab = 0xFF30161E,
        Separator = 0xFF40212C,
        ScrollGrab = 0xFF582C3A,
        ScrollGrabHover = 0xFF6E3A4A,
    };

    private static readonly Palette Warm = new()
    {
        WindowBg = 0xFF13100E,
        PanelBg = 0xFF1D1916,
        ListBg = 0xFF1A1613,
        SubBg = 0xFF171310,
        PopupBg = 0xFF231E1A,
        Border = 0xFF382F29,
        RowLine = 0xFF2B2520,
        RowHover = 0xFF241F1B,
        TitleBg = 0xFF181411,
        TitleBgActive = 0xFF241F1B,
        TextBright = 0xFFEDE7E2,
        Muted = 0xFF8C8078,
        Heading = 0xFFB8ACA3,
        FrameBg = 0xFF241F1B,
        FrameHover = 0xFF322B25,
        FrameActive = 0xFF40372F,
        Button = 0xFF2A2420,
        ButtonHover = 0xFF3A322B,
        ButtonActive = 0xFF4B4138,
        Header = 0xFF322B25,
        HeaderHover = 0xFF443A32,
        HeaderActive = 0xFF554940,
        Tab = 0xFF221D1A,
        Separator = 0xFF382F29,
        ScrollGrab = 0xFF453B33,
        ScrollGrabHover = 0xFF594D43,
    };

    private static Palette P => Skin switch
    {
        1 => Purple,
        2 => Warm,
        _ => Black,
    };

    public static uint WindowBg => P.WindowBg;
    public static uint PanelBg => P.PanelBg;
    public static uint ListBg => P.ListBg;
    public static uint SubBg => P.SubBg;
    public static uint Border => P.Border;
    public static uint RowLine => P.RowLine;
    public static uint RowHover => P.RowHover;
    public static uint TextBright => P.TextBright;
    public static uint Muted => P.Muted;
    public static uint Heading => P.Heading;
    public static uint Said => Lighten(P.Muted, 0.55f);
    public static uint OnAccent => P.WindowBg;

    public static uint Good => Colorblind ? 0xFF9BD64F : 0xFF7ED13F;
    public static uint Warn => Colorblind ? 0xFF2E9FE0 : 0xFF3CB2F2;
    public static uint Danger => Colorblind ? 0xFFB07AD8 : 0xFF5B54F2;

    public static readonly (string Name, uint Color)[] Swatches =
    [
        ("Ice", Ice),
        ("Teal", Teal),
        ("Mint", 0xFF9BE86A),
        ("Plum", 0xFFD86AB0),
        ("Ember", 0xFF4C7CF2),
        ("Bone", 0xFFC8CFD2),
    ];

    public static float S(float px) => px * Scale;

    public static Vector4 V(uint abgr) => new(
        (abgr & 0xFF) / 255f,
        ((abgr >> 8) & 0xFF) / 255f,
        ((abgr >> 16) & 0xFF) / 255f,
        ((abgr >> 24) & 0xFF) / 255f);

    public static uint Pack(Vector4 v) =>
        ((uint)(Math.Clamp(v.W, 0f, 1f) * 255f) << 24) |
        ((uint)(Math.Clamp(v.Z, 0f, 1f) * 255f) << 16) |
        ((uint)(Math.Clamp(v.Y, 0f, 1f) * 255f) << 8) |
        (uint)(Math.Clamp(v.X, 0f, 1f) * 255f);

    public static uint Lighten(uint abgr, float t)
    {
        uint Channel(int shift)
        {
            var c = (abgr >> shift) & 0xFF;
            return (uint)(c + (255 - c) * t) & 0xFF;
        }

        return (abgr & 0xFF000000) | (Channel(16) << 16) | (Channel(8) << 8) | Channel(0);
    }

    public static uint Fade(uint abgr, float alpha) =>
        (abgr & 0x00FFFFFF) | ((uint)(Math.Clamp(alpha, 0f, 1f) * 255f) << 24);

    public static uint Wash(uint abgr, uint alpha) => (abgr & 0x00FFFFFF) | (alpha << 24);

    public static uint ReadableOn(uint abgr)
    {
        var r = abgr & 0xFF;
        var g = (abgr >> 8) & 0xFF;
        var b = (abgr >> 16) & 0xFF;
        return r * 299 + g * 587 + b * 114 > 140_000 ? OnAccent : TextBright;
    }

    private const int WindowColorCount = 7;
    private const int WidgetColorCount = 26;

    private static readonly (ImGuiStyleVar Var, float Val)[] WindowVars =
    [
        (ImGuiStyleVar.WindowRounding, 8f),
        (ImGuiStyleVar.WindowBorderSize, 1f),
        (ImGuiStyleVar.ChildRounding, 7f),
        (ImGuiStyleVar.PopupRounding, 7f),
    ];

    private static readonly (ImGuiStyleVar Var, float Val)[] WidgetVars =
    [
        (ImGuiStyleVar.FrameRounding, 4f),
        (ImGuiStyleVar.GrabRounding, 4f),
        (ImGuiStyleVar.TabRounding, 5f),
        (ImGuiStyleVar.ScrollbarRounding, 8f),
    ];

    private static readonly (ImGuiStyleVar Var, Vector2 Val)[] WidgetPads =
    [
        (ImGuiStyleVar.FramePadding, new Vector2(9, 5)),
        (ImGuiStyleVar.ItemSpacing, new Vector2(8, 6)),
        (ImGuiStyleVar.ItemInnerSpacing, new Vector2(6, 4)),
    ];

    public static void PushWindow()
    {
        var p = P;
        ImGui.PushStyleColor(ImGuiCol.WindowBg, p.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, p.PopupBg);
        ImGui.PushStyleColor(ImGuiCol.Border, p.Border);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, p.TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, p.TitleBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, p.TitleBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, p.WindowBg);

        foreach (var (v, val) in WindowVars) ImGui.PushStyleVar(v, val);
    }

    public static void PopWindow()
    {
        ImGui.PopStyleVar(WindowVars.Length);
        ImGui.PopStyleColor(WindowColorCount);
    }

    public static void PushWidgets()
    {
        var p = P;
        ImGui.PushStyleColor(ImGuiCol.Text, p.TextBright);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, p.Muted);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0x00000000);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, p.FrameBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, p.FrameHover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, p.FrameActive);
        ImGui.PushStyleColor(ImGuiCol.Button, p.Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, p.ButtonHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, p.ButtonActive);
        ImGui.PushStyleColor(ImGuiCol.Header, p.Header);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, p.HeaderHover);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, p.HeaderActive);
        ImGui.PushStyleColor(ImGuiCol.Tab, p.Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, p.HeaderHover);
        ImGui.PushStyleColor(ImGuiCol.TabActive, p.HeaderActive);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, p.FrameBg);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, p.FrameActive);
        ImGui.PushStyleColor(ImGuiCol.Separator, p.Separator);
        ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, p.HeaderHover);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, p.ScrollGrab);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, p.ScrollGrabHover);

        ImGui.PushStyleColor(ImGuiCol.CheckMark, Accent);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, Accent);
        ImGui.PushStyleColor(ImGuiCol.SeparatorActive, Accent);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, Accent);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, AccentHover);

        foreach (var (v, val) in WidgetVars) ImGui.PushStyleVar(v, val * Scale);
        foreach (var (v, val) in WidgetPads) ImGui.PushStyleVar(v, val * Scale);
    }

    public static void PopWidgets()
    {
        ImGui.PopStyleVar(WidgetVars.Length + WidgetPads.Length);
        ImGui.PopStyleColor(WidgetColorCount);
    }
}
