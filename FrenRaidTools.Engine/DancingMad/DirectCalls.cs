namespace FrenRaidTools.Engine.DancingMad;

public sealed class DirectCalls
{
    public const string Group = "direct";

    public const int PhaseNumber = 1;

    public const string MechanicName = "Other";

    public const uint RevoltingRuinCast = 0xC403;
    public const uint P5EnrageCast = 0xBB3A;

    public readonly Callout revoltingRuinIII =
        Callout.Duration("Revolting Ruin III", "Buster on {event.target}");

    public readonly Callout p5enrage =
        Callout.Duration("P5 Enrage", "Enrage");

    public Sequence Build(IWorld world) =>
        Sequence.Indexed(Group, 30,
            e => e.Is(EventKind.CastStart, RevoltingRuinCast, P5EnrageCast),
            (start, run, invocation) =>
            {
                run.Call(start.Id == RevoltingRuinCast ? revoltingRuinIII : p5enrage, start);
                return Task.CompletedTask;
            });
}
