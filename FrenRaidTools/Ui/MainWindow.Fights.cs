using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private void DrawFightCategory()
    {
        var fights = Expansions.Order(FightPlans.All.Where(f => f.Category == _navCategory));

        PageHeader(_navCategory, fights.Count == 1 ? "1 fight" : $"{fights.Count} fights");

        if (fights.Count == 0)
        {
            Widgets.EmptyState("Nothing here yet", "Fights land in this list as they are built.");
            return;
        }

        foreach (var expansion in Expansions.Order(fights.Select(f => f.Expansion)))
        {
            var inside = fights.Where(f => f.Expansion == expansion).ToList();
            var open = !_shutExpansions.Contains(expansion);
            var was = open;

            if (Widgets.FoldBegin("exp" + expansion, expansion, $"{inside.Count}", 0, ref open))
                foreach (var fight in inside)
                    DrawFightRow(fight);

            Widgets.FoldEnd();

            if (open == was) continue;
            if (open) _shutExpansions.Remove(expansion);
            else _shutExpansions.Add(expansion);
        }
    }

    private readonly HashSet<string> _shutExpansions = [];

    private void DrawFightRow(PlannedFight fight)
    {
        var here = Game.Zone == fight.Territory;
        var loaded = fight.Key == C.PlanFight ? Board.Catalog.Count : 0;

        var hint = here ? "You are in here now"
            : loaded > 0 ? $"{loaded} calls ready"
            : $"Zone {fight.Territory}";

        if (!Widgets.RowDoor(fight.FullName, hint, here ? Theme.Accent : 0u)) return;

        OpenFight(fight);
    }

    private void OpenFight(PlannedFight fight)
    {
        C.PlanFight = fight.Key;
        _navCategory = fight.Category;
        _nav = Nav.Calls;
        _search = "";
        Touch();
    }

    private void DrawBackToCategory()
    {
        if (!Widgets.Crumb(_navCategory)) return;
        _nav = Nav.Fights;
        _search = "";
    }
}
