using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruRelativity
{
    public const string Group = "fru.relativity";

    public const string MechanicName = "Ultimate Relativity";

    public const string SequenceName = Group + ".spots";

    public const uint UltimateRelativity = 0x9D4A;

    public const uint Speed = 0x9D65;

    public const uint SinboundMeltdown = 0x9D63;

    public const uint HourglassTether = 0x86;

    public const uint Hourglass = 17832;

    public const uint DarkFire = 0x997;

    public const uint DarkBlizzard = 0x99E;

    public const uint Return = 0x9A0;

    public const double EarlyReturn = 16.0;

    public const double LateReturn = 26.0;

    public const double TimeoutSeconds = 90;

    public const double DurationSlack = 2.0;

    public const string GazeSequenceName = Group + ".gaze";

    public const uint Shadoweye = 0x0998;

    public const double GazeLead = 4.0;

    public const double GazeFindSeconds = 20.0;

    public static readonly Callout relativityGaze = new()
    {
        Description = "Ultimate Relativity",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "relativityGaze",
        FromPlan = true,
        Speech = "Look outside",
        Text = "Look outside" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The eye at the end of the phase goes off on its own timer with nothing "
                + "casting it, so it is read off the Spell-in-Waiting itself.\n"
                + "Facing away from the middle answers it for all eight.",
    };

    public static Sequence Gaze(IWorld world) =>
        Sequence.Repeat(GazeSequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, UltimateRelativity),
            async (start, run) =>
            {
                var eye = await run.FindOrWaitForStatusWithin(
                    world, e => e.Id == Shadoweye && e.Target is not null, GazeFindSeconds);
                if (eye is null) return;

                await run.WaitSeconds(run.Remaining(eye) - GazeLead);
                run.Call(relativityGaze, eye);
            });

    public const int Rounds = 3;

    public const double ApartSeconds = 5.0;

    public const int ToNextMs = 6000;

    public const int ToFinalMs = 3700;

    public enum Depth { Middle, Halfway, Bait, Wall }

    public enum Slot { ShortDps, ShortSupport, MediumDps, MediumSupport, LongSupport, LongDps }

    private static readonly Dictionary<Slot, ArenaSector[]> Bearing = new()
    {
        [Slot.ShortDps] = [ArenaSector.Southwest, ArenaSector.Southeast],
        [Slot.ShortSupport] = [ArenaSector.North],
        [Slot.MediumDps] = [ArenaSector.East],
        [Slot.MediumSupport] = [ArenaSector.West],
        [Slot.LongSupport] = [ArenaSector.Northwest, ArenaSector.Northeast],
        [Slot.LongDps] = [ArenaSector.South],
    };

    public const int MomentCount = 7;

    public const int LastMoment = MomentCount - 1;

    public const string Home = "Go middle";

    public const string Mark = "Ice";

    public const string IceWords = "Drop ice middle";

    public const string Centre = "middle";

    public const int IceDrop = 2;

    private static readonly Dictionary<Slot, Depth[]> Depths = new()
    {
        [Slot.ShortDps] =
        [
            Depth.Wall, Depth.Halfway, Depth.Middle, Depth.Bait,
            Depth.Middle, Depth.Middle, Depth.Middle,
        ],
        [Slot.ShortSupport] =
        [
            Depth.Wall, Depth.Halfway, Depth.Middle, Depth.Bait,
            Depth.Middle, Depth.Middle, Depth.Middle,
        ],
        [Slot.MediumDps] =
        [
            Depth.Middle, Depth.Middle, Depth.Wall, Depth.Halfway,
            Depth.Middle, Depth.Bait, Depth.Middle,
        ],
        [Slot.MediumSupport] =
        [
            Depth.Middle, Depth.Halfway, Depth.Wall, Depth.Halfway,
            Depth.Middle, Depth.Bait, Depth.Middle,
        ],
        [Slot.LongSupport] =
        [
            Depth.Middle, Depth.Bait, Depth.Middle, Depth.Middle,
            Depth.Wall, Depth.Middle, Depth.Middle,
        ],
        [Slot.LongDps] =
        [
            Depth.Middle, Depth.Bait, Depth.Middle, Depth.Middle,
            Depth.Wall, Depth.Middle, Depth.Middle,
        ],
    };

    public static readonly Callout relativitySpot = new()
    {
        Description = "Ultimate Relativity",
        Mechanic = MechanicName,
        Phase = 4,
        Key = "relativitySpot",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Answered from the fire you are holding and whether you are a support.\n"
                + "The whole mechanic turns with the hourglasses; every direction is "
                + "read off them.",
    };

    public static ArenaSector NorthOf(IReadOnlyList<ArenaSector> glasses)
    {
        if (glasses.Count != 3 || glasses.Any(g => !g.IsPoint())) return ArenaSector.Unknown;

        foreach (var one in glasses)
        {
            var flanked = glasses.All(other =>
            {
                var step = one.EighthsTo(other);
                return step is 0 or 3 or ArenaSectors.Eighths - 3;
            });
            if (flanked) return one.Opposite();
        }

        return ArenaSector.Unknown;
    }

    private static Func<uint, double?> Holding(IWorld world, Actor you)
    {
        var mine = world.ActiveStatuses()
            .Where(s => s.Target is not null && s.Target.ObjectId == you.ObjectId)
            .ToList();

        return status =>
            mine.Where(s => s.Id == status).Select(s => (double?)s.Duration).FirstOrDefault();
    }

    public static string? MarkOf(IWorld world)
    {
        if (world.You is null) return null;

        var held = Holding(world, world.You);
        if (held(DarkFire) is { } fire)
        {
            if (Near(fire, 11)) return "Short fire";
            if (Near(fire, 21)) return "Medium fire";
            if (Near(fire, 31)) return "Long fire";
        }

        return held(DarkBlizzard) is { } ice && Near(ice, 21) ? Mark : null;
    }

    public static int? RewindMoment(IWorld world)
    {
        if (world.You is null) return null;

        var back = Holding(world, world.You)(Return);
        if (back is not { } seconds) return null;

        if (Near(seconds, EarlyReturn)) return 1;

        return Near(seconds, LateReturn) ? 3 : null;
    }

    public static Slot? SlotOf(IWorld world) =>
        world.You is null ? null : SlotOf(world, world.You);

    public static Slot? SlotOf(IWorld world, Actor you)
    {
        var held = Holding(world, you);

        var seat = world.SeatOf(you);
        var support = seat >= 0 ? Slots.IsSupport(seat) : you.Support;
        var fire = held(DarkFire);
        if (fire is { } seconds)
        {
            if (Near(seconds, 11)) return support ? Slot.ShortSupport : Slot.ShortDps;
            if (Near(seconds, 21)) return support ? Slot.MediumSupport : Slot.MediumDps;
            if (Near(seconds, 31)) return support ? Slot.LongSupport : Slot.LongDps;
        }

        if (held(DarkBlizzard) is { } ice && Near(ice, 21))
            return support ? Slot.ShortSupport : Slot.LongDps;

        return null;
    }

    private static bool Near(double got, double want) => Math.Abs(got - want) <= DurationSlack;

    public static bool? TakesEast(IWorld world, Slot slot)
    {
        if (Bearing[slot].Length < 2 || world.You is null) return null;

        var mine = world.SeatOf(world.You);
        var others = world.Party
            .Where(p => p.ObjectId != world.You.ObjectId && SlotOf(world, p) == slot)
            .Select(world.SeatOf)
            .Where(seat => seat >= 0)
            .ToList();

        if (mine < 0 || others.Count == 0) return null;

        return others.All(other => FruAssignments.TakesEast(mine, other));
    }

    public static Depth DepthAt(Slot slot, int moment, bool ice) =>
        ice && slot == Slot.ShortSupport && moment == 0 ? Depth.Halfway : Depths[slot][moment];

    public static (string Text, string Speech) Words(
        Slot slot, int moment, ArenaSector north, bool? east = null,
        string? mark = null, int? rewind = null, bool ice = false)
    {
        var turn = (int)north;
        var bearings = Bearing[slot];
        if (east is { } side && bearings.Length > 1) bearings = [bearings[side ? 1 : 0]];
        var points = bearings.Select(b => b.PlusEighths(turn)).ToList();
        var depth = DepthAt(slot, moment, ice);

        var shown = string.Join(" or ", points.Select(p => p.Short()));
        var said = string.Join(" or ", points.Select(p => p.Spoken()));

        if (moment == LastMoment) return (Home, Home);

        if (ice && moment == IceDrop) return (IceWords, IceWords);

        if (moment == rewind)
        {
            var (showBack, sayBack) = depth switch
            {
                Depth.Wall => ($"wall at {shown}", $"the wall, {said}"),
                Depth.Halfway => ($"front of Tower {shown}", $"front of {said} tower"),
                Depth.Bait => ($"on {shown} Tower", $"on the {said} tower"),
                _ => (Centre, Centre),
            };

            return ($"Drop rewind, {showBack}", $"Drop rewind, {sayBack}");
        }

        var (show, say) = depth switch
        {
            Depth.Wall when ice => ($"Go wall at {shown}", $"Go wall, {said}"),
            Depth.Wall => ($"Drop at wall at {shown}", $"Drop at wall, {said}"),
            Depth.Halfway => ($"Front of Tower {shown}", $"Front of {said} tower"),
            Depth.Bait => ($"Bait on {shown} Tower", $"Bait the tower, {said}"),
            _ => ($"{Home} by {shown}", $"{Home} by {said}"),
        };

        return mark is null ? (show, say) : ($"{mark}, {show}", $"{mark}, {say}");
    }

    public static bool Speaks(Slot slot, int moment, int? rewind, bool ice = false) =>
        moment == 0 || moment == LastMoment || moment == rewind
        || (ice && moment == IceDrop)
        || DepthAt(slot, moment, ice) != Depth.Middle;

    private static void Say(SequenceRun run, GameEvent on, IWorld world, Slot slot,
                            int moment, ArenaSector north, bool? east,
                            string? mark, int? rewind, bool ice)
    {
        if (!Speaks(slot, moment, rewind, ice)) return;

        var (text, speech) = Words(
            slot, moment, north, east, moment == 0 ? mark : null, rewind, ice);
        run.SetParam(SeatCalls.TextParam, text);
        run.SetParam(SeatCalls.SpeechParam, speech);
        run.Call(relativitySpot, on);
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, UltimateRelativity),
            async (start, run) =>
            {
                var tethers = await run.WaitEvents(3, EventKind.Tether,
                    e => e.Id == HourglassTether);
                if (tethers.Count < 3) return;

                var glasses = tethers
                    .Select(t => FruArena.SectorOf(world, t.Source, FruArena.Middle))
                    .ToList();

                var north = NorthOf(glasses);
                if (!north.IsPoint()) return;

                var speed = await run.FindOrWaitForCast(world, e => e.Id == Speed);
                if (speed is null) return;

                var slot = SlotOf(world);
                if (slot is null) return;

                var east = TakesEast(world, slot.Value);
                var mark = MarkOf(world);
                var rewind = RewindMoment(world);
                var ice = mark == Mark;

                Say(run, speed, world, slot.Value, 0, north, east, mark, rewind, ice);

                var last = double.NegativeInfinity;

                for (var round = 0; round < Rounds; round++)
                {
                    GameEvent? meltdown;
                    do
                    {
                        meltdown = await run.WaitEvent(EventKind.CastStart, SinboundMeltdown);
                        if (meltdown is null) return;
                    }
                    while (meltdown.At - last < ApartSeconds);

                    last = meltdown.At;
                    Say(run, meltdown, world, slot.Value, 1 + round * 2, north, east,
                        mark, rewind, ice);

                    await run.WaitMs(round == Rounds - 1 ? ToFinalMs : ToNextMs);
                    Say(run, meltdown, world, slot.Value, 2 + round * 2, north, east,
                        mark, rewind, ice);
                }
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 4, new FruRelativity(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world), Gaze(world)],
        });
}
