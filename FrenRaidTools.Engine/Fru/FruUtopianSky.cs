using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruUtopianSky
{
    public const string Group = "fru.utopianSky";

    public const string MechanicName = "Utopian Sky";

    public const string SequenceName = Group + ".safe";

    public const uint Stack = 0x9CDA;

    public const uint Spread = 0x9CDB;

    public const uint BlastingZone = 0x9CDE;

    public const uint MarkCategory = 0x003F;

    public const uint MarkData = 4;

    public const int Marks = 8;

    public const int Safe = 2;

    public const double GatherSeconds = 3.0;

    public const double TimeoutSeconds = 30.0;

    public static readonly Callout utopianSafe = new()
    {
        Description = "Utopian Sky",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "utopianSafe",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The clones each take a sector out of the ring and whatever is left over is "
                + "where the party goes, stacked or spread by which id opened the cast.\n"
                + "Nothing is said unless exactly two are left, so a pull that marks fewer "
                + "clones than the ring can spare stays quiet rather than naming a guess.",
    };

    public static List<ArenaSector> Left(IWorld world, IEnumerable<GameEvent> marks)
    {
        var ring = ArenaSectors.Clockwise.ToList();

        foreach (var mark in marks)
        {
            var at = FruArena.SectorOf(world, mark.Target);
            if (at.IsPoint()) ring.Remove(at);
        }

        return ring;
    }

    public static (string Text, string Speech) Words(bool stack, IReadOnlyList<ArenaSector> safe)
    {
        var what = stack ? "Stack" : "Spread";
        var shown = string.Join(" and ", safe.Select(s => s.Short()));
        var said = string.Join(" and ", safe.Select(s => s.Spoken()));
        return ($"{what} {shown}", $"{what} {said}");
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, Stack, Spread),
            async (start, run) =>
            {
                var stack = start.Id == Stack;

                var marks = await run.WaitEventsQuickSuccession(Marks,
                    e => e.Kind == EventKind.ActorControl && e.Id == MarkCategory
                         && e.Arg1 == MarkData,
                    GatherSeconds);
                if (marks.Count == 0) return;

                var safe = Left(world, marks);
                if (safe.Count != Safe) return;

                var words = Words(stack, safe);
                run.SetParam(SeatCalls.TextParam, words.Text);
                run.SetParam(SeatCalls.SpeechParam, words.Speech);
                run.Call(utopianSafe, start);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 1, new FruUtopianSky(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
