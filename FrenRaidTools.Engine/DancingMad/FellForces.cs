namespace FrenRaidTools.Engine.DancingMad;

public sealed class FellForces
{
    public const string Group = "fellForces";

    public const int PhaseNumber = 5;

    public const string MechanicName = "Fell Forces";

    public const uint TankAutos = 0xC653;
    public const uint HealerAutos = 0xC654;
    public const uint DpsAutos = 0xC655;

    public const double AutosAfterRepeaterSeconds = 5.56;
    public const double AutosAfterOrchestraSeconds = 7.53;
    public const double MinCountdownSeconds = 1.0;
    public const int HolyHitsPerRound = 3;

    public static readonly int[] RepeaterHits = [3, 2];
    public static readonly int[] OrchestraHits = [2, 3];

    public const double HitApartSeconds = 2.91;

    public static double Countdown(double toFirstAuto, int hits) =>
        toFirstAuto + Math.Max(hits - 1, 0) * HitApartSeconds;

    private int _repeaterSets;
    private int _orchestraSets;

    public static int HitsFor(IReadOnlyList<int> sets, int index) =>
        sets[Math.Clamp(index, 0, sets.Count - 1)];

    public void ForgetSets()
    {
        _repeaterSets = 0;
        _orchestraSets = 0;
    }

    public const string SpotParam = "mySpot";
    public const string HitsParam = "hits";

    public const string AfterRepeaterName = MechanicName + ": After Repeater";
    public const string AfterOrchestraName = MechanicName + ": After Orchestra";
    public const string TanksJoinName = MechanicName + ": Tanks Together";

    public const int JoinAfterHits = 2;

    public readonly Callout tanksJoin =
        Callout.Duration(TanksJoinName, "Tanks Together")
            .Note("The final set of role autos is the only one with a third hit, and the off tank moves in to share the tank spot for it. This goes up the moment the second auto lands, with the clock running to the third. Tanks only.")
            .OutOfPhase("Tank actions");

    private const string Shared = "Fell Forces is a set of role-based autos, one per role, on a 2.91 second beat. Four sets land in the phase, each with its own fixed number of autos: three then two after the two Ultima Repeaters, two then three after the two Maddening Orchestras. The call waits for the raidwide or the flare ahead of it to land before it appears, so nobody moves early. The clock covers the whole set, running from the moment the call goes up to the last auto landing, so it clears as the hits stop. Use {hits} for how many are coming and {mySpot} for the spot your role takes, which is North for tanks, Southwest for healers and Southeast for dps.";

    public readonly Callout fellForces =
        Callout.Duration(AfterRepeaterName,
            "{hits} Role Autos",
            "Role Autos x{hits}")
            .Note(Shared + " This one follows Ultima Repeater, whose own call already sends you to your spot, so it does not repeat it.");

    public readonly Callout fellForcesSpots =
        Callout.Duration(AfterOrchestraName,
            "{hits} Role Autos, Role Spots",
            "Role Autos x{hits}, Role Spots")
            .Note(Shared + " This one follows Maddening Orchestra, which has nothing ahead of it naming your spot, so it carries the reminder itself.");

    public static ArenaSector Spot(string job) => JobKinds.Kind(job) switch
    {
        JobKind.Tank => ArenaSector.North,
        JobKind.Healer => ArenaSector.Southwest,
        JobKind.Melee or JobKind.PhysRanged or JobKind.Caster => ArenaSector.Southeast,
        _ => ArenaSector.Unknown,
    };

    public static ArenaSector Spot(IWorld world) =>
        world.You is { } you ? Spot(you.Job) : ArenaSector.Unknown;

    public Sequence BuildAfterRepeater(IWorld world) =>
        Sequence.Repeat(Group + "Repeater", 60,
            e => e.Is(EventKind.CastStart, Flood.UltimaRepeaterCast),
            async (start, run) =>
            {
                await run.WaitEvent(EventKind.AbilityHit, e => e.Id == Flood.UltimaRepeaterCast);

                Announce(run, world, fellForces, AutosAfterRepeaterSeconds, HitsFor(RepeaterHits, _repeaterSets++));
            });

    public Sequence BuildAfterOrchestra(IWorld world) =>
        Sequence.Repeat(Group + "Orchestra", 60,
            e => e.Is(EventKind.CastStart, MaddeningOrchestra.OrchestraCast),
            async (start, run) =>
            {
                await run.WaitEventsQuickSuccession(HolyHitsPerRound, MaddeningOrchestra.IsHolyHit);
                await run.WaitEventsQuickSuccession(HolyHitsPerRound, MaddeningOrchestra.IsHolyHit);

                var set = _orchestraSets++;
                Announce(run, world, fellForcesSpots, AutosAfterOrchestraSeconds, HitsFor(OrchestraHits, set));

                if (set != OrchestraHits.Length - 1 || !IsTank(world)) return;

                for (var beat = 0; beat < JoinAfterHits; beat++)
                    await run.WaitEvent(EventKind.AbilityHit, IsAutoBeat);

                run.Call(tanksJoin, new GameEvent
                {
                    Kind = EventKind.CastStart,
                    Id = TankAutos,
                    At = run.Now,
                    Duration = HitApartSeconds,
                });
            });

    public static bool IsAutoBeat(GameEvent e) => e.Id == TankAutos && e.FirstTarget;

    public static bool IsTank(IWorld world) =>
        JobKinds.Kind(world.You?.Job ?? "") == JobKind.Tank;

    private void Announce(SequenceRun run, IWorld world, Callout call, double toFirstAuto, int hits)
    {
        var spot = Spot(world);

        run.SetParam(SpotParam, spot.IsPoint() ? spot.Told() : null);
        run.SetParam(HitsParam, hits);

        run.Call(call, new GameEvent
        {
            Kind = EventKind.CastStart,
            Id = DpsAutos,
            At = run.Now,
            Duration = Math.Max(Countdown(toFirstAuto, hits), MinCountdownSeconds),
        });
    }
}
