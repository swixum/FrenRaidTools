using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private PlannedFight PickedFight => FightPlans.ByKey(C.PlanFight) ?? FightPlans.First;

    private StrategyPick Plan => C.PlanFor(PickedFight.Key);

    private void DrawFightStrats(PlannedFight fight)
    {
        var asset = PlanSource.Asset(fight);

        if (asset is null)
        {
            if (PlanSource.Loading(fight)) Widgets.RowNote("Reading the strats.");
            else Widgets.RowNoteWrap($"The strats would not load. {PlanSource.Fault(fight)}",
                Theme.Danger);
            return;
        }

        if (asset.Options.Count > 0)
        {
            Widgets.SectionHeader("NA Party Finder Strats");
            foreach (var option in asset.Options) DrawOptionRow(option);
        }

        DrawSeatNudge();

        DrawDirections(asset);
    }

    private void DrawSeatNudge()
    {
        if (SeatSync.SeatFor(C).Length > 0) return;

        Widgets.SectionHeader("Your spot");

        Widgets.RowNote(C.Roles.Filled == 0
            ? "No names on the Roles page yet"
            : "Your name is not in a spot on the Roles page", Theme.Warn);

        if (Widgets.RowDoor("Open Roles", "Put yourself in a spot")) Show(Nav.Roles);
    }

    private void DrawOptionRow(StrategyOption option)
    {
        var choices = option.Options;
        var chosen = Plan.Value(option.Key) ?? option.DefaultValue;
        var index = 0;
        for (var i = 0; i < choices.Count; i++)
            if (choices[i].Value == chosen) index = i;

        var pick = choices.Count > index ? choices[index] : null;
        var label = LabelText.Of(option.Label);
        var difference = pick?.Difference ?? "";
        var hint = Gist(difference);

        if (choices.Count == 1)
        {
            Widgets.RowValue(label, hint, pick?.Label ?? "", Theme.Accent,
                sub: true, id: "opt" + option.Key, tip: difference);
            return;
        }

        var labels = new string[choices.Count];
        for (var i = 0; i < choices.Count; i++) labels[i] = choices[i].Label;

        if (Widgets.RowCombo(label, hint, ref index, labels, sub: true))
        {
            Plan.Set(option.Key, choices[index].Value);
            Touch();
        }

        Widgets.Tip(difference);
    }

    private static string Gist(string text)
    {
        var stop = text.IndexOf(". ", StringComparison.Ordinal);
        return (stop < 0 ? text : text[..stop]).TrimEnd('.', ' ');
    }
}
