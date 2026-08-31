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
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The same turn, applied to the stack positions.",
    };

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

    public static readonly IReadOnlyList<Spot> AfterEruption =
    [
        Near(ArenaSector.North), Near(ArenaSector.North),
        Wall(ArenaSector.North, 0), Wall(ArenaSector.North, 0),
        Near(ArenaSector.South), Near(ArenaSector.South),
        Wall(ArenaSector.South, 0), Wall(ArenaSector.South, 0),
    ];

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

                var landed = await run.WaitEvent(EventKind.AbilityHit, EruptionLands);
                var stacks = Lines(AfterEruption, turn, "stack", "stack");
                SeatCalls.Say(run, apocStacks, landed ?? eruption, world,
                    stacks.Text, stacks.Speech);
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
