namespace FrenRaidTools.Engine;

public sealed record StrategyBranch(string ParamKey, string WhenTrue, string WhenFalse);

public sealed record StrategySpot
{
    public string CallKey { get; }
    public string Phase { get; }
    public IReadOnlyList<string> Mechanics { get; }
    public StrategyBranch? Branch { get; init; }

    public StrategySpot(string callKey, string phase, params string[] mechanics)
    {
        if (mechanics.Length == 0)
            throw new ArgumentException($"{callKey} names no mechanic.", nameof(mechanics));
        CallKey = callKey;
        Phase = phase;
        Mechanics = mechanics;
    }

    public StrategySpot When(string paramKey, string whenTrue, string whenFalse)
    {
        foreach (var name in new[] { whenTrue, whenFalse })
            if (!Mechanics.Contains(name))
                throw new ArgumentException($"{CallKey} does not list '{name}'.", nameof(paramKey));

        return this with { Branch = new StrategyBranch(paramKey, whenTrue, whenFalse) };
    }
}

public static class StrategySpots
{
    public const string GravenFireIce = "Graven 1: Fire + Ice";
    public const string GravenLasers = "Graven 1: Lasers + Towers";
    public const string GravenPuddles = "Graven 2: Puddles";
    public const string GravenSecondConfetti = "Graven 2: Second Confetti";
    public const string GravenArrows = "Graven 3: Arrows";
    public const string GazeFireLightning = "Gaze + Fire + Lightning";
    public const string Bowels = "Bowels of Agony";
    public const string Stompies = "Earthquake: Stompies";
    public const string MaddeningOrchestra = "Maddening Orchestra";

    private static readonly IReadOnlyList<StrategySpot> DancingMadSpots =
    [
        new("gravenImage", GravenFireIce, "Start"),
        new("gravenRealIceSpread", GravenFireIce, "Spread"),
        new("gravenFakeIceSpread", GravenFireIce, "Spread"),
        new("gravenRealIceStack", GravenFireIce, "Stack"),
        new("gravenFakeIceStack", GravenFireIce, "Stack"),

        new("gravenSpreadForLaser", GravenLasers, "Conga"),

        new("sleepTetherInitial", GravenArrows, ArrowTetherSpots, ArrowTetherTethers),
        new("confusionTetherInitial", GravenArrows, ArrowTetherSpots, ArrowTetherTethers),
        new("sleepTether", GravenArrows, ArrowTetherSpots, ArrowTetherTethers),
        new("confuseTether", GravenArrows, ArrowTetherSpots, ArrowTetherTethers),

        new("earlyFakeGaze", GazeFireLightning, "Static Spots"),
        new("earlyRealGaze", GazeFireLightning, "Static Spots"),

        new StrategySpot("elementMechanic", GazeFireLightning, GazeSpread, GazeStack)
            .When("actualSpread", GazeSpread, GazeStack),

        new("bowelsInitial", Bowels, "Setup"),
    ];

    private const string ArrowTetherSpots = "Sleep/Confuse (Fixed positions)";
    private const string ArrowTetherTethers = "Sleep/Confuse (Tethers matter)";
    private const string GazeSpread = "Spread";
    private const string GazeStack = "Stack";

    private static readonly Dictionary<string, IReadOnlyList<StrategySpot>> ByFight =
        new(StringComparer.Ordinal)
        {
            ["umad"] = DancingMadSpots,
        };

    private static readonly Dictionary<string, Dictionary<string, StrategySpot>> Lookup =
        Build(ByFight);

    public static IReadOnlyList<StrategySpot> Of(string fightKey) =>
        ByFight.GetValueOrDefault(fightKey) ?? [];

    public static StrategySpot? For(string fightKey, string callKey)
    {
        if (callKey.Length == 0) return null;
        return Lookup.TryGetValue(fightKey, out var calls)
            ? calls.GetValueOrDefault(callKey)
            : null;
    }

    private static Dictionary<string, Dictionary<string, StrategySpot>> Build(
        IReadOnlyDictionary<string, IReadOnlyList<StrategySpot>> fights)
    {
        var built = new Dictionary<string, Dictionary<string, StrategySpot>>(StringComparer.Ordinal);
        foreach (var (fightKey, spots) in fights)
        {
            var calls = new Dictionary<string, StrategySpot>(StringComparer.Ordinal);
            foreach (var spot in spots)
            {
                if (calls.ContainsKey(spot.CallKey))
                    throw new InvalidOperationException(
                        $"Two spots in {fightKey} claim the call '{spot.CallKey}'.");
                calls[spot.CallKey] = spot;
            }
            built[fightKey] = calls;
        }
        return built;
    }
}
