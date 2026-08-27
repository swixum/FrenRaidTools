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

    public const uint ProvokeAction = 0x1D6D;
    public const uint ShirkAction = 0x1D71;

    public readonly Callout provoked =
        Callout.Of("Provoke", "{event.source} Taunted")
            .Note("Tanks only. Confirms the swap the moment either tank presses Provoke.");

    public readonly Callout shirked =
        Callout.Of("Shirk", "{event.source} Shirked");

    public Sequence Build(IWorld world) =>
        Sequence.Indexed(Group, 30,
            e => e.Is(EventKind.CastStart, RevoltingRuinCast, P5EnrageCast),
            (start, run, invocation) =>
            {
                run.Call(start.Id == RevoltingRuinCast ? revoltingRuinIII : p5enrage, start);
                return Task.CompletedTask;
            });

    public Sequence BuildTankActions(IWorld world) =>
        Sequence.Indexed(Group + "TankActions", 10,
            e => e.Kind == EventKind.AbilityHit
                 && e.Id is ProvokeAction or ShirkAction
                 && e.Source?.IsPlayer == true,
            (start, run, invocation) =>
            {
                if (JobKinds.Tanking(world))
                    run.Call(start.Id == ProvokeAction ? provoked : shirked, start);
                return Task.CompletedTask;
            });
}
