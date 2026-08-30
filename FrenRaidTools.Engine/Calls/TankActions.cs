namespace FrenRaidTools.Engine;

public static class TankActions
{
    public const uint Provoke = 0x1D6D;
    public const uint Shirk = 0x1D71;

    public const double TimeoutSeconds = 10;

    public const string MechanicName = "Tank swaps";

    public static readonly Callout Provoked =
        (Callout.Of("Provoke", "{event.source} taunted") with
        {
            Key = "tank.provoke", Mechanic = MechanicName,
        }).Note("Tanks only. Confirms the swap the moment either tank presses Provoke.");

    public static readonly Callout Shirked =
        (Callout.Of("Shirk", "{event.source} shirked") with
        {
            Key = "tank.shirk", Mechanic = MechanicName,
        }).Note("Tanks only. Confirms enmity moved before the next buster.");

    public static Sequence Build(string group, IWorld world) =>
        Sequence.Indexed(group + "TankActions", TimeoutSeconds,
            e => e.Kind == EventKind.AbilityHit
                 && e.Id is Provoke or Shirk
                 && e.Source?.IsPlayer == true,
            (start, run, invocation) =>
            {
                if (JobKinds.Tanking(world))
                    run.Call(start.Id == Provoke ? Provoked : Shirked, start);
                return Task.CompletedTask;
            });
}
