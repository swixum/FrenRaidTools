namespace FrenRaidTools.Engine.DancingMad;

public sealed class LimitCut
{
    public const string Group = "limitCut";

    public const uint StartCast = 0xBAF2;
    public const uint CloneHit = 0xBAE3;

    public static readonly ArenaPos Wide = new(100, 100, 10, 10);

    public readonly Callout limitCutInitial =
        Callout.Of("Limit Cut: Initial",
            "{startWaymark} - {resultingClockwise ? 'Clockwise' : 'Counterclockwise'}",
            "{startWaymark} - {resultingClockwise ? 'CW' : 'CCW'}")
            .Note("The four variables you can use in this call are initialClone and initialClockwise which are the first clone and which direction the initial waves are going. resultingStart and resultingClockwise are where the limit cut hits will start from and which direction.")
            .At("Start and rotation");

    public const string WaymarkParam = "myWaymark";
    public const string StartWaymarkParam = "startWaymark";

    public const double ResolveAfterNumbersSeconds = 12.24;
    public const double PerNumberSeconds = 0.22;

    public static double Countdown(int number) =>
        number < 1 || number > Numbers
            ? 0
            : ResolveAfterNumbersSeconds + (number - 1) * PerNumberSeconds;

    public const int Numbers = 8;

    public const int CloneReads = 8;

    public static int? StepBetween(int dashesApart, ArenaSector first, ArenaSector second)
    {
        if (dashesApart <= 0 || !first.IsPoint() || !second.IsPoint()) return null;

        var delta = first.EighthsTo(second);
        var clockwise = Turns(1, dashesApart) == delta;
        var widdershins = Turns(-1, dashesApart) == delta;

        return clockwise == widdershins ? null : clockwise ? 1 : -1;
    }

    private static int Turns(int step, int dashes) =>
        (step * dashes % ArenaSectors.Eighths + ArenaSectors.Eighths) % ArenaSectors.Eighths;

    public static (ArenaSector Origin, bool Clockwise)? Solve(
        IReadOnlyList<(int Index, ArenaSector Where)> reads)
    {
        if (reads.Count < 2) return null;

        var first = reads[0];
        var last = reads[^1];

        if (StepBetween(last.Index - first.Index, first.Where, last.Where) is not { } step) return null;

        return (first.Where.PlusEighths(-step * first.Index), step > 0);
    }

    public static string? Spot(ArenaSector start, bool clockwise, int number)
    {
        if (!start.IsPoint() || number < 1 || number > Numbers) return null;

        var step = clockwise ? number - 1 : Numbers - number;
        var first = start.PlusEighths(step).Waymark();
        var second = start.PlusEighths(step + 1).Waymark();

        return first is null || second is null ? null : first + second;
    }

    public readonly Callout limitCutNumber1 =
        Callout.Duration("Limit Cut: 1", "{myNumber} - {myWaymark}")
            .Note("For the individual number calls, you can use {myNumber} which is your limit cut number, starting at 1, in case you want to do math in the expressions. You can also use {myPosition} for the arena position opposite your clone. For example, { myPosition } { resultingClockwise ? 'Left' : 'Right' } would call out something like North Right (left/right is looking inwards) for the typical LC strategy.")
            .At("Your number");
    public readonly Callout limitCutNumber2 = Callout.Duration("Limit Cut: 2", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout limitCutNumber3 = Callout.Duration("Limit Cut: 3", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout limitCutNumber4 = Callout.Duration("Limit Cut: 4", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout limitCutNumber5 = Callout.Duration("Limit Cut: 5", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout limitCutNumber6 = Callout.Duration("Limit Cut: 6", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout limitCutNumber7 = Callout.Duration("Limit Cut: 7", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout limitCutNumber8 = Callout.Duration("Limit Cut: 8", "{myNumber} - {myWaymark}")
        .At("Your number");
    public readonly Callout unknown = Callout.Of("Limit Cut: Error", "Error").WhenReadFails()
        .At("Your number");

    public static int NumberFor(uint markerId) => markerId switch
    {
        336 => 1,
        337 => 2,
        338 => 3,
        339 => 4,
        437 => 5,
        438 => 6,
        439 => 7,
        440 => 8,
        _ => -1,
    };

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, StartCast),
            (start, run) => Run(start, run, world));

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        var reads = new List<(int Index, ArenaSector Where)>();
        (ArenaSector Origin, bool Clockwise)? clones = null;
        GameEvent? marked = null;

        for (var read = 0; read < CloneReads && clones is null && marked is null; read++)
        {
            var next = await run.WaitEvent(e => IsCloneRead(e) || IsMyNumber(e));
            if (IsMyNumber(next))
            {
                marked = next;
                break;
            }

            await run.Settle();

            var where = Wide.For(Fresh(next.Source, world));
            if (!where.IsPoint()) continue;

            reads.Add((read, where));
            clones = Solve(reads);
        }

        bool? resultingClockwise = null;
        var resultingStart = ArenaSector.Unknown;

        run.SetParam("initialClone", clones?.Origin.Told());

        if (clones is { } dash)
        {
            run.SetParam("initialClockwise", dash.Clockwise);

            resultingClockwise = !dash.Clockwise;
            run.SetParam("resultingClockwise", resultingClockwise);

            resultingStart = dash.Origin.Opposite();
            run.SetParam("resultingStart", resultingStart.Told());
            run.SetParam(StartWaymarkParam, resultingStart.Waymark());
        }

        run.Call(limitCutInitial);

        var myMarker = marked ?? await run.WaitEvent(IsMyNumber);
        var myNumber = NumberFor(myMarker.Id);
        run.SetParam("myNumber", myNumber);

        if (resultingClockwise is { } clockwise && myNumber > 0)
        {
            var step = (myNumber - 1) * (clockwise ? 1 : -1);
            run.SetParam("myPosition", resultingStart.PlusEighths(step).Told());
            run.SetParam(WaymarkParam, Spot(resultingStart, clockwise, myNumber));
        }
        else
        {
            run.SetParam("myPosition", null);
            run.SetParam(WaymarkParam, null);
        }

        run.Call(myNumber switch
        {
            1 => limitCutNumber1,
            2 => limitCutNumber2,
            3 => limitCutNumber3,
            4 => limitCutNumber4,
            5 => limitCutNumber5,
            6 => limitCutNumber6,
            7 => limitCutNumber7,
            8 => limitCutNumber8,
            _ => unknown,
        }, myMarker with { Duration = Countdown(myNumber) });
    }

    private static bool IsCloneRead(GameEvent e) =>
        e.Kind == EventKind.AbilityHit && e.Id == CloneHit && e.FirstTarget;

    private static bool IsMyNumber(GameEvent e) =>
        e.Kind == EventKind.HeadMarker && e.Target?.IsYou == true && NumberFor(e.Id) > 0;

    private static Actor? Fresh(Actor? actor, IWorld world) =>
        actor is null ? null : world.Latest(actor) ?? actor;
}
