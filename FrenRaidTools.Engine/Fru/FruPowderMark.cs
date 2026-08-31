using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruPowderMark
{
    public const string Group = "fru.powderMark";

    public const string MechanicName = "Powder Mark Trail";

    public const string SequenceName = Group + ".mark";

    public const uint PowderMarkTrail = 0x9CE8;

    public const uint PowderMark = 0x1046;

    public const double MarkLead = 3.0;

    public const double FindSeconds = 10.0;

    public const double TimeoutSeconds = 40.0;

    public static readonly Callout powderMarkSoon = new()
    {
        Description = "Powder Mark Trail",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "powderMarkSoon",
        FromPlan = true,
        Speech = "Tanks together",
        Text = "Tanks together" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The mark sits on both tanks and bursts when it runs out, hitting whoever "
                + "is nearest, so the pair stands one notch off the wall toward each other "
                + "and everybody else stays off them.\n"
                + "Nothing casts the burst, so this is read off the mark's own timer.",
    };

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, PowderMarkTrail),
            async (start, run) =>
            {
                var mark = await run.FindOrWaitForStatusWithin(
                    world, e => e.Id == PowderMark && e.Target is not null, FindSeconds);
                if (mark is null) return;

                await run.WaitSeconds(run.Remaining(mark) - MarkLead);
                run.Call(powderMarkSoon, mark);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 1, new FruPowderMark(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
