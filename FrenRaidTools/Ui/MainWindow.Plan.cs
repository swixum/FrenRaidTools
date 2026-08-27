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

        Widgets.SectionHeader("Your spot");

        DrawSeatRow(asset, fight);
        DrawAlignmentRow(asset);

        DrawDirections(asset);
    }

    private void DrawSeatRow(StrategyAsset asset, PlannedFight fight)
    {
        var fromRoles = C.SeatFromRoles;
        if (Widgets.RowCheckClick("Use my spot from Roles",
                "Whoever you are on the Roles page", ref fromRoles,
                id: "seatroles" + fight.Key, sub: true))
        {
            C.SeatFromRoles = fromRoles;
            SeatSync.Reset();
            Touch();
        }

        var mine = SeatSync.SeatFor(C);

        if (fromRoles)
        {
            if (mine.Length > 0)
            {
                Widgets.RowValue("Your spot", SeatHint(asset, mine), mine, Theme.Accent,
                    sub: true, id: "seatfromroles" + fight.Key);
                return;
            }

            Widgets.RowNote(C.Roles.Filled == 0
                ? "Nobody is on the Roles page yet."
                : "You are not in a spot on the Roles page.", Theme.Warn);

            if (Widgets.RowDoor("Open Roles", "Put yourself in a spot")) Show(Nav.Roles);
            return;
        }

        var seats = asset.Seats;
        var labels = new string[seats.Count + 1];
        labels[0] = "Not set";
        for (var i = 0; i < seats.Count; i++)
            labels[i + 1] = $"{seats[i].Key}  {seats[i].Role} {seats[i].Party}";

        var index = 0;
        for (var i = 0; i < seats.Count; i++)
            if (seats[i].Key == Plan.Seat) index = i + 1;

        if (Widgets.RowCombo("Your spot", "Which of the eight you play",
                ref index, labels, sub: true))
        {
            Plan.Seat = index == 0 ? "" : seats[index - 1].Key;
            Touch();
        }

        if (Plan.Seat.Length == 0)
            Widgets.RowNote("Pick one, or no call can name your spot.", Theme.Warn);
        else if (mine.Length > 0 && mine != Plan.Seat)
            Widgets.RowNote($"The Roles page has you in {mine}, not {Plan.Seat}.", Theme.Warn);
    }

    private static string SeatHint(StrategyAsset asset, string seat)
    {
        foreach (var known in asset.Seats)
            if (known.Key == seat) return $"{known.Role} {known.Party}, off the Roles page";
        return "Off the Roles page";
    }

    private void DrawAlignmentRow(StrategyAsset asset)
    {
        var options = asset.Alignments;
        if (options.Count == 0) return;

        var labels = new string[options.Count];
        for (var i = 0; i < options.Count; i++) labels[i] = options[i].Label;

        var index = 0;
        for (var i = 0; i < options.Count; i++)
            if (options[i].Value == Plan.Alignment) index = i;

        var hint = Plan.Alignment == StrategyPick.BossNorth
            ? "Directions read as the plan writes them"
            : "Tower directions turn to true north, the rest read as written";

        if (!Widgets.RowCombo("Which way is north", hint, ref index, labels, sub: true)) return;

        Plan.Alignment = options[index].Value;
        Touch();
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
