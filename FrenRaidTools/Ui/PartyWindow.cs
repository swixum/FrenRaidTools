using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace FrenRaidTools.Ui;

public sealed class PartyWindow : Window
{
    private readonly Plugin _plugin;
    private readonly PartyPanel _panel = new("party");
    private Configuration C => _plugin.Config;

    public PartyWindow(Plugin plugin) : base("Party###frtparty")
    {
        _plugin = plugin;
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public void Open()
    {
        _panel.Forget();
        IsOpen = true;
    }

    public override bool DrawConditions() => !Service.GameGui.GameUiHidden;

    public override void PreDraw()
    {
        Theme.Accent = C.AccentColor;
        Theme.Scale = Math.Clamp(C.UiScale, 0.8f, 1.6f);
        Theme.Colorblind = C.Colorblind;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(Theme.S(470f), Theme.S(100f)),
            MaximumSize = new Vector2(Theme.S(720f), Theme.S(900f)),
        };

        Theme.PushWindow();
    }

    public override void PostDraw() => Theme.PopWindow();

    public override void Draw()
    {
        Theme.PushWidgets();
        try
        {
            Body();
        }
        catch (Exception ex)
        {
            Widgets.ListAbort();
            Service.Log.Error(ex, "Party draw failed.");
        }
        finally
        {
            Theme.PopWidgets();
        }
    }

    private void Body()
    {
        _panel.Draw(C, _plugin.Now);

        ImGui.Spacing();

        if (Widgets.GhostButton("Open Roles")) _plugin.MainWindow.Show(MainWindow.Nav.Roles);
    }
}
