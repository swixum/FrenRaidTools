namespace FrenRaidTools.Engine.DancingMad;

public sealed class Trines
{
    public const string Group = "trines";

    public const uint TrinesCast = 0xBADF;
    public const uint WingsRight = 0xBACE;
    public const uint WingsLeft = 0xBACD;

    public const uint TrineControl = 0x19D;
    public const uint TrineArg1 = 0x10;
    public const uint TrineArg2 = 0x20;

    public const uint LightOfJudgmentEnrageCast = 0xBAE1;
    public const uint AeroIIIAssaultCast = 0xC3F7;

    public static readonly ArenaPos Tight = new(100.0, 100.0, 4.0, 4.0);

    public static readonly ArenaSector[] TrinePositions =
    [
        ArenaSector.Center, ArenaSector.North, ArenaSector.Southeast, ArenaSector.Northeast,
        ArenaSector.South, ArenaSector.Southwest, ArenaSector.Northwest,
    ];

    public readonly Callout trinesInitial = Callout.Duration("Trines (Initial)", "Trines");
    public readonly Callout wingsOfDestruction =
        Callout.Duration("Trines: Wings of Destruction 1", "{wingsSafe} Safe");

    public readonly Callout trinesSafe = Callout.Of(
            "{bestStart} to {firstTrineLocations}",
            "{bestStart}",
            "{bestStart} to {firstTrineLocations}")
        .Note("This call will provide a starting position (prefers center if available, else one that is adjacent to one or more safe spots) and all of the safe spots.");

    public readonly Callout lightOfJudgmentEnrage = Callout.Duration("Failed P2 Enrage", "Failed");
    public readonly Callout aeroIIIAssault = Callout.Duration("Aero III Assault", "Knockback");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 120, e => e.Is(EventKind.CastStart, TrinesCast),
            (start, run) => Run(start, run, world));

    private static bool IsTrineDrop(GameEvent e) =>
        e.Kind == EventKind.ActorControl && e.Id == TrineControl &&
        e.Arg1 == TrineArg1 && e.Arg2 == TrineArg2 && e.Arg3 == 0 && e.Arg4 == 0;

    public static ArenaSector BestStart(
        IReadOnlyList<ArenaSector> third, IReadOnlyList<ArenaSector> first)
    {
        if (third.Contains(ArenaSector.Center)) return ArenaSector.Center;

        foreach (var candidate in third)
        {
            if (candidate.IsCardinal())
            {
                if (first.Any(f => f.IsStrictlyAdjacentTo(candidate))) return candidate;
                continue;
            }

            foreach (var from in first)
            {
                if (from.IsCardinal())
                {
                    if (from.IsStrictlyAdjacentTo(candidate)) return candidate;
                    continue;
                }

                var sameWest = from.IsStrictlyAdjacentTo(ArenaSector.West)
                               && candidate.IsStrictlyAdjacentTo(ArenaSector.West);
                var sameEast = from.IsStrictlyAdjacentTo(ArenaSector.East)
                               && candidate.IsStrictlyAdjacentTo(ArenaSector.East);
                if (sameWest || sameEast) return candidate;
            }
        }

        return ArenaSector.Unknown;
    }

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(trinesInitial, start);

        var firstSet = await run.WaitEventsQuickSuccession(3, IsTrineDrop);
        await run.Settle();

        var firstLocations = Locations(firstSet, world);
        run.SetParam("firstTrineLocations", firstLocations.Told());

        var wings = await run.FindOrWaitForCast(world, e => e.Id is WingsRight or WingsLeft);
        if (wings is not null)
        {
            var facing = ArenaPos.Facing(wings.Source?.Heading ?? 0);
            var safe = facing.PlusQuads(wings.Id == WingsRight ? -1 : 1);
            run.SetParam("wingsSafe", safe.Told());
            run.Call(wingsOfDestruction, wings);
        }

        var secondSet = await run.WaitEventsQuickSuccession(3, IsTrineDrop);
        await run.Settle();

        var secondLocations = Locations(secondSet, world);
        run.SetParam("secondTrineLocations", secondLocations.Told());

        var third = TrinePositions
            .Where(p => !firstLocations.Contains(p) && !secondLocations.Contains(p))
            .ToList();

        run.SetParam("thirdTrineLocations", third.Told());
        run.SetParam("bestStart", BestStart(third, firstLocations).Told());

        run.Call(trinesSafe);
    }

    private static List<ArenaSector> Locations(IEnumerable<GameEvent> drops, IWorld world) =>
        drops.Select(d => d.Target)
             .OfType<Actor>()
             .Select(a => Tight.For(world.Latest(a) ?? a, TrinePositions))
             .ToList();

    public Sequence BuildExtras(IWorld world) =>
        Sequence.Indexed(Group + "Extras", 30,
            e => e.Is(EventKind.CastStart, LightOfJudgmentEnrageCast, AeroIIIAssaultCast),
            (start, run, i) =>
            {
                run.Call(start.Id == LightOfJudgmentEnrageCast ? lightOfJudgmentEnrage : aeroIIIAssault, start);
                return Task.CompletedTask;
            });
}
