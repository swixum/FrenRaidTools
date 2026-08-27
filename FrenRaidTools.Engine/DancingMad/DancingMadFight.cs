namespace FrenRaidTools.Engine.DancingMad;

public enum Phase
{
    P1Kefka = 1,
    P2ForsakenKefka = 2,
    P3ChaosAndExdeath = 3,
    P4KefkaSays = 4,
    P5UltimaKefka = 5,
}

public sealed record PhaseInfo(Phase Phase, string Name, IReadOnlyList<string> Groups);

public sealed class DancingMadFight
{
    public const ushort Territory = 1363;

    public GravenImage GravenImage { get; } = new();
    public TeleTrouncing TeleTrouncing { get; } = new();
    public Forsaken Forsaken { get; } = new();
    public Trines Trines { get; } = new();
    public Bowels Bowels { get; } = new();
    public LimitCut LimitCut { get; } = new();
    public Earthquake Earthquake { get; } = new();
    public StompAMole StompAMole { get; } = new();
    public GrandCross GrandCross { get; } = new();
    public KefkaSays KefkaSays { get; } = new();
    public Flood Flood { get; } = new();
    public MaddeningOrchestra MaddeningOrchestra { get; } = new();
    public Celestriad Celestriad { get; } = new();
    public FellForces FellForces { get; } = new();
    public P5Forsaken P5Forsaken { get; } = new();
    public DirectCalls DirectCalls { get; } = new();

    public VfxTracker Vfx { get; } = new();

    public DancingMadFight()
    {
        GrandCross.Vfx = Vfx;
        KefkaSays.Vfx = Vfx;
    }

    public static readonly IReadOnlyList<PhaseInfo> Phases =
    [
        new(Phase.P1Kefka, "Kefka",
            [DancingMad.GravenImage.Group, DancingMad.TeleTrouncing.Group]),
        new(Phase.P2ForsakenKefka, "Forsaken Kefka",
            [DancingMad.Forsaken.Group, DancingMad.Trines.Group]),
        new(Phase.P3ChaosAndExdeath, "Chaos and Exdeath",
            [DancingMad.Bowels.Group, DancingMad.LimitCut.Group,
             DancingMad.Earthquake.Group, DancingMad.StompAMole.Group]),
        new(Phase.P4KefkaSays, "Kefka Says",
            [DancingMad.GrandCross.Group, DancingMad.KefkaSays.Group]),
        new(Phase.P5UltimaKefka, "Ultima Kefka",
            [DancingMad.Flood.Group, DancingMad.MaddeningOrchestra.Group,
             DancingMad.Celestriad.Group, DancingMad.FellForces.Group,
             DancingMad.P5Forsaken.Group]),
    ];

    public CalloutCatalog Catalog()
    {
        var catalog = new CalloutCatalog();
        foreach (var part in Parts())
            catalog.Register(part.Group, part.Holder, (int)part.Phase, part.Mechanic);
        return catalog;
    }

    public sealed record Part(Phase Phase, string Group, string Mechanic, object Holder);

    public IEnumerable<Part> Parts()
    {
        yield return new Part(Phase.P1Kefka, DancingMad.GravenImage.Group, "Graven Image", GravenImage);
        yield return new Part(Phase.P1Kefka, DancingMad.TeleTrouncing.Group, "Tele-trouncing", TeleTrouncing);
        yield return new Part(Phase.P2ForsakenKefka, DancingMad.Forsaken.Group, "Forsaken", Forsaken);
        yield return new Part(Phase.P2ForsakenKefka, DancingMad.Trines.Group, "Trines", Trines);
        yield return new Part(Phase.P3ChaosAndExdeath, DancingMad.Bowels.Group, "Bowels of Agony", Bowels);
        yield return new Part(Phase.P3ChaosAndExdeath, DancingMad.LimitCut.Group, "Limit Cut", LimitCut);
        yield return new Part(Phase.P3ChaosAndExdeath, DancingMad.Earthquake.Group, DancingMad.Earthquake.MechanicName, Earthquake);
        yield return new Part(Phase.P3ChaosAndExdeath, DancingMad.StompAMole.Group, DancingMad.StompAMole.MechanicName, StompAMole);
        yield return new Part(Phase.P4KefkaSays, DancingMad.GrandCross.Group, DancingMad.GrandCross.MechanicName, GrandCross);
        yield return new Part(Phase.P4KefkaSays, DancingMad.KefkaSays.Group, DancingMad.KefkaSays.MechanicName, KefkaSays);
        yield return new Part(Phase.P5UltimaKefka, DancingMad.Flood.Group, DancingMad.Flood.MechanicName, Flood);
        yield return new Part(Phase.P5UltimaKefka, DancingMad.MaddeningOrchestra.Group, DancingMad.MaddeningOrchestra.MechanicName, MaddeningOrchestra);
        yield return new Part(Phase.P5UltimaKefka, DancingMad.Celestriad.Group, DancingMad.Celestriad.MechanicName, Celestriad);
        yield return new Part(Phase.P5UltimaKefka, DancingMad.FellForces.Group, DancingMad.FellForces.MechanicName, FellForces);
        yield return new Part(Phase.P5UltimaKefka, DancingMad.P5Forsaken.Group, DancingMad.P5Forsaken.MechanicName, P5Forsaken);
        yield return new Part(Phase.P1Kefka, DancingMad.DirectCalls.Group, DancingMad.DirectCalls.MechanicName, DirectCalls);
    }

    public static string PhaseName(int phase) =>
        Phases.FirstOrDefault(p => (int)p.Phase == phase) is { } found
            ? $"P{(int)found.Phase} {found.Name}"
            : $"P{phase}";

    public IEnumerable<Sequence> Sequences(IWorld world)
    {
        yield return GravenImage.Build(world);
        yield return TeleTrouncing.Build(world);
        yield return Forsaken.Build(world);
        yield return Trines.Build(world);
        yield return Bowels.Build(world);
        yield return LimitCut.Build(world);
        yield return Earthquake.Build(world);
        yield return StompAMole.Build(world);
        yield return GrandCross.Build(world);
        yield return GrandCross.BuildInfernoTsunami(world);
        yield return KefkaSays.Build(world);
        yield return Flood.Build(world);
        yield return MaddeningOrchestra.Build(world);
        yield return Celestriad.Build(world);
        yield return Celestriad.BuildCatastrophic(world);
        yield return FellForces.BuildAfterRepeater(world);
        yield return FellForces.BuildAfterOrchestra(world);
        yield return P5Forsaken.Build(world);
        yield return Earthquake.BuildLatLong(world);
        yield return Earthquake.BuildCleanses(world);
        yield return Earthquake.BuildTethers(world);
        yield return KefkaSays.BuildExdeath(world);
        yield return KefkaSays.BuildChaos(world);
        yield return KefkaSays.BuildLimitBreak(world);
        yield return DirectCalls.Build(world);
        yield return Trines.BuildExtras(world);
        yield return Bowels.BuildHeroes(world);
        yield return Bowels.BuildDecisive(world);
        yield return Bowels.BuildBaitJump(world);
        yield return Forsaken.BuildExtras(world);
        yield return TeleTrouncing.BuildJudgment(world);
        yield return Earthquake.BuildCasts(world);
        yield return Earthquake.BuildTankSwap(world);
        yield return Trines.BuildTankBuster(world);
        yield return DirectCalls.BuildTankActions(world);
        yield return Flood.BuildCasts(world);
        yield return P5Forsaken.BuildCasts(world);
        yield return BuildVfxTracking();
    }

    public const string VfxTrackingName = "vfxTracking";

    public Sequence BuildVfxTracking() =>
        Sequence.Indexed(VfxTrackingName, 1, e => e.Kind == EventKind.StatusLoopVfx,
            (start, run, invocation) =>
            {
                Vfx.Take(start);
                return Task.CompletedTask;
            });

    public void Install(SequenceHost host)
    {
        host.AddRange(Sequences(host.World));
        host.ResetHooks.Add(Vfx.Reset);
        host.ResetHooks.Add(Earthquake.ForgetAssignment);
        host.ResetHooks.Add(Forsaken.ForgetStacks);
        host.ResetHooks.Add(FellForces.ForgetSets);
        host.ResetHooks.Add(KefkaSays.ForgetSecondSpot);
    }
}
