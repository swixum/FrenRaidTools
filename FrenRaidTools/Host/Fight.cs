using FrenRaidTools.Engine;
using FrenRaidTools.Engine.DancingMad;

namespace FrenRaidTools;

public sealed class Fight
{
    public CalloutCatalog Catalog { get; private set; } = new();

    public List<string> Faults { get; } = [];

    public DancingMadFight DancingMad { get; } = new();

    public int Sequences { get; private set; }

    public PlanCalls? Plan { get; private set; }

    public string? Key { get; private set; }

    public int Generation { get; private set; }

    private readonly List<LocalFight> _local = [];

    public IReadOnlyList<LocalFight> Local => _local;

    public bool PlanReady => Plan is not null;

    public bool RunsDancingMad => Key is null or "umad";

    private Func<StrategyBook?>? _book;

    public string? Chosen(string optionKey) => _book?.Invoke()?.ChosenOrDefault(optionKey);

    public MechanicNames Mechanics { get; private set; } = new();

    public CallOwners Owners { get; private set; } = new();

    public bool Load(PlannedFight fight, StrategyAsset asset, Func<StrategyBook?> book)
    {
        if (Key == fight.Key && Plan is not null) return false;

        Catalog = new CalloutCatalog();
        Owners = new CallOwners();
        Faults.Clear();
        _local.Clear();
        Mechanics = new MechanicNames();
        Plan = null;
        _book = null;
        Key = null;
        Sequences = 0;

        if (fight.Key == "umad") UseDancingMad();
        UseLocal(fight.Key);

        try
        {
            var plan = new PlanCalls(asset.FightKey, book);
            plan.Register(Catalog, asset);
            Plan = plan;
            Key = asset.FightKey;
            _book = book;
        }
        catch (Exception ex)
        {
            Faults.Add($"strat spots: {ex.Message}");
            Service.Log.Error(ex, "Plan calls would not load.");
            Key = fight.Key;
        }

        Generation++;
        return true;
    }

    private void UseDancingMad()
    {
        var parts = DancingMad.Parts().ToList();
        Sequences += parts.Count;

        foreach (var part in parts)
        {
            Mechanics.Claim(part.Mechanic, part.Mechanic);
            Mechanics.Claim(part.Group, part.Mechanic);
            Owners.Claim(part.Mechanic, "umad");
            Owners.Claim(part.Group, "umad");

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

    private void UseLocal(string fightKey)
    {
        foreach (var part in LocalFights.For(fightKey))
        {
            if (_local.Contains(part)) continue;

            try
            {
                Catalog.Register(part.Group, part.Holder, part.Phase, part.Mechanic);
                Owners.Claim(part.Group, part.Key);
                _local.Add(part);
                Sequences++;
            }
            catch (Exception ex)
            {
                Faults.Add($"{part.Group}: {ex.Message}");
                Service.Log.Error(ex, "Local fight calls would not load.");
            }
        }
    }

    public string FoldFor(CatalogEntry entry) => Mechanics.Fold(entry);

    public IEnumerable<int> Phases => Catalog.PhasesPresent;

    public static string PhaseName(int phase) => DancingMadFight.PhaseName(phase);

    public string PhaseNameFor(string fightKey, int phase)
    {
        foreach (var part in _local)
            if (part.Key == fightKey) return part.PhaseName(phase);
        return fightKey == "umad" ? DancingMadFight.PhaseName(phase) : $"P{phase}";
    }

    public IEnumerable<CatalogEntry> InPhase(int phase) => Catalog.InPhase(phase);
}
