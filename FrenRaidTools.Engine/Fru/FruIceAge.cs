using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruIceAge
{
    public const string Group = "fru.iceAge";

    public const string MechanicName = "Endless Ice Age";

    public const string SequenceName = Group + ".crystals";

    public const uint EndlessIceAge = 0x9D43;

    public const uint CrystalTether = 0x0054;

    public const uint VulnerabilityDown = 0x0896;

    public const double CastSeconds = 39.7;

    public const double VulnSeconds = 35.0;

    public const double TimeoutSeconds = 45.0;

    public static readonly Callout iceAgeTether = new()
    {
        Description = "Endless Ice Age",
        Mechanic = MechanicName,
        Phase = 3,
        Key = "iceAgeTether",
        FromPlan = true,
        Speech = "Tether",
        Text = "Tether",
        Notes = "Only the four the crystal tethers hear this.",
    };

    public static readonly Callout iceAgeMainCrystal = new()
    {
        Description = "Endless Ice Age",
        Mechanic = MechanicName,
        Phase = 3,
        Key = "iceAgeMainCrystal",
        FromPlan = true,
        Speech = "Kill main crystal",
        Text = "Kill main crystal" + Callout.CountdownToken,
        FromDuration = true,
        HoldsToCountdown = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Which crystal is read off the vulnerability landing on it, which nothing "
                + "casts.\n"
                + "The number is the Ice Veil's own Endless Ice Age cast bar, which wipes "
                + "the party if it finishes, so it counts down to the wipe and not to the "
                + "moment the crystal was named.",
    };

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, EndlessIceAge),
            async (start, run) =>
            {
                var deadline = run.Now + VulnSeconds;
                var tethered = false;

                while (run.Now < deadline)
                {
                    var got = await run.WaitEventUntil(
                        e => (e.Kind == EventKind.Tether && e.Id == CrystalTether
                              && FruArena.Mine(e, world))
                             || (e.Kind == EventKind.StatusGain && e.Id == VulnerabilityDown),
                        deadline);
                    if (got is null) return;

                    if (got.Kind == EventKind.Tether)
                    {
                        if (!tethered) run.Call(iceAgeTether, got);
                        tethered = true;
                        continue;
                    }

                    run.Call(iceAgeMainCrystal, start);
                    return;
                }
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 3, new FruIceAge(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
