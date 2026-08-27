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
    public const uint WingsBusterCast = 0xC487;

    public readonly Callout trinesInitial = Callout.Duration("Trines (Initial)", "Trines");
    public readonly Callout wingsOfDestruction =
        Callout.Duration("Trines: Wings of Destruction 1", "{wingsSafe} Safe");

    public readonly Callout trinesTankSpot =
        Callout.Of("Trines: Tank Spot", "Center to {tankSpot}")
            .Note("Tanks only. Counting counterclockwise from the 1 waymark, the first spot the opening trines left free, named by the waymark it sits on or between. Said once.");

    public readonly Callout trinesPartySpot =
        Callout.Of("Trines: DPS and Healer Spot", "Center to {partySpot}")
            .Note("DPS and healers. Counting clockwise from the A waymark, the first spot the opening trines left free, named by the waymark it sits on or between. Said once.");

    public readonly Callout lightOfJudgmentEnrage = Callout.Duration("Failed P2 Enrage", "Failed");
    public readonly Callout aeroIIIAssault = Callout.Duration("Aero III Assault", "Knockback");

    public readonly Callout wingsBuster =
        Callout.Duration("Trines: Wings Buster", "Near and Far Buster", "Near/Far Buster")
            .Note("Tanks only. One tank takes the near hit, the other the far hit, while the trines resolve.");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 120, e => e.Is(EventKind.CastStart, TrinesCast),
            (start, run) => Run(start, run, world));

    public Sequence BuildTankBuster(IWorld world) =>
        Sequence.Indexed(Group + "TankBuster", 30,
            e => e.Is(EventKind.CastStart, WingsBusterCast),
            (start, run, i) =>
            {
                if (JobKinds.Tanking(world)) run.Call(wingsBuster, start);
                return Task.CompletedTask;
            });

    private static bool IsTrineDrop(GameEvent e) =>
        e.Kind == EventKind.ActorControl && e.Id == TrineControl &&
        e.Arg1 == TrineArg1 && e.Arg2 == TrineArg2 && e.Arg3 == 0 && e.Arg4 == 0;

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(trinesInitial, start);

        var firstSet = await run.WaitEventsQuickSuccession(3, IsTrineDrop);
        await run.Settle();
        Spots(run, world, firstSet);

        var wings = await run.FindOrWaitForCast(world, e => e.Id is WingsRight or WingsLeft);
        if (wings is not null)
        {
            var facing = ArenaPos.Facing(wings.Source?.Heading ?? 0);
            var safe = facing.PlusQuads(wings.Id == WingsRight ? -1 : 1);
            run.SetParam("wingsSafe", safe.Told());
            run.Call(wingsOfDestruction, wings);
        }
    }

    private void Spots(SequenceRun run, IWorld world, IReadOnlyList<GameEvent> wave)
    {
        if (world.You is not { } you) return;

        var taken = Taken(wave, world);
        if (taken.Count == 0) return;

        var tank = TrineRing.FirstFree(taken, TrineRing.TankStart, -1);
        var party = TrineRing.FirstFree(taken, TrineRing.PartyStart, 1);

        run.SetParam("tankSpot", tank);
        run.SetParam("partySpot", party);

        var tanking = Tanking(world, you);
        if (tanking && tank is null) return;
        if (!tanking && party is null) return;

        run.Call(tanking ? trinesTankSpot : trinesPartySpot);
    }

    public static bool Tanking(IWorld world, Actor you)
    {
        var seat = world.SeatOf(you);
        return seat >= 0
            ? Slots.RoleOf(seat) == SlotRole.Tank
            : JobKinds.Kind(you.Job) == JobKind.Tank;
    }

    public static HashSet<int> Taken(IEnumerable<GameEvent> drops, IWorld world) =>
        [.. drops.Select(d => d.Target)
                 .OfType<Actor>()
                 .Select(a => TrineRing.Spot((world.Latest(a) ?? a).Pos))
                 .Where(spot => spot >= 0)];

    public Sequence BuildExtras(IWorld world) =>
        Sequence.Indexed(Group + "Extras", 30,
            e => e.Is(EventKind.CastStart, LightOfJudgmentEnrageCast, AeroIIIAssaultCast),
            (start, run, i) =>
            {
                run.Call(start.Id == LightOfJudgmentEnrageCast ? lightOfJudgmentEnrage : aeroIIIAssault, start);
                return Task.CompletedTask;
            });
}
