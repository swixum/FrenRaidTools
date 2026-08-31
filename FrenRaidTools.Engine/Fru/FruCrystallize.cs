using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruCrystallize
{
    public const string Group = "fru.crystallize";

    public const string MechanicName = "Crystallize Time";

    public const string SequenceName = Group + ".lights";

    public const uint CrystallizeTime = 0x9D30;

    public const uint LightTether = 0x85;

    public const uint SorrowsHourglass = 17837;

    public const uint FirstLights = 0x9D6B;

    public const uint TidalLight = 0x9D3B;

    public const int LongLights = 2;

    public const string ClawSequenceName = Group + ".claws";

    public const uint Wyrmclaw = 0xCBF;

    public const int Claws = 4;

    public const double AeroSeconds = 25;

    public const double TimeoutSeconds = 90;

    public static readonly Callout crystallizeKnockback = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeKnockback",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The two long lights are tethered and the two short ones are always north "
                + "and south. Whichever long light sits beside north is the side the "
                + "knockback leaves safe.",
    };

    public static readonly Callout crystallizeRewind = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeRewind",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The two tidal waves come from two sides, and the corner between them is "
                + "where the rewind goes.",
    };

    public static readonly Callout crystallizeClaw = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeClaw",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The four claws are two pairs, aero and ice, and each pair takes one head "
                + "each.\n"
                + "Which head is yours is your place in the group's claw order.",
    };

    public static (string Text, string Speech) ClawWords(bool west)
    {
        var side = west ? "West" : "East";
        return ($"{side} head", $"Intercept {side.ToLowerInvariant()} head");
    }

    public static int Partner(IWorld world, IReadOnlyList<GameEvent> claws, int seat)
    {
        var mine = claws.FirstOrDefault(
            c => c.Target is not null && world.SeatOf(c.Target) == seat);
        if (mine is null) return -1;

        foreach (var claw in claws)
        {
            if (claw.Target is null || ReferenceEquals(claw, mine)) continue;
            if (IsAero(claw) != IsAero(mine)) continue;
            var other = world.SeatOf(claw.Target);
            if (other >= 0 && other != seat) return other;
        }

        return -1;
    }

    public static bool IsAero(GameEvent claw) => claw.Duration > AeroSeconds;

    public static ArenaSector Safe(IReadOnlyList<ArenaSector> longLights)
    {
        var beside = longLights
            .Where(s => s.IsPoint() && s.IsStrictlyAdjacentTo(ArenaSector.North))
            .ToList();

        return beside.Count == 1 ? beside[0] : ArenaSector.Unknown;
    }

    private static bool IsLight(Actor? actor) =>
        actor is not null && actor.BaseId == SorrowsHourglass;

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, CrystallizeTime),
            async (start, run) =>
            {
                var tethers = await run.WaitEvents(LongLights, EventKind.Tether,
                    e => e.Id == LightTether && (IsLight(e.Source) || IsLight(e.Target)));
                if (tethers.Count < LongLights) return;

                var lights = tethers
                    .Select(t => FruArena.SectorOf(
                        world, IsLight(t.Source) ? t.Source : t.Target, FruArena.Middle))
                    .ToList();

                var safe = Safe(lights);
                if (!safe.IsPoint()) return;

                var pushed = await run.WaitEvent(EventKind.AbilityHit, FirstLights);
                if (pushed is null) return;

                run.SetParam(SeatCalls.TextParam, $"Pushed to {safe.Short()}");
                run.SetParam(SeatCalls.SpeechParam, $"Get pushed {safe.Spoken()}");
                run.Call(crystallizeKnockback, pushed);

                var first = await run.FindOrWaitForCast(world, e => e.Id == TidalLight);
                if (first is null) return;
                var one = FruArena.SectorOf(world, first.Source);

                var second = await run.WaitEvent(EventKind.CastStart, TidalLight);
                if (second is null) return;
                var two = FruArena.SectorOf(world, second.Source);

                var corner = ArenaSectors.Between(one, two);
                if (!corner.IsPoint()) return;

                run.SetParam(SeatCalls.TextParam, $"Drop rewind {corner.Short()}");
                run.SetParam(SeatCalls.SpeechParam, $"Drop rewind {corner.Spoken()}");
                run.Call(crystallizeRewind, second);
            });

    public static Sequence Claw(IWorld world) =>
        Sequence.Repeat(ClawSequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, CrystallizeTime),
            async (start, run) =>
            {
                var claws = await run.WaitEvents(Claws, EventKind.StatusGain,
                    e => e.Id == Wyrmclaw);
                if (claws.Count < Claws) return;

                var seat = SeatCalls.MySeat(world);
                var other = Partner(world, claws, seat);
                var west = FruAssignments.ClawWest(seat, other);
                if (west is null) return;

                var (text, speech) = ClawWords(west.Value);
                run.SetParam(SeatCalls.TextParam, text);
                run.SetParam(SeatCalls.SpeechParam, speech);
                run.Call(crystallizeClaw, claws[^1]);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 5, new FruCrystallize(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world), Claw(world)],
        });
}
