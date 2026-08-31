using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruFulgent
{
    public const string Group = "fru.fulgent";

    public const string MechanicName = "Fulgent Blade";

    public const string SequenceName = Group + ".waves";

    public const uint FulgentBlade = 0x9D72;

    public static readonly uint[] FirstWave = [0x9CB6, 0x9D73];

    public static readonly uint[] LaterWaves = [0x9D74, 0x9D75];

    public const uint WaveControl = 413;

    public const uint Lit = 1;

    public const uint Opens = 32;

    public const int LitCount = 6;

    public const int OpensCount = 2;

    public const int Waves = 7;

    public const double TimeoutSeconds = 60;

    public const double ApartSeconds = 1.0;

    public static readonly Callout fulgentTurn = new()
    {
        Description = "Fulgent Blade",
        Mechanic = MechanicName,
        Phase = 6,
        Key = "fulgentTurn",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Which way the waves sweep, read off the two that open against the six that "
                + "light up.\nSix of the eight points light, and the two that open are "
                + "always neighbours.",
    };

    public static readonly Callout fulgentMove = new()
    {
        Description = "Fulgent Blade",
        Mechanic = MechanicName,
        Phase = 6,
        Key = "fulgentMove",
        Speech = "Move",
        Text = "Move",
    };

    public static bool? Clockwise(
        IReadOnlyList<ArenaSector> lit, IReadOnlyList<ArenaSector> opens)
    {
        var later = lit.Where(s => s.IsPoint() && !opens.Contains(s)).ToList();

        foreach (var first in opens)
        {
            if (!first.IsPoint()) continue;
            foreach (var next in later)
            {
                var step = first.EighthsTo(next);
                if (step == 1) return true;
                if (step == ArenaSectors.Eighths - 1) return false;
            }
        }

        return null;
    }

    private static List<ArenaSector> Sectors(IWorld world, IReadOnlyList<GameEvent> got) =>
        got.Select(e => FruArena.SectorOf(world, e.Target, FruArena.Close)).ToList();

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, FulgentBlade),
            async (start, run) =>
            {
                var lit = await run.WaitEvents(LitCount, EventKind.ActorControl,
                    e => e.Id == WaveControl && e.Arg1 == Lit);
                if (lit.Count < LitCount) return;

                var opens = await run.WaitEvents(OpensCount, EventKind.ActorControl,
                    e => e.Id == WaveControl && e.Arg1 == Opens);
                if (opens.Count < OpensCount) return;

                var turn = Clockwise(Sectors(world, lit), Sectors(world, opens));
                if (turn is null) return;

                var first = await run.FindOrWaitForCast(world, e => FirstWave.Contains(e.Id));
                if (first is null) return;

                run.SetParam(SeatCalls.TextParam, turn.Value ? "Waves CW" : "Waves CCW");
                run.SetParam(SeatCalls.SpeechParam,
                    turn.Value ? "Waves clockwise" : "Waves counter clockwise");
                run.Call(fulgentTurn, first);

                var said = double.NegativeInfinity;

                for (var wave = 0; wave < Waves; wave++)
                {
                    var ids = wave == 0 ? FirstWave : LaterWaves;
                    GameEvent? next;
                    do
                    {
                        next = await run.WaitEvent(EventKind.AbilityHit,
                            e => ids.Contains(e.Id));
                        if (next is null) return;
                    }
                    while (next.At - said < ApartSeconds);

                    said = next.At;
                    run.Call(fulgentMove, next);
                }
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 6, new FruFulgent(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
