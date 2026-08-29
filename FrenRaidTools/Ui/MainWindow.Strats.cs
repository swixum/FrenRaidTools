using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private HashSet<string>? _openStratExpansions;

    private HashSet<string> StratFolds =>
        _openStratExpansions ??= [Expansions.Current, PickedFight.Expansion];

    private HashSet<string>? _openFights;

    private HashSet<string> OpenFights => _openFights ??= [PickedFight.Key];

    private void DrawStrats()
    {
        PageHeader("Strats", "what your group runs");

        DrawStratFightList();
    }

    private void DrawStratFightList()
    {
        var picked = PickedFight;
        var here = FightPlans.InZone(Game.Zone);
        var folds = StratFolds;

        Widgets.SectionHeader("Fight");

        foreach (var expansion in Expansions.Order(FightPlans.All.Select(f => f.Expansion)))
        {
            var inside = Expansions.Order(FightPlans.In(expansion));
            var mine = inside.Any(f => f.Key == picked.Key);
            var badge = mine ? picked.FullName : $"{inside.Count}";

            Fold(expansion, expansion, badge, mine ? Theme.Accent : 0, folds, openByDefault: false,
                () => { foreach (var fight in inside) DrawFightRow(fight, picked, here); });
        }

        if (here is null || here.Key == picked.Key) return;

        Widgets.ListBegin();
        Widgets.RowNoteWrap($"You are in {here.Name}. Its calls read its own strats.", Theme.Warn);
        Widgets.ListEnd();
    }

    private void DrawFightRow(PlannedFight fight, PlannedFight picked, PlannedFight? here)
    {
        var isPicked = fight.Key == picked.Key;
        var hint = fight.Key == here?.Key ? "You are in this fight" : fight.Category;
        var shown = isPicked && OpenFights.Contains(fight.Key);
        var wasShown = shown;

        if (Widgets.RowPickFold(fight.FullName, hint, isPicked, ref shown))
        {
            C.PlanFight = fight.Key;
            Touch();
        }

        if (shown != wasShown)
        {
            if (shown) OpenFights.Add(fight.Key);
            else OpenFights.Remove(fight.Key);
        }

        if (!isPicked || !shown) return;

        DrawFightStrats(fight);
    }
}
