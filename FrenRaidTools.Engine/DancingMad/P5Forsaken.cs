namespace FrenRaidTools.Engine.DancingMad;

public sealed class P5Forsaken
{
    public const string Group = "p5Forsaken";

    public const int PhaseNumber = 5;

    public const string MechanicName = "P5 Forsaken";

    public const uint ForsakenCast = 0xBB35;
    public const uint MoveCast = 0xBB38;
    public const int Moves = 4;

    public const uint ExaflaresCast = 0xBB3B;
    public const uint StrayEntropyCast = 0xBB3E;

    public readonly Callout exaflares =
        Callout.Duration("Exaflares", "Exaflares");

    public readonly Callout strayEntropy =
        Callout.Duration("Stray Entropy", "Spread");

    public readonly Callout p5forsaken =
        Callout.Duration("P5 Forsaken", "Stack South, Raidwide");

    public readonly Callout p5forsakenMove =
        Callout.Duration("P5 Forsaken Move", "Move");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, ForsakenCast),
            async (start, run) =>
            {
                run.Call(p5forsaken, start);
                for (var i = 0; i < Moves; i++)
                {
                    var cast = await run.WaitEvent(EventKind.CastStart, MoveCast);
                    run.Call(p5forsakenMove, cast);
                }
            });

    public Sequence BuildCasts(IWorld world) =>
        CastCalls.For(Group + "Casts",
            (ExaflaresCast, exaflares),
            (StrayEntropyCast, strayEntropy));
}
