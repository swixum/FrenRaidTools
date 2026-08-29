using FrenRaidTools.Engine;
using FrenRaidTools.Engine.DancingMad;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private static readonly string[] Debuffs = ["Stack", "Cone", "Circle"];

    private readonly HashSet<string> _foldedSpots = [];

    private void DrawDirections(StrategyAsset asset)
    {
        var book = PlanSource.Book(PickedFight, Plan);
        if (book is null || !book.Ready)
        {
            Widgets.SectionHeader("Your directions");
            Widgets.ListBegin();
            Widgets.RowNote("Pick your spot above.", Theme.Warn);
            Widgets.ListEnd();
            return;
        }

        Widgets.SectionHeader("Your directions");

        DrawForsakenDirections(book);
        DrawBlackHoleDirections(book);
        DrawSpotDirections(book);
    }

    private static readonly string[] TimelineNames =
        [PlanAnchors.TimelineDouble, PlanAnchors.TimelineDsa, PlanAnchors.TimelineSda];

    private void DrawBlackHoleDirections(StrategyBook book)
    {
        StrategyMechanic? mechanic = null;
        foreach (var name in TimelineNames)
        {
            mechanic = book.Mechanic(PlanAnchors.BlackHoles, name);
            if (mechanic is not null) break;
        }

        if (mechanic?.Action is null) return;

        var timeline = TextLines.Of(mechanic.Action.Text);
        var rules = TextLines.Of(mechanic.Description?.Text);

        Fold("spotsHoles", "Black Holes", Short(mechanic.Name), Theme.Muted,
            _foldedSpots, openByDefault: false, () =>
            {
                Widgets.RowNoteWrap(
                    "Your hole depends on where Kefka ports. Tether number "
                    + "here, direction in the fight.");

                foreach (var place in PlanTether.Places)
                foreach (var group in PlanTether.Groups)
                    DrawHoleSets(book, mechanic, timeline, rules, place, group);
            });
    }

    private void DrawHoleSets(
        StrategyBook book, StrategyMechanic mechanic,
        IReadOnlyList<string> timeline, IReadOnlyList<string> rules, string place, string group)
    {
        for (var set = 1; set <= HoleSets; set++)
        {
            var lines = PlanTether.Lines(timeline, rules, set, place, group, null);
            var cue = PlanStep.Read(lines, line => book.Aligned(line, mechanic));
            if (cue.Empty) continue;

            Widgets.RowText($"{place} in Line {group}  Set {set}", cue.Display, sub: true);
        }
    }

    private const int HoleSets = 4;

    private static string Short(string name)
    {
        var open = name.IndexOf('(');
        var close = name.LastIndexOf(')');
        return open >= 0 && close > open ? name[(open + 1)..close] : name;
    }

    private void DrawForsakenDirections(StrategyBook book)
    {
        foreach (var group in new[] { PlanAnchors.GroupA, PlanAnchors.GroupB })
        {
            var mechanic = book.Mechanic(PlanAnchors.ForsakenOverview, group);
            if (mechanic is null) continue;
            if (!mechanic.Seats.TryGetValue(book.Pick.Seat, out var seat)) continue;

            Fold("spots" + group, group, "8 towers", Theme.Muted,
                _foldedSpots, openByDefault: false, () =>
                {
                    for (var tower = 1; tower <= Forsaken.LastTowerSet; tower++)
                        DrawTower(book, mechanic, seat, tower);
                });
        }
    }

    private void DrawTower(
        StrategyBook book, StrategyMechanic mechanic, StrategyText seat, int tower)
    {
        var block = seat.Step(tower);
        if (block is null) return;

        var role = SlotRoleName(Plan.Seat);

        foreach (var debuff in Debuffs)
        {
            var lines = PlanStep.Lines(block, debuff, null, null, role);
            var cue = PlanStep.Read(lines, line => book.Aligned(line, mechanic));
            if (cue.Empty) continue;

            Widgets.RowText($"Tower {tower}  {debuff}", cue.Display, sub: true);
        }
    }

    private static string? SlotRoleName(string seat) => Slots.IndexOf(seat) switch
    {
        0 or 1 => "Tank",
        2 or 3 => "Healer",
        4 or 5 => "Melee",
        6 or 7 => "Ranged",
        _ => null,
    };

    private void DrawSpotDirections(StrategyBook book)
    {
        var spots = PlanAnchors.For(PickedFight.Key)
            .Where(a => !a.Replaces)
            .Select(a => (a.Phase, Name: a.Mechanics[0]))
            .Distinct()
            .ToList();

        Fold("spotsPlain", "Everything else", $"{spots.Count}", Theme.Muted,
            _foldedSpots, openByDefault: false, () =>
            {
                foreach (var (phase, name) in spots)
                {
                    var mechanic = book.Mechanic(phase, name);
                    if (mechanic is null) continue;

                    var cue = book.Say(mechanic);
                    if (cue.Empty) continue;

                    Widgets.RowText($"{phase}: {name}", cue.Display.Replace("\n", ", "), sub: true);
                }
            });
    }
}
