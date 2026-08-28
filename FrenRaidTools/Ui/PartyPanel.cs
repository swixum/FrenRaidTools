using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

internal sealed class PartyPanel
{
    private readonly string _id;
    private string _note = "";
    private double _noteUntil;

    public PartyPanel(string id) => _id = id;

    private string ClearPopup => "##clear" + _id;

    public void Forget()
    {
        _note = "";
        _noteUntil = 0;
    }

    public void Draw(Configuration config, double now)
    {
        var you = Party.YouName();
        var verdicts = RosterGlance.Verdicts(config, now);

        DrawGroupPicker(config, now);

        var glance = RosterGlance.Members(now);
        var duplicates = config.Roles.Duplicates();

        if (duplicates.Count > 0)
            Widgets.Banner(Theme.Danger, "Same name in two spots. Calls take the first.");

        Widgets.ListBegin();
        for (var slot = 0; slot < Slots.Count; slot++)
            if (RoleRows.Row(config.Roles, slot, _id + slot, duplicates, you, verdicts?[slot], glance))
                config.Save(now);
        Widgets.ListEnd();

        ImGui.Spacing();

        if (Widgets.GhostButton("Fill blanks")) FillBlanks(config, now);
        Widgets.Tip("Fill the empty spots only");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.DangerButton("Clear all")) ImGui.OpenPopup(ClearPopup);
        Widgets.Tip("Empty all eight spots");

        if (Widgets.ConfirmPopup(ClearPopup, "Empty all eight spots?", "Clear all", "Keep them"))
        {
            config.Roles.Clear();
            config.Save(now);
            Say(now, "Cleared all eight spots.");
        }

        if (now < _noteUntil)
        {
            ImGui.SameLine(0, Theme.S(8f));
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Theme.V(Theme.Muted), _note);
        }

        ImGui.Spacing();

        var seat = SeatSync.SeatFor(config);
        if (seat.Length > 0)
            ImGui.TextColored(Theme.V(Theme.Good), $"You are {seat}.");
        else
            ImGui.TextColored(Theme.V(Theme.Warn),
                "Your name is not in a spot");
    }

    private void DrawGroupPicker(Configuration config, double now)
    {
        if (config.Setups.Count <= 1) return;

        var names = new string[config.Setups.Count];
        for (var i = 0; i < config.Setups.Count; i++)
        {
            var label = config.Setups[i].Name ?? "";
            names[i] = label.Length > 0 ? label : $"Group {i + 1}";
        }

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Theme.V(Theme.Muted), "Group");
        ImGui.SameLine(0, Theme.S(8f));

        var index = config.ActiveSetup;
        ImGui.SetNextItemWidth(Theme.S(200f));
        if (ImGui.Combo("##group" + _id, ref index, names, names.Length))
        {
            config.ActiveSetup = Math.Clamp(index, 0, config.Setups.Count - 1);
            config.Save(now);
        }

        ImGui.Spacing();
    }

    private void FillBlanks(Configuration config, double now)
    {
        var members = Party.Read();

        if (members.Count == 0)
        {
            Say(now, "No party to read.");
            return;
        }

        var placed = config.Roles.Fill(members, keepExisting: true, config.JobSpots, Party.YouName());
        Say(now, placed > 0 ? $"Set {TextLines.Spots(placed)}." : "Nothing left to fill.");

        if (placed > 0) config.Save(now);
    }

    private void Say(double now, string text)
    {
        _note = text;
        _noteUntil = now + NoteSeconds;
    }

    private const double NoteSeconds = 5.0;
}
