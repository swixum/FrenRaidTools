using FrenRaidTools.Engine;
using FrenRaidTools.Engine.DancingMad;

namespace FrenRaidTools;

public sealed class Fight
{
    public CalloutCatalog Catalog { get; } = new();

    public List<string> Faults { get; } = [];

    public DancingMadFight DancingMad { get; } = new();

    public int Sequences { get; }

    public Fight()
    {
        var parts = DancingMad.Parts().ToList();
        Sequences = parts.Count;

        foreach (var part in parts)
        {
            _mechanicByGroup[part.Mechanic] = part.Mechanic;
            _mechanicByGroup[part.Group] = part.Mechanic;

            try
            {
                Catalog.Register(part.Mechanic, part.Holder, (int)part.Phase, part.Mechanic);
            }
            catch (Exception ex)
            {
                Faults.Add($"{part.Mechanic}: {ex.Message}");
                Service.Log.Error(ex, "Callout group would not load.");
            }
        }
    }

    public PlanCalls? Plan { get; private set; }

    public bool PlanReady => Plan is not null;

    private Func<StrategyBook?>? _book;

    public string? Chosen(string optionKey) => _book?.Invoke()?.ChosenOrDefault(optionKey);

    public bool UsePlan(StrategyAsset asset, Func<StrategyBook?> book)
    {
        if (Plan is not null) return false;

        try
        {
            var plan = new PlanCalls(asset.FightKey, book);
            plan.Register(Catalog, asset);
            Plan = plan;
            _book = book;
            return true;
        }
        catch (Exception ex)
        {
            Faults.Add($"strat spots: {ex.Message}");
            Service.Log.Error(ex, "Plan calls would not load.");
            return false;
        }
    }

    private readonly Dictionary<string, string> _mechanicByGroup = new(StringComparer.Ordinal);

    public string MechanicFor(string group) =>
        _mechanicByGroup.TryGetValue(group, out var mechanic) ? mechanic : group;

    public IEnumerable<int> Phases => Catalog.PhasesPresent;

    public static string PhaseName(int phase) => DancingMadFight.PhaseName(phase);

    public IEnumerable<CatalogEntry> InPhase(int phase) => Catalog.InPhase(phase);
}
