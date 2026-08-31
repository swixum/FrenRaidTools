using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruTurn
{
    public const string Group = "fru.turn";

    public const string MechanicName = "Turn of the Heavens";

    public const string SequenceName = Group + ".knockback";

    public const uint BlueSafe = 0x9CD6;

    public const uint RedSafe = 0x9CD7;

    public const uint WideLightning = 0x9CE3;

    public const uint Burnout = 0x9CE1;

    public const uint RedHalo = 17821;

    public const uint BlueHalo = 17822;

    public static readonly uint[] Tethers = [0x00F9, 0x011F];

    public const double TimeoutSeconds = 40;

    public const int Tries = 40;

    public const int WaitMs = 200;

    public static readonly Callout turnKnockback = new()
    {
        Description = "Turn of the Heavens",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "turnKnockback",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The safe side is whichever halo of the safe colour is still standing east "
                + "or west when the knockback casts.\n"
                + "A tether on you adds whether it is the close one or the far one.",
    };

    public static ArenaSector SafeSide(IWorld world, bool red)
    {
        var sides = world.NpcsById(red ? RedHalo : BlueHalo)
            .Select(n => FruArena.SectorOf(world, n))
            .Where(s => s is ArenaSector.East or ArenaSector.West)
            .Distinct()
            .ToList();

        return sides.Count == 1 ? sides[0] : ArenaSector.Unknown;
    }

    public static (string Text, string Speech) Words(ArenaSector safe, ArenaSector tether)
    {
        var shown = safe.Short();
        var said = safe.Spoken();

        if (tether.IsPoint() && tether.IsStrictlyAdjacentTo(safe))
            return ($"Knocked to {shown}, close tether",
                    $"Get knocked {said}, close tether");

        if (tether.IsPoint() && tether.IsStrictlyAdjacentTo(safe.Opposite()))
            return ($"Knocked to {shown}, far tether", $"Get knocked {said}, far tether");

        return ($"Knocked to {shown}", $"Get knocked {said}");
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, BlueSafe, RedSafe),
            async (start, run) =>
            {
                var red = start.Id == RedSafe;

                var wide = await run.FindOrWaitForCast(world, e => e.Id == WideLightning);
                if (wide is null) return;

                var tethers = await run.WaitEvents(2, EventKind.Tether,
                    e => Tethers.Contains(e.Id));

                var burnout = await run.FindOrWaitForCast(world, e => e.Id == Burnout);
                if (burnout is null) return;

                var safe = ArenaSector.Unknown;
                for (var tries = 0; tries < Tries && !safe.IsPoint(); tries++)
                {
                    safe = SafeSide(world, red);
                    if (!safe.IsPoint()) await run.WaitMs(WaitMs);
                }

                if (!safe.IsPoint()) return;

                var mine = tethers.FirstOrDefault(t => FruArena.Mine(t, world));
                var from = mine is null
                    ? ArenaSector.Unknown
                    : FruArena.SectorOf(world, mine.Source);

                var (text, speech) = Words(safe, from);
                run.SetParam(SeatCalls.TextParam, text);
                run.SetParam(SeatCalls.SpeechParam, speech);
                run.Call(turnKnockback, burnout);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 1, new FruTurn(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
