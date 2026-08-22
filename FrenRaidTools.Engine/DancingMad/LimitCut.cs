namespace FrenRaidTools.Engine.DancingMad;

public sealed class LimitCut
{
    public const string Group = "limitCut";

    public const uint StartCast = 0xBAF2;
    public const uint CloneHit = 0xBAE3;

    public static readonly ArenaPos Wide = new(100, 100, 10, 10);

    public readonly Callout limitCutInitial =
        Callout.Of("Limit Cut: Initial", "Starting {resultingStart} -> {resultingClockwise ? 'Clockwise' : 'CCW'}")
            .Note("The four variables you can use in this call are initialClone and initialClockwise which are the first clone and which direction the initial waves are going. resultingStart and resultingClockwise are where the limit cut hits will start from and which direction.");

    public readonly Callout limitCutNumber1 =
        Callout.Of("Limit Cut: 1", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }")
            .Note("For the individual number calls, you can use {myNumber} which is your limit cut number, starting at 1, in case you want to do math in the expressions. You can also use {myPosition} for the arena position opposite your clone. For example, { myPosition } { resultingClockwise ? 'Left' : 'Right' } would call out something like North Right (left/right is looking inwards) for the typical LC strategy.");
    public readonly Callout limitCutNumber2 = Callout.Of("Limit Cut: 2", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout limitCutNumber3 = Callout.Of("Limit Cut: 3", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout limitCutNumber4 = Callout.Of("Limit Cut: 4", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout limitCutNumber5 = Callout.Of("Limit Cut: 5", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout limitCutNumber6 = Callout.Of("Limit Cut: 6", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout limitCutNumber7 = Callout.Of("Limit Cut: 7", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout limitCutNumber8 = Callout.Of("Limit Cut: 8", "{myNumber} { myPosition } { resultingClockwise ? 'CW' : 'CCW' }");
    public readonly Callout unknown = Callout.Of("Limit Cut: Error", "Error");

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
        var hit1 = await run.WaitEvent(EventKind.AbilityHit, e => e.Id == CloneHit && e.FirstTarget);
        var hit2 = await run.WaitEvent(EventKind.AbilityHit, e => e.Id == CloneHit && e.FirstTarget);
        await run.Settle();

        var from1 = Wide.For(Fresh(hit1.Source, world));
        var from2 = Wide.For(Fresh(hit2.Source, world));

        run.SetParam("initialClone", from1.Told());

        bool? resultingClockwise = null;
        var resultingStart = ArenaSector.Unknown;

        if (from1.IsStrictlyAdjacentTo(from2))
        {
            var initialClockwise = from1.EighthsTo(from2) == 1;
            run.SetParam("initialClockwise", initialClockwise);

            resultingClockwise = !initialClockwise;
            run.SetParam("resultingClockwise", resultingClockwise);

            resultingStart = from1.Opposite();
            run.SetParam("resultingStart", resultingStart.Told());
        }

        run.Call(limitCutInitial);

        var myMarker = await run.WaitEvent(EventKind.HeadMarker, e => e.Target?.IsYou == true);
        var myNumber = NumberFor(myMarker.Id);
        run.SetParam("myNumber", myNumber);

        if (resultingClockwise is { } clockwise && myNumber > 0)
        {
            var step = (myNumber - 1) * (clockwise ? 1 : -1);
            run.SetParam("myPosition", resultingStart.PlusEighths(step).Told());
        }
        else
        {
            run.SetParam("myPosition", null);
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
        }, myMarker);
    }

    private static Actor? Fresh(Actor? actor, IWorld world) =>
        actor is null ? null : world.Latest(actor) ?? actor;
}
