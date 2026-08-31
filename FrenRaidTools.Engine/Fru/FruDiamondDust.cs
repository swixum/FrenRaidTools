using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruDiamondDust
{
    public const string Group = "fru.diamondDust";

    public const string MechanicName = "Diamond Dust";

    public const string SequenceName = Group + ".kicks";

    public const uint DiamondDust = 0x9D05;

    public const uint AxeKick = 0x9D0A;

    public const uint ScytheKick = 0x9D0B;

    public const uint IcicleImpact = 0x9D06;

    public const uint FrigidNeedle = 0x9D08;

    public const string GazeSequenceName = Group + ".gaze";

    public const uint SinboundHoly = 0x9D10;

    public const double GazeMs = 10250;

    public const int Markers = 4;

    public const double TimeoutSeconds = 30;

    public static readonly Callout diamondSwap = new()
    {
        Description = "Diamond Dust",
        Mechanic = MechanicName,
        Phase = 2,
        Key = "diamondSwap",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Answered from two things the pull rolls: whether the head marker is on you, "
                + "and whether the first ice circles land on the letters or the numbers.\n"
                + "Marked goes to the other set, unmarked closes on the circles.",
    };

    public static readonly Callout diamondKick = new()
    {
        Description = "Diamond Dust",
        Mechanic = MechanicName,
        Phase = 2,
        Key = "diamondKick",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "In or out by the kick, plus the puddle drop if you are the one marked.",
    };

    public static readonly Callout diamondKnockback = new()
    {
        Description = "Diamond Dust",
        Mechanic = MechanicName,
        Phase = 2,
        Key = "diamondKnockback",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The knockback is safe into the first ice circles, which are named here "
                + "not left as red or purple.",
    };

    public static readonly Callout diamondGaze = new()
    {
        Description = "Diamond Dust",
        Mechanic = MechanicName,
        Phase = 2,
        Key = "diamondGaze",
        FromPlan = true,
        Speech = "Look away",
        Text = "Look away",
        Notes = "The eye lands with nothing casting it, so the moment is measured from "
                + "Sinbound Holy, which it follows by 14.25 seconds on both clears.\n"
                + "No side is named: the Usurper's position is not reported until "
                + "half a second before the eye, so any side said in time to act on "
                + "would be read off a stale spot.",
    };

    public static Sequence Gaze(IWorld world) =>
        Sequence.Repeat(GazeSequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, SinboundHoly),
            async (start, run) =>
            {
                await run.WaitMs(GazeMs);

                run.Call(diamondGaze, start);
            });

    public static List<ArenaSector> FirstIces(IWorld world) =>
        world.ActiveCasts()
            .Where(c => c.Id == IcicleImpact)
            .Select(c => FruArena.SectorOf(world, c.Source))
            .Where(s => s.IsPoint())
            .Distinct()
            .OrderBy(s => (int)s)
            .ToList();

    public static (string Text, string Speech) SwapWords(bool marked, bool onLetters) =>
        (marked, onLetters) switch
        {
            (false, true) => ("Unmarked, letters", "Unmarked, close on the letters"),
            (true, true) => ("Marked, numbers", "Marked, go to the numbers"),
            (false, false) => ("Unmarked, numbers", "Unmarked, close on the numbers"),
            _ => ("Marked, letters", "Marked, go to the letters"),
        };

    public static bool? SupportsOnCardinals(IReadOnlyList<int> marked, bool circlesOnLetters)
    {
        if (marked.Count == 0) return null;

        var allDps = marked.All(seat => !Slots.IsSupport(seat));
        var allSupports = marked.All(Slots.IsSupport);
        if (allDps == allSupports) return null;

        return allDps == circlesOnLetters;
    }

    public static (string Text, string Speech) SpotWords(bool marked, ArenaSector spot)
    {
        var what = marked ? "drop ice" : "close in";
        return ($"{spot.Short()}, {what}",
                $"{spot.Name()} at {spot.SpokenMark()}, {what}");
    }

    public static List<int> MarkedSeats(IWorld world, IReadOnlyList<GameEvent> marks)
    {
        var seats = new List<int>();
        foreach (var mark in marks)
        {
            if (mark.Target is null) continue;
            var seat = world.SeatOf(mark.Target);
            if (seat >= 0 && !seats.Contains(seat)) seats.Add(seat);
        }

        return seats;
    }

    public static (string Text, string Speech) KickWords(bool axe, bool marked) =>
        (axe, marked) switch
        {
            (true, false) => ("Out, far safe", "Out, far is safe"),
            (true, true) => ("Out, drop puddle", "Out, drop puddle"),
            (false, false) => ("In, close safe", "In, close is safe"),
            _ => ("In, drop puddle", "In, drop puddle"),
        };

    public static (string Text, string Speech) KnockbackWords(IReadOnlyList<ArenaSector> ices)
    {
        if (ices.Count == 0) return ("Knockback in", "Knockback in to the safe circles");

        var shown = string.Join(" and ", ices.Select(s => s.Short()));
        var said = string.Join(" and ", ices.Select(s => s.Spoken()));
        return ($"Knockback to {shown}", $"Knockback to {said}");
    }

    private static ArenaSector Spot(IWorld world, IReadOnlyList<GameEvent> marks, bool letters)
    {
        var seat = SeatCalls.MySeat(world);
        var supports = SupportsOnCardinals(MarkedSeats(world, marks), letters);
        return supports is null ? ArenaSector.Unknown
            : FruAssignments.DiamondDustSpot(seat, supports.Value);
    }

    private static void Say(SequenceRun run, Callout call, GameEvent on,
                            (string Text, string Speech) words)
    {
        run.SetParam(SeatCalls.TextParam, words.Text);
        run.SetParam(SeatCalls.SpeechParam, words.Speech);
        run.Call(call, on);
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, DiamondDust),
            async (dust, run) =>
            {
                var start = await run.WaitEvent(EventKind.CastStart, AxeKick, ScytheKick);
                if (start is null) return;

                var axe = start.Id == AxeKick;

                var marks = await run.WaitEvents(Markers, EventKind.HeadMarker, e => true);
                if (marks.Count < Markers) return;

                var marked = marks.Any(m => FruArena.Mine(m, world));
                var ices = FirstIces(world);
                if (ices.Count == 0) return;

                var letters = ices.All(s => s.IsCardinal());
                var spot = Spot(world, marks, letters);

                Say(run, diamondSwap, start,
                    spot.IsPoint() ? SpotWords(marked, spot) : SwapWords(marked, letters));
                await run.WaitCastFinished(start);
                Say(run, diamondKick, start, KickWords(axe, marked));

                var needle = await run.FindOrWaitForCast(world, e => e.Id == FrigidNeedle);
                if (needle is null) return;
                Say(run, diamondKnockback, needle, KnockbackWords(ices));
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 2, new FruDiamondDust(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world), Gaze(world)],
        });
}
