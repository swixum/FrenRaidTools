using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruApoc
{
    public const string Group = "fru.apoc";

    public const string MechanicName = "Sextuple Apoc";

    public const string SequenceName = Group + ".spots";

    public const uint Apocalypse = 0x9D68;

    public const uint DarkEruption = 0x9D51;

    public const uint EruptionLands = 0x9D52;

    public const uint DarkestDance = 0x9CF5;

    public const uint DarkWater = 0x099D;

    public const uint SpinControl = 413;

    public const uint SpinKind = 4;

    public const uint Clockwise = 16;

    public const uint CounterClockwise = 64;

    public const double TimeoutSeconds = 60;

    public static readonly Callout apocSpread = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "apocSpread",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Answered from which pair of lights opens and which way they turn.\n"
                + "Both are read off the two rotation markers the lights raise.",
    };

    public static readonly Callout apocStacks = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "apocStacks",
        FromPlan = true,
        Speech = "Run to stack",
        Text = "Run to stack" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Every seat runs to the same stack, so the call names no spot.\n"
                + "The countdown is the water that pops next.",
    };

    public static readonly Callout apocKnockback = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "apocKnockback",
        FromPlan = true,
        Speech = "Knockback into stacks",
        Text = "Knockback into stacks" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The last water resolves through the knockback, so the stack and the push "
                + "are one moment rather than two.\n"
                + "Group one goes left of the Oracle and group two right, and the wall is "
                + "what the push has to miss.",
    };

    public const double WaterSlack = 2.0;

    public static readonly IReadOnlyList<double> WaterTimers = [10, 29, 38];

    public const double SwapDelaySeconds = 1.0;

    public const double SwapBackDelaySeconds = 1.2;

    public static readonly Callout apocSwap = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "apocSwap",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        SpeechDelaySeconds = SwapDelaySeconds,
        Notes = "The Apocalypse call rides the same cast and speaks first, so the voice waits "
                + "a second while the line is on screen straight away.\n"
                + "Six players draw a water timer, two of each length, and each group of four "
                + "needs one of each.\n"
                + "When a group draws the same length twice the seat higher up the order "
                + "moves, MT then OT then H1 then H2, and M1 then M2 then R1 then R2, so a "
                + "support and a DPS always trade sides together.\n"
                + "Said only to the two who move; the other six already have their side.",
    };

    public static readonly Callout apocSwapBack = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "apocSwapBack",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        SpeechDelaySeconds = SwapBackDelaySeconds,
        Notes = "The spreads are taken from your own side, so a swapper goes home for them "
                + "and has to cross back before the knockback flank.\n"
                + "Same two seats and the same partner as the first call.\n"
                + "It rides the Darkest Dance cast so the cross back is asked for right "
                + "before the knockback, and the delay keeps the voice clear of the "
                + "knockback line on the same cast.",
    };

    private static bool Matches(double got, double want) =>
        Math.Abs(got - want) <= WaterSlack;

    public static IReadOnlyList<(int Support, int Dps)> Swaps(IWorld world)
    {
        var held = new double[8];
        foreach (var status in world.ActiveStatuses())
        {
            if (status.Id != DarkWater || status.Target is null) continue;
            var seat = world.SeatOf(status.Target);
            if (seat >= 0) held[seat] = status.Duration;
        }

        var supports = new List<int>();
        var dps = new List<int>();

        foreach (var timer in WaterTimers)
        {
            var drew = Enumerable.Range(0, 8).Where(seat => Matches(held[seat], timer)).ToList();
            var mine = drew.Where(Slots.IsSupport).ToList();
            var theirs = drew.Where(seat => !Slots.IsSupport(seat)).ToList();
            if (mine.Count == 2) supports.Add(mine.Min());
            if (theirs.Count == 2) dps.Add(theirs.Min());
        }

        if (supports.Count != dps.Count) return [];

        return supports.Zip(dps).ToList();
    }

    public static Func<int, string> Called(IWorld world)
    {
        var bySeat = new Dictionary<int, string>();
        foreach (var one in world.Party)
        {
            var seat = world.SeatOf(one);
            if (seat >= 0 && one.Called.Length > 0) bySeat[seat] = one.Called;
        }

        return seat => bySeat.TryGetValue(seat, out var name) ? name : Slots.Names[seat];
    }

    public static (string[] Text, string[] Speech) SwapLines(
        IReadOnlyList<(int Support, int Dps)> swaps, string lead, Func<int, string> called)
    {
        var text = new string[8];
        var speech = new string[8];
        for (var seat = 0; seat < 8; seat++)
        {
            text[seat] = string.Empty;
            speech[seat] = string.Empty;
        }

        foreach (var (support, dps) in swaps)
        {
            text[support] = speech[support] = $"{lead} with {called(dps)}";
            text[dps] = speech[dps] = $"{lead} with {called(support)}";
        }

        return (text, speech);
    }

    public sealed record Spot(ArenaSector At, int Lean, bool Far)
    {
        public Spot Turned(int eighths) => this with { At = At.PlusEighths(eighths) };
    }

    private static Spot Near(ArenaSector at) => new(at, 0, false);

    private static Spot Wall(ArenaSector at, int lean) => new(at, lean, true);

    public static readonly IReadOnlyList<Spot> SpreadClockwise =
    [
        Near(ArenaSector.North), Near(ArenaSector.Northwest),
        Wall(ArenaSector.North, 1), Wall(ArenaSector.North, -1),
        Near(ArenaSector.South), Near(ArenaSector.Southeast),
        Wall(ArenaSector.South, 1), Wall(ArenaSector.South, -1),
    ];

    public static readonly IReadOnlyList<Spot> SpreadCounter =
    [
        Near(ArenaSector.Northeast), Near(ArenaSector.North),
        Wall(ArenaSector.North, 1), Wall(ArenaSector.North, -1),
        Near(ArenaSector.Southwest), Near(ArenaSector.South),
        Wall(ArenaSector.South, 1), Wall(ArenaSector.South, -1),
    ];

    public static GameEvent? SoonestHeld(SequenceRun run, IWorld world, uint statusId) =>
        world.ActiveStatuses()
            .Where(s => s.Id == statusId && s.Target is not null && run.Remaining(s) > 0)
            .OrderBy(run.Remaining)
            .FirstOrDefault();

    public static readonly IReadOnlyList<int> TurnClockwise = [-1, 0, -3, -2];

    public static readonly IReadOnlyList<int> TurnCounter = [-3, -2, -1, 0];

    public static int TurnFor(ArenaSector opening, bool clockwise)
    {
        var quarter = ((int)opening % 4 + 4) % 4;
        return (clockwise ? TurnClockwise : TurnCounter)[quarter];
    }

    public static ArenaSector Opening(ArenaSector a, ArenaSector b)
    {
        if (!a.IsPoint() || !b.IsPoint()) return ArenaSector.Unknown;
        if (a.Opposite() != b) return ArenaSector.Unknown;
        return (int)a < 4 ? a : b;
    }

    public static (string Text, string Speech) Words(Spot spot, string what, string said)
    {
        var at = spot.At;
        if (!spot.Far)
            return ($"{at.Short()}, {what}", $"{at.Name()}, {said}");

        if (spot.Lean == 0)
            return ($"{at.Short()} wall, {what}", $"The {at.Spoken()} wall, {said}");

        var toward = at.PlusEighths(spot.Lean);
        return ($"{at.Short()} wall, {toward.Short()} side",
                $"The {at.Spoken()} wall, {toward.Spoken()} side");
    }

    public static (string[] Text, string[] Speech) Lines(
        IReadOnlyList<Spot> table, int turn, string what, string said)
    {
        var text = new string[8];
        var speech = new string[8];
        for (var seat = 0; seat < 8; seat++)
        {
            var (t, s) = Words(table[seat].Turned(turn), what, said);
            text[seat] = t;
            speech[seat] = s;
        }

        return (text, speech);
    }

    private static ArenaSector SectorOf(IWorld world, GameEvent got) =>
        FruArena.SectorOf(world, got.Target);

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, Apocalypse),
            async (start, run) =>
            {
                var swaps = Swaps(world);
                if (swaps.Count > 0)
                {
                    var moving = SwapLines(swaps, "Swap", Called(world));
                    SeatCalls.Say(run, apocSwap, start, world, moving.Text, moving.Speech);
                }

                var marks = await run.WaitEvents(2, EventKind.ActorControl,
                    e => e.Id == SpinControl && e.Arg1 == SpinKind
                         && e.Arg2 is Clockwise or CounterClockwise);
                if (marks.Count < 2) return;

                var clockwise = marks[0].Arg2 == Clockwise;
                var opening = Opening(SectorOf(world, marks[0]), SectorOf(world, marks[1]));
                if (!opening.IsPoint()) return;

                var turn = TurnFor(opening, clockwise);
                var table = clockwise ? SpreadClockwise : SpreadCounter;

                var eruption = await run.FindOrWaitForCast(world, e => e.Id == DarkEruption);
                if (eruption is null) return;

                var spread = Lines(table, turn, "spread", "spread");
                SeatCalls.Say(run, apocSpread, eruption, world, spread.Text, spread.Speech);

                await run.WaitEvent(EventKind.AbilityHit, EruptionLands);
                var next = SoonestHeld(run, world, DarkWater);
                if (next is not null) run.Call(apocStacks, next);

                var dance = await run.FindOrWaitForCast(world, e => e.Id == DarkestDance);
                if (dance is null) return;

                if (swaps.Count > 0)
                {
                    var back = SwapLines(swaps, "Swap again", Called(world));
                    SeatCalls.Say(run, apocSwapBack, dance, world, back.Text, back.Speech);
                }

                var water = run.LongestHeld(world, DarkWater);
                if (water is null) return;
                run.Call(apocKnockback, water);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 4, new FruApoc(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
