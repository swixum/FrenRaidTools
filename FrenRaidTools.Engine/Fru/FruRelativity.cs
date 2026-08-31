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

    public const double TimeoutSeconds = 90;

    public const double DurationSlack = 2.0;

    public const int Rounds = 3;

    public const double ApartSeconds = 5.0;

    public const int ToNextMs = 6000;

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

    public static readonly IReadOnlyList<string> Moments =
        ["1st fire", "1st bait", "2nd fire", "2nd bait", "3rd fire", "3rd bait", "rewind"];

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

    public static Slot? SlotOf(IWorld world) =>
        world.You is null ? null : SlotOf(world, world.You);

    public static Slot? SlotOf(IWorld world, Actor you)
    {
        var mine = world.ActiveStatuses()
            .Where(s => s.Target is not null && s.Target.ObjectId == you.ObjectId)
            .ToList();

        double? Held(uint status) =>
            mine.Where(s => s.Id == status).Select(s => (double?)s.Duration).FirstOrDefault();

        var seat = world.SeatOf(you);
        var support = seat >= 0 ? Slots.IsSupport(seat) : you.Support;
        var fire = Held(DarkFire);
        if (fire is { } seconds)
        {
            if (Near(seconds, 11)) return support ? Slot.ShortSupport : Slot.ShortDps;
            if (Near(seconds, 21)) return support ? Slot.MediumSupport : Slot.MediumDps;
            if (Near(seconds, 31)) return support ? Slot.LongSupport : Slot.LongDps;
        }

        if (Held(DarkBlizzard) is { } ice && Near(ice, 21))
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

    public static (string Text, string Speech) Words(
        Slot slot, int moment, ArenaSector north, bool? east = null)
    {
        var turn = (int)north;
        var bearings = Bearing[slot];
        if (east is { } side && bearings.Length > 1) bearings = [bearings[side ? 1 : 0]];
        var points = bearings.Select(b => b.PlusEighths(turn)).ToList();
        var depth = Depths[slot][moment];

        var shown = string.Join(" or ", points.Select(p => p.Short()));
        var said = string.Join(" or ", points.Select(p => p.Spoken()));
        said = said[..1].ToUpperInvariant() + said[1..];

        return depth switch
        {
            Depth.Wall => ($"{shown}, out to the wall", $"{said}, out to the wall"),
            Depth.Halfway => ($"{shown}, halfway out", $"{said}, halfway out"),
            Depth.Bait => ($"{shown}, bait at marker", $"{said}, bait on marker"),
            _ => ($"{shown}, middle", $"{said}, to the middle"),
        };
    }

    private static void Say(SequenceRun run, GameEvent on, IWorld world, Slot slot,
                            int moment, ArenaSector north, bool? east)
    {
        var (text, speech) = Words(slot, moment, north, east);
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

                Say(run, speed, world, slot.Value, 0, north, east);

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
                    Say(run, meltdown, world, slot.Value, 1 + round * 2, north, east);

                    await run.WaitMs(ToNextMs);
                    Say(run, meltdown, world, slot.Value, 2 + round * 2, north, east);
                }
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 4, new FruRelativity(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
