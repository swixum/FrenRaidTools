using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;

namespace FrenRaidTools.Ui;

public partial class MainWindow : Window
{
    public enum Nav
    {
        Home,
        Fights,
        Calls,
        Roles,
        Strats,
        Overlay,
        Voice,
        Parser,
        Diagnostics,
        Appearance,
    }

    private readonly Plugin _plugin;
    private Configuration C => _plugin.Config;
    private CallBoard Board => _plugin.Board;
    private Fight Fight => _plugin.Fight;

    private Nav _nav = Nav.Home;
    private string _search = "";
    private double _now;

    private const float SidebarWidth = 178f;

    public MainWindow(Plugin plugin)
        : base("Fren Raid Tools###frtmain")
    {
        _plugin = plugin;
        Size = new Vector2(840, 660);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(640, 440),
            MaximumSize = new Vector2(2400, 1800),
        };
    }

    private void Touch() => C.Save(_now);

    public void Show(Nav page)
    {
        _nav = page;
        _search = "";
        IsOpen = true;
    }

    public override void PreDraw()
    {
        Theme.Accent = C.AccentColor;
        Theme.Scale = Math.Clamp(C.UiScale, 0.8f, 1.6f);
        Theme.Colorblind = C.Colorblind;
        Theme.PushWindow();
    }

    public override void PostDraw() => Theme.PopWindow();

    public override void Draw()
    {
        _now = _plugin.Now;
        Theme.PushWidgets();
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, Theme.S(15f));

        try
        {
            DrawStatusStrip();
            ImGui.Separator();

            var footer = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y * 2f;
            if (ImGui.BeginChild("##body", new Vector2(0, -footer), false))
            {
                DrawSidebar();
                ImGui.SameLine(0, Theme.S(10f));
                DrawPage();
            }
            ImGui.EndChild();

            DrawFooter();
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Window draw failed.");
        }
        finally
        {
            ImGui.PopStyleVar();
            Theme.PopWidgets();
        }
    }

    private void DrawPage()
    {
        if (!ImGui.BeginChild("##page", new Vector2(0, 0), false))
        {
            ImGui.EndChild();
            return;
        }

        try
        {
            if (_search.Length > 0) DrawCalls();
            else
                switch (_nav)
                {
                    case Nav.Home: DrawHome(); break;
                    case Nav.Fights: DrawFightCategory(); break;
                    case Nav.Calls: DrawCalls(); break;
                    case Nav.Roles: DrawRoles(); break;
                    case Nav.Strats: DrawStrats(); break;
                    case Nav.Overlay: DrawOverlaySettings(); break;
                    case Nav.Voice: DrawVoice(); break;
                    case Nav.Parser: DrawParser(); break;
                    case Nav.Diagnostics: DrawDiagnosticsPage(); break;
                    case Nav.Appearance: DrawAppearance(); break;
                }
        }
        catch (Exception ex)
        {
            Widgets.ListAbort();
            Service.Log.Error(ex, "Page draw failed.");
            ImGui.TextColored(Theme.V(Theme.Danger), "This page failed to draw. See the log.");
        }

        ImGui.EndChild();
    }

    private void DrawFooter()
    {
        ImGui.Separator();
        ImGui.AlignTextToFramePadding();

        var room = ImGui.GetContentRegionAvail().X - Theme.S(60f);
        var notice = Board.Notice;
        var fault = Board.LastFault;

        if (notice.Length > 0) ImGui.TextColored(Theme.V(Theme.Accent), Widgets.Elide(notice, room));
        else if (fault.Length > 0) ImGui.TextColored(Theme.V(Theme.Warn), Widgets.Elide(fault, room));
        else ImGui.TextColored(Theme.V(Theme.Muted), "Saved automatically");

        const string hint = "/frt";
        ImGui.SameLine(MathF.Max(ImGui.GetCursorPosX(),
            ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(hint).X - Theme.S(4f)));
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(Theme.Muted), hint);
    }

    private void PageHeader(string title, string detail)
    {
        Widgets.WindowHeader(title, detail);
        ImGui.Spacing();
    }
}
