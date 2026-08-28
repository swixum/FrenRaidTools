using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;
using FrenRaidTools.Engine.DancingMad;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private const string DancingMadKey = "umad";

    private static readonly string[] CleanseModes =
    [
        "Every cleanse",
        "Prior set, same role",
        "Whole prior set",
    ];

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
        if (fight.Key == DancingMadKey) DrawDancingMadStrats();
    }

    private void DrawDancingMadStrats()
    {
        Widgets.SectionHeader("Call choices");
        Widgets.ListBegin();

        var cleanse = (int)C.CleanseCallMode;
        if (Widgets.RowCombo("Earthquake cleanses", CleanseHint(C.CleanseCallMode),
                ref cleanse, CleanseModes, sub: true))
        {
            C.CleanseCallMode = (CleanseCalls)Math.Clamp(cleanse, 0, CleanseModes.Length - 1);
            Touch();
        }

        var noDebuff = C.DoubleTowerOnlyWithNoDebuff;
        if (Widgets.RowCheckClick("Skip double tower when you start with a debuff",
                "Only when you go in clean", ref noDebuff, sub: true))
        {
            C.DoubleTowerOnlyWithNoDebuff = noDebuff;
            Touch();
        }

        Widgets.ListEnd();
    }

    private static string CleanseHint(CleanseCalls mode) => mode switch
    {
        CleanseCalls.All => "Every cleanse that lands, whoever it was",
        CleanseCalls.Matched => "Only whoever took your spot in the set before yours",
        _ => "Everyone in the set before yours",
    };
}
