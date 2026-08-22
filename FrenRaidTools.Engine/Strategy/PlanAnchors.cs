namespace FrenRaidTools.Engine;

public sealed record PlanAnchor
{
    public required string Phase { get; init; }
    public required IReadOnlyList<string> Mechanics { get; init; }

    public string RideCall { get; init; } = "";
    public StrategyBranch? Branch { get; init; }

    public int Invocation { get; init; } = -1;

    public bool Wildcard => Invocation < 0;

    public static PlanAnchor Ride(StrategySpot spot) => new()
    {
        Phase = spot.Phase,
        Mechanics = spot.Mechanics,
        RideCall = spot.CallKey,
        Branch = spot.Branch,
    };

    public static PlanAnchor Ride(string callKey, string phase, params string[] mechanics) => new()
    {
        Phase = phase,
        Mechanics = mechanics,
        RideCall = callKey,
    };

    public bool Replaces { get; init; }

    public string StepParam { get; init; } = "";

    public bool FromAction { get; init; }

    public bool Bait { get; init; }

    public string CallName { get; init; } = "";

    public string Named => CallName.Length > 0 ? CallName : Mechanics[0];

    public PlanAnchor Nth(int invocation) => this with { Invocation = invocation };

    public PlanAnchor Instead() => this with { Replaces = true };

    public PlanAnchor Step(string paramKey) => this with { StepParam = paramKey };

    public PlanAnchor Timeline() => this with { FromAction = true };

    public PlanAnchor Baits() => this with { Bait = true };

    public PlanAnchor Called(string name) => this with { CallName = name };

    public PlanAnchor When(string paramKey, string whenTrue, string whenFalse) =>
        this with { Branch = new StrategyBranch(paramKey, whenTrue, whenFalse) };
}

public static class PlanAnchors
{
    private static readonly Dictionary<string, Dictionary<string, int>> PhaseByTag =
        new(StringComparer.Ordinal)
        {
            ["umad"] = new(StringComparer.Ordinal)
            {
                ["p1"] = 1, ["arrows"] = 1,
                ["p2"] = 2, ["forsaken"] = 2,
                ["p3"] = 3, ["bowels"] = 3, ["blackhole"] = 3,
                ["p4"] = 4,
                ["p5"] = 5,
            },
        };

    public const string Bowels = "Bowels of Agony";
    public const string Orchestra = "Maddening Orchestra";
    public const string Orchestra2 = "Maddening Orchestra 2";

    public const uint UmbraSmash = 0xBB00;

    public const string ForsakenOverview = "Forsaken Overview";
    public const string GroupA = "Group A (Different Debuffs)";
    public const string GroupB = "Group B (Same Debuffs)";
    public const string TowerStep = "towerSet";
    public const string DebuffSplit = "differentDebuffs";

    public static readonly string[] ForsakenBaitCalls =
    [
        "forsakenFollowupPastCone", "forsakenFollowupPastCircle",
        "forsakenFollowupPastStack", "forsakenFollowupPastNothing",
        "forsakenFollowupFutureCone", "forsakenFollowupFutureCircle",
        "forsakenFollowupFutureStack", "forsakenFollowupFutureNothing",
    ];

    public static readonly string[] ForsakenTowerCalls =
    [
        "forsakenFirstCone", "forsakenFirstCircle", "forsakenFirstStack", "forsakenFirstNothing",
        "forsakenTowerCone", "forsakenTowerCircle", "forsakenTowerStack", "forsakenTowerNothing",
        "forsakenTowerNoPfCone", "forsakenTowerNoPfCircle",
        "forsakenTowerNoPfStack", "forsakenTowerNoPfNothing",
        .. ForsakenBaitCalls,
    ];

    private static IEnumerable<PlanAnchor> ForsakenTowers()
    {
        foreach (var call in ForsakenTowerCalls)
        {
            var anchor = PlanAnchor.Ride(call, ForsakenOverview, GroupA, GroupB)
                .When(DebuffSplit, GroupA, GroupB)
                .Step(TowerStep)
                .Instead();

            yield return ForsakenBaitCalls.Contains(call) ? anchor.Baits() : anchor;
        }
    }

    public const string BlackHoles = "Earthquake: Black Holes";
    public const string TimelineDouble = "Tether Timeline (DSA Double)";
    public const string TimelineDsa = "Tether Timeline (D>S>A)";
    public const string TimelineSda = "Tether Timeline (S>D>A)";
    public const string TetherStep = "tetherSet";

    public static readonly string[] BlackHoleCalls =
    [
        "earthquakeTetherSet1", "earthquakeTetherSet2",
        "earthquakeTetherSet3", "earthquakeTetherSet4",
    ];

    private static IEnumerable<PlanAnchor> BlackHoleSets()
    {
        for (var i = 0; i < BlackHoleCalls.Length; i++)
            yield return PlanAnchor
                .Ride(BlackHoleCalls[i], BlackHoles, TimelineDouble, TimelineDsa, TimelineSda)
                .Called($"Tether Set {i + 1}")
                .Step(TetherStep)
                .Timeline()
                .Instead();
    }

    public const string ShortResolve = "First Resolve (Short)";
    public const string LongResolve = "Second Resolve (Long)";

    private static readonly Dictionary<string, IReadOnlyList<PlanAnchor>> Extra =
        new(StringComparer.Ordinal)
        {
            ["umad"] =
            [
                PlanAnchor.Ride(
                    "graven2dropFirstStone", StrategySpots.GravenPuddles, "First Rocks").Instead(),
                PlanAnchor.Ride(
                    "graven2dropSecondStone", StrategySpots.GravenPuddles, "Second Rocks").Instead(),

                PlanAnchor.Ride("bowelsHeadwindAfter", Bowels, "Superjump").Instead(),
                PlanAnchor.Ride("bowelsTailwindAfter", Bowels, "Superjump").Instead(),

                PlanAnchor.Ride("stompAMoleTakeTower", StrategySpots.Stompies, "Towers + Enrage").Instead(),

                .. ForsakenTowers(),
                .. BlackHoleSets(),
            ],
        };

    public static IReadOnlyList<PlanAnchor> For(string fightKey)
    {
        var anchors = new List<PlanAnchor>();

        foreach (var spot in StrategySpots.Of(fightKey))
            anchors.Add(PlanAnchor.Ride(spot));

        if (Extra.TryGetValue(fightKey, out var extra))
            anchors.AddRange(extra);

        return anchors;
    }

    public static int PhaseFor(StrategyAsset asset, string planPhase)
    {
        if (!PhaseByTag.TryGetValue(asset.FightKey, out var byTag)) return 0;

        foreach (var phase in asset.Phases)
        {
            if (phase.Name != planPhase) continue;
            if (phase.Tag is { } tag && byTag.TryGetValue(tag, out var number)) return number;
        }

        return 0;
    }

    public static bool AnySeatText(StrategyAsset asset, string planPhase, string mechanic)
    {
        foreach (var phase in asset.Phases)
        {
            if (phase.Name != planPhase) continue;
            foreach (var candidate in phase.Mechanics)
                if (candidate.Name == mechanic && candidate.Seats.Count > 0) return true;
        }

        return false;
    }

    public static bool AnyActionText(StrategyAsset asset, string planPhase, string mechanic)
    {
        foreach (var phase in asset.Phases)
        {
            if (phase.Name != planPhase) continue;
            foreach (var candidate in phase.Mechanics)
                if (candidate.Name == mechanic && candidate.Action is { Text.Length: > 0 }) return true;
        }

        return false;
    }

    public static bool Reachable(PlanAnchor anchor, StrategyAsset asset)
    {
        foreach (var mechanic in anchor.Mechanics)
        {
            if (AnyActionText(asset, anchor.Phase, mechanic)) return true;
            if (!anchor.FromAction && AnySeatText(asset, anchor.Phase, mechanic)) return true;
        }

        return false;
    }
}
