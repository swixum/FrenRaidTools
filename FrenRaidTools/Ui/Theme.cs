using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace FrenRaidTools.Ui;

internal static class Theme
{
    public const uint DefaultAccent = Violet;

    private const uint Blue = 0xFFF6823B;
    private const uint Amber = 0xFF3B88F0;
    private const uint Violet = 0xFFF56B9B;
    private const uint Teal = 0xFFA8C93B;
    private const uint Rose = 0xFF7A5CF0;

    public static uint Accent = DefaultAccent;
    public static float Scale = 1f;
    public static bool Colorblind;

    public static uint AccentHover => Lighten(Accent, 0.28f);
    public static uint AccentSoft => Wash(Accent, 0x2A);
    public static uint AccentFaint => Wash(Accent, 0x14);

    public const uint WindowBg = 0xFF120E0D;
    public const uint PopupBg = 0xFF1B1614;
    public const uint PanelBg = 0xFF14110E;
    public const uint ListBg = 0xFF110D0B;
    public const uint SubBg = 0xFF0F0C0A;
    public const uint Border = 0xFF2F2724;
    public const uint RowLine = 0xFF1F1916;
    public const uint TitleBg = 0xFF191311;
    public const uint TitleBgActive = 0xFF221A16;

    public const uint TextBright = 0xFFECE8E6;
    public const uint Muted = 0xFF81766E;
    public const uint Heading = 0xFFB0A398;
    public const uint SectionText = 0xFFB8A89E;
    public const uint NavText = 0xFFD1C4BD;
    public const uint OnAccent = WindowBg;

    public const uint FrameBg = 0xFF241D1A;
    public const uint ButtonBg = 0xFF30231F;
    public const uint HeaderBg = 0xFF34271F;
    public const uint TabBg = 0xFF2A211C;
    public const uint TabIdleBg = 0xFF241D1A;
    public const uint ScrollGrabBg = 0xFF382E2A;

    private const float HotMix = 0.30f;
    private const float HeldMix = 0.50f;

    public static uint FrameHot => Mix(FrameBg, Accent, HotMix);
    public static uint PanelHot => Mix(PanelBg, Accent, HotMix);

    public const uint CheckOn = 0xFF5AC832;
    public const uint CheckOnHover = 0xFF6FD647;
    public const uint CheckMark = 0xFFFFFFFF;

    public static uint Said => Lighten(Muted, 0.55f);

    public static uint Good => Colorblind ? 0xFF739E00 : 0xFF4FB45A;
    public static uint Warn => Colorblind ? 0xFF009FE6 : 0xFF3BC0F0;
    public static uint Danger => Colorblind ? 0xFFA779CC : 0xFF5050E0;

    public static readonly (string Name, uint Color)[] Swatches =
    [
        ("Blue", Blue),
        ("Amber", Amber),
        ("Violet", Violet),
        ("Teal", Teal),
        ("Rose", Rose),
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

    public static uint Mix(uint abgr, uint toward, float t)
    {
        uint Channel(int shift)
        {
            var from = (abgr >> shift) & 0xFF;
            var to = (toward >> shift) & 0xFF;
            return (uint)(from + (to - (float)from) * t) & 0xFF;
        }

        return (abgr & 0xFF000000) | (Channel(16) << 16) | (Channel(8) << 8) | Channel(0);
    }

    public static uint ReadableOn(uint abgr)
    {
        var r = abgr & 0xFF;
        var g = (abgr >> 8) & 0xFF;
        var b = (abgr >> 16) & 0xFF;
        return r * 299 + g * 587 + b * 114 > 140_000 ? OnAccent : TextBright;
    }

    private static readonly (ImGuiCol Col, uint Val)[] WindowColors =
    [
        (ImGuiCol.WindowBg, WindowBg),
        (ImGuiCol.PopupBg, PopupBg),
        (ImGuiCol.Border, Border),
        (ImGuiCol.TitleBg, TitleBg),
        (ImGuiCol.TitleBgActive, TitleBgActive),
        (ImGuiCol.TitleBgCollapsed, TitleBg),
        (ImGuiCol.ScrollbarBg, WindowBg),
    ];

    private static readonly (ImGuiCol Col, uint Val)[] WidgetColors =
    [
        (ImGuiCol.Text, TextBright),
        (ImGuiCol.TextDisabled, Muted),
        (ImGuiCol.ChildBg, 0x00000000),
        (ImGuiCol.FrameBg, FrameBg),
        (ImGuiCol.Button, ButtonBg),
        (ImGuiCol.Header, HeaderBg),
        (ImGuiCol.Tab, TabBg),
        (ImGuiCol.TabUnfocused, TabIdleBg),
        (ImGuiCol.Separator, Border),
        (ImGuiCol.ScrollbarGrab, ScrollGrabBg),
    ];

    private static readonly (ImGuiCol Col, uint Base)[] HotColors =
    [
        (ImGuiCol.FrameBgHovered, FrameBg),
        (ImGuiCol.ButtonHovered, ButtonBg),
        (ImGuiCol.HeaderHovered, HeaderBg),
        (ImGuiCol.TabHovered, TabBg),
        (ImGuiCol.SeparatorHovered, Border),
        (ImGuiCol.ScrollbarGrabHovered, ScrollGrabBg),
    ];

    private static readonly (ImGuiCol Col, uint Base)[] HeldColors =
    [
        (ImGuiCol.FrameBgActive, FrameBg),
        (ImGuiCol.ButtonActive, ButtonBg),
        (ImGuiCol.HeaderActive, HeaderBg),
        (ImGuiCol.TabActive, TabBg),
        (ImGuiCol.TabUnfocusedActive, TabIdleBg),
    ];

    private static readonly ImGuiCol[] AccentColors =
    [
        ImGuiCol.CheckMark, ImGuiCol.SliderGrab, ImGuiCol.SeparatorActive, ImGuiCol.ScrollbarGrabActive,
    ];

    private static readonly (ImGuiStyleVar Var, float Val)[] WindowVars =
    [
        (ImGuiStyleVar.WindowRounding, 9f),
        (ImGuiStyleVar.WindowBorderSize, 1f),
        (ImGuiStyleVar.ChildRounding, 8f),
        (ImGuiStyleVar.PopupRounding, 7f),
    ];

    private static readonly (ImGuiStyleVar Var, float Val)[] WidgetVars =
    [
        (ImGuiStyleVar.FrameRounding, 5f),
        (ImGuiStyleVar.GrabRounding, 4f),
        (ImGuiStyleVar.TabRounding, 5f),
        (ImGuiStyleVar.ScrollbarRounding, 6f),
    ];

    private static readonly (ImGuiStyleVar Var, Vector2 Val)[] WidgetPads =
    [
        (ImGuiStyleVar.FramePadding, new Vector2(9, 5)),
        (ImGuiStyleVar.ItemSpacing, new Vector2(8, 6)),
        (ImGuiStyleVar.ItemInnerSpacing, new Vector2(6, 4)),
    ];

    public static void PushWindow()
    {
        foreach (var (col, val) in WindowColors) ImGui.PushStyleColor(col, val);
        foreach (var (var, val) in WindowVars) ImGui.PushStyleVar(var, val);
    }

    public static void PopWindow()
    {
        ImGui.PopStyleVar(WindowVars.Length);
        ImGui.PopStyleColor(WindowColors.Length);
    }

    public static void PushWidgets()
    {
        foreach (var (col, val) in WidgetColors) ImGui.PushStyleColor(col, val);
        foreach (var (col, seed) in HotColors) ImGui.PushStyleColor(col, Mix(seed, Accent, HotMix));
        foreach (var (col, seed) in HeldColors) ImGui.PushStyleColor(col, Mix(seed, Accent, HeldMix));
        foreach (var col in AccentColors) ImGui.PushStyleColor(col, Accent);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, AccentHover);

        foreach (var (var, val) in WidgetVars) ImGui.PushStyleVar(var, val * Scale);
        foreach (var (var, val) in WidgetPads) ImGui.PushStyleVar(var, val * Scale);
    }

    public static void PopWidgets()
    {
        ImGui.PopStyleVar(WidgetVars.Length + WidgetPads.Length);
        ImGui.PopStyleColor(WidgetColors.Length + HotColors.Length + HeldColors.Length
                            + AccentColors.Length + 1);
    }
}
