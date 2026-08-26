namespace FrenRaidTools.Engine.DancingMad;

public sealed class Flood
{
    public const string Group = "flood";

    public const int PhaseNumber = 5;

    public const string MechanicName = "Flood of Naught";

    public const uint FloodCast = 0xC13F;
    public const uint FloodTowerCast = 0xC183;
    public const uint FloodResolve = 0xC269;

    public const uint UltimaUpsurgeCast = 0xC24A;
    public const uint P4EnrageFailCast = 0xBABB;
    public const uint UltimaRepeaterCast = 0xBB40;

    public static readonly ArenaPos Ap = new(100, 100, 2, 2);

    public readonly Callout ultimaUpsurge =
        Callout.Duration("Ultima Upsurge", "Big Raidwide");

    public readonly Callout p4enrageFail =
        Callout.Duration("P4 Enrage (Failed)", "Failed");

    public readonly Callout ultimaRepeater =
        Callout.Duration("Ultima Repeater", "Raidwide, Role Spots");

    public readonly Callout flood =
        Callout.Duration("Flood", "Flood");

    public readonly Callout floodCall =
        Callout.Of("Flood: Location and Rotation",
            "Start {startWaymark}, {clockwise ? 'Clockwise' : 'Counterclockwise'}",
            "Start {startWaymark}, {clockwise ? 'CW' : 'CCW'}").Note("{startWaymark} is the waymark letter on the cardinal to start at. You can use the variables 'start' and 'secondStart' for the direction opposite the first and second hit (intercard),\\ along with {cardinalStart} for the cardinal between those two. 'final' is opposite the final hit, and cardinalFinal is the safe spot for the 3rd and 4th hits.");

    public readonly Callout floodMove1 =
        Callout.Of("Flood: Move 1", "Move").Quiet();

    public readonly Callout floodMove2 =
        Callout.Of("Flood: Move 2", "Move").Quiet();

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, FloodCast),
            (start, run) => Run(start, run, world));

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(flood, start);

        var firstSet = await run.WaitEventsQuickSuccession(2, e => e.Is(EventKind.CastStart, FloodTowerCast));
        var secondSet = await run.WaitEventsQuickSuccession(2, e => e.Is(EventKind.CastStart, FloodTowerCast));

        var firstLocation = Where(firstSet, world);
        var secondLocation = Where(secondSet, world);

        var startSector = firstLocation.Opposite();
        var second = secondLocation.Opposite();

        run.SetParam("start", startSector.Told());
        run.SetParam("second", second.Told());
        var cardinalStart = Combine(startSector, second);
        run.SetParam("cardinalStart", cardinalStart.Told());
        run.SetParam("startWaymark", cardinalStart.Waymark());

        var clockwise = Turning(firstLocation, secondLocation);
        run.SetParam("clockwise", clockwise);
        run.SetParam("final", startSector.PlusQuads(clockwise ? -1 : 1).Told());
        run.SetParam("cardinalFinal", cardinalStart.Opposite().Told());

        run.Call(floodCall);

        await run.WaitEvent(EventKind.AbilityHit, FloodResolve);
        run.Call(floodMove1);
        await run.WaitMs(100);
        await run.WaitEvent(EventKind.AbilityHit, FloodResolve);
        run.Call(floodMove2);
    }

    public Sequence BuildCasts(IWorld world) =>
        CastCalls.For(Group + "Casts",
            (UltimaUpsurgeCast, ultimaUpsurge),
            (P4EnrageFailCast, p4enrageFail),
            (UltimaRepeaterCast, ultimaRepeater));

    private static ArenaSector Where(IEnumerable<GameEvent> casts, IWorld world)
    {
        foreach (var cast in casts)
        {
            var actor = cast.Source is null ? null : world.Latest(cast.Source) ?? cast.Source;
            if (actor is null) continue;
            var sector = Ap.For(actor.Pos);
            if (sector.IsPoint()) return sector;
        }
        return ArenaSector.Unknown;
    }

    public static bool Turning(ArenaSector from, ArenaSector to)
    {
        var step = from.EighthsTo(to);
        return step > 0 && step < ArenaSectors.Eighths / 2;
    }

    private static ArenaSector Combine(ArenaSector a, ArenaSector b)
    {
        if (!a.IsPoint() || !b.IsPoint()) return ArenaSector.Unknown;
        var step = a.EighthsTo(b);
        if (step == 2) return a.PlusEighths(1);
        if (step == 6) return b.PlusEighths(1);
        return ArenaSector.Unknown;
    }
}
