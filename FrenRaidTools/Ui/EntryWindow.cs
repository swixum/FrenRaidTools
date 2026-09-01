using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace FrenRaidTools.Ui;

public sealed class EntryWindow : Window
{
    private readonly Plugin _plugin;
    private readonly PartyPanel _panel = new("entry");
    private Configuration C => _plugin.Config;

    public EntryWindow(Plugin plugin) : base("Check In###frtentry")
    {
        _plugin = plugin;
        Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;
    }

    public void Open()
    {
        _panel.Forget();
        IsOpen = true;
    }

    public override bool DrawConditions()
    {
        if (Service.GameGui.GameUiHidden) return false;
        if (!Game.InTheFight)
        {
            IsOpen = false;
            return false;
        }

        return !Game.InFight;
    }

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
            Service.Log.Error(ex, "Check In draw failed.");
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

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.AccentButton("Done")) IsOpen = false;
    }
}
