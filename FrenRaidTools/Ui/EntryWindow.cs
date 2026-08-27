using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public sealed class EntryWindow : Window
{
    private readonly Plugin _plugin;
    private Configuration C => _plugin.Config;

    public EntryWindow(Plugin plugin) : base("Check In###frtentry")
    {
        _plugin = plugin;
        Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 100),
            MaximumSize = new Vector2(900, 1200),
        };
    }

    public void Open() => IsOpen = true;

    public override bool DrawConditions()
    {
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
        Theme.Skin = C.Skin;
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
        finally
        {
            Theme.PopWidgets();
        }
    }

    private void Body()
    {
        var now = _plugin.Now;
        var verdicts = RosterGlance.Verdicts(C, now);
        var you = Party.YouName();

        if (C.Setups.Count > 1)
        {
            var names = new string[C.Setups.Count];
            for (var i = 0; i < C.Setups.Count; i++)
            {
                var label = C.Setups[i].Name ?? "";
                names[i] = label.Length > 0 ? label : $"Group {i + 1}";
            }

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Theme.V(Theme.Muted), "Group");
            ImGui.SameLine(0, Theme.S(8f));

            var index = C.ActiveSetup;
            ImGui.SetNextItemWidth(Theme.S(200f));
            if (ImGui.Combo("##entrysetup", ref index, names, names.Length))
            {
                C.ActiveSetup = Math.Clamp(index, 0, C.Setups.Count - 1);
                C.Save(now);
            }

            ImGui.Spacing();
        }

        Widgets.ListBegin();
        for (var slot = 0; slot < Slots.Count; slot++)
            Row(C.Roles, slot, verdicts?[slot], you);
        Widgets.ListEnd();

        ImGui.Spacing();

        var seat = SeatSync.SeatFor(C);
        if (seat.Length > 0)
            ImGui.TextColored(Theme.V(Theme.Good), $"You are {seat}.");
        else
            ImGui.TextColored(Theme.V(Theme.Warn),
                "No spot has your name, so no call tells you where to stand.");

        ImGui.Spacing();

        if (Widgets.GhostButton("Roles page")) _plugin.MainWindow.Show(MainWindow.Nav.Roles);

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.AccentButton("OK")) IsOpen = false;
    }

    private static void Row(Roster roster, int slot, SpotVerdict? verdict, string you)
    {
        var name = roster.Players[slot].Trim();
        var isYou = name.Length > 0 && string.Equals(name, you, StringComparison.OrdinalIgnoreCase);

        var hint = verdict?.Check switch
        {
            SpotCheck.NearMiss => $"Did you mean {verdict.Value.Suggestion}?",
            SpotCheck.Absent => "Not in this party",
            _ => isYou ? "you" : "",
        };

        var (icon, iconColor) = verdict?.Check switch
        {
            SpotCheck.Confirmed => (FontAwesomeIcon.Check, Theme.Good),
            SpotCheck.NearMiss => (FontAwesomeIcon.ExclamationTriangle, Theme.Warn),
            SpotCheck.Absent => (FontAwesomeIcon.Times, Theme.Danger),
            _ => (FontAwesomeIcon.None, 0u),
        };

        var shown = name.Length > 0 ? name : "Empty";

        Widgets.RowBegin(Slots.Names[slot], hint,
            ImGui.CalcTextSize(shown).X + Theme.S(4f),
            icon: icon, iconColor: iconColor, id: "entry" + slot,
            edgeColor: JobLook.SlotColor(slot),
            hintColor: verdict?.Check switch
            {
                SpotCheck.NearMiss => Theme.Warn,
                SpotCheck.Absent => Theme.Danger,
                _ => 0u,
            });

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(
            Theme.V(name.Length == 0 ? Theme.Muted : isYou ? Theme.Accent : Theme.TextBright),
            shown);

        Widgets.RowEnd();
    }
}
