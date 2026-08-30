namespace FrenRaidTools.Engine;

public sealed record LocalFight(
    string Key,
    string Group,
    string Mechanic,
    int Phase,
    object Holder,
    Func<Sequence>? Build)
{
    public IReadOnlyList<string> PhaseNames { get; init; } = [];

    public Func<IWorld, IEnumerable<Sequence>>? Extra { get; init; }

    public const string AnyPhase = "Any phase";

    public string PhaseName(int phase) =>
        phase switch
        {
            <= 0 => AnyPhase,
            _ when phase <= PhaseNames.Count => PhaseNames[phase - 1],
            _ => $"P{phase}",
        };
}

public static class LocalFights
{
    private static readonly List<LocalFight> Registry = [];

    public static IReadOnlyList<LocalFight> All
    {
        get
        {
            lock (Registry) return Registry.ToArray();
        }
    }

    public static void Register(LocalFight fight)
    {
        lock (Registry)
        {
            foreach (var known in Registry)
                if (known.Group == fight.Group) return;
            Registry.Add(fight);
        }
    }

    public static IEnumerable<LocalFight> For(string? fightKey)
    {
        if (string.IsNullOrEmpty(fightKey)) yield break;
        foreach (var fight in All)
            if (fight.Key == fightKey) yield return fight;
    }
}
