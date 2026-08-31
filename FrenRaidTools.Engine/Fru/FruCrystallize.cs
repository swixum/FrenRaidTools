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

    public const uint WildCharge = 0x9D8C;

    public const uint WaveStep = 0x9D3D;

    public const int LongLights = 2;

    public const int TrafficSets = 3;

    public const int WavesBeforeSpread = 6;

    public const double PairSeconds = 1.0;

    public const string ClawSequenceName = Group + ".claws";

    public const string TrafficSequenceName = Group + ".traffic";

    public const uint SpiritTaker = 0x9D60;

    public const uint Wyrmclaw = 0xCBF;

    public const uint Wyrmfang = 0xCC0;

    public const uint Eruption = 0x099C;

    public const uint BlueIce = 0x099E;

    public const uint Unholy = 0x0996;

    public const uint Water = 0x099D;

    public const double RewindSeconds = 5.0;

    public const double StackFindSeconds = 20.0;

    public const double WaterLead = 4.0;

    public const double UnholyLead = 4.0;

    public const int Claws = 4;

    public const double AeroSeconds = 25;

    public const double TimeoutSeconds = 90;

    public const double QuietSeconds = 70;

    public static readonly CallWindow Window = new();

    public static readonly Callout crystallizePurple = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizePurple",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The two long lights land on one diagonal, either northeast and southwest "
                + "or northwest and southeast, and the pull rolls which.\n"
                + "The northern one is the eruption spot and the southern one takes the "
                + "ice, unholy and water, so this names the pair the plan calls north and "
                + "south intercardinal.",
    };

    public static readonly Callout crystallizeTraffic = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeTraffic",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "Three sets of hourglasses go off in opposite pairs, north and south first "
                + "and then the two diagonals, read off the hourglasses as they cast.",
    };

    public static readonly Callout crystallizeExaline = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeExaline",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The first wave crosses from the east or the west and the second from the "
                + "north or the south, both read off where the Usurper casts them.\n"
                + "The party waits middle north for the first and steps into the second.",
    };

    public static readonly Callout crystallizeSpread = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeSpread",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The Spirit Taker spread is read off the rewind marker: tanks close to the "
                + "wall, healers close to the middle, ranged far by the wall, melee far "
                + "toward the middle.",
    };

    public static readonly Callout crystallizeKnockback = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeKnockback",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "Hallowed Wings knocks the whole party away from the Usurper twice, the "
                + "second one through the stun, and both land on the rewind spot.",
    };

    public static readonly Callout crystallizeRewind = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeRewind",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The two tidal waves come from two sides, and the corner between them is "
                + "where the rewind goes.",
    };

    public static readonly Callout crystallizeWaterStack = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeWaterStack",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The water lands on the south intercardinal group, so the three southern "
                + "blues and the aero on that side share it.\n"
                + "That aero then knocks the three blues across to the northern purple "
                + "light and stays south for the late head.",
    };

    public static readonly Callout crystallizeUnholyStack = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeUnholyStack",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The unholy stack is taken at the northern purple light by all four blues "
                + "plus the ice head on that same side.\n"
                + "The other ice head stays wide and the two aeros stay south.",
    };

    public static readonly Callout crystallizeClaw = new()
    {
        Description = "Crystallize Time",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "crystallizeClaw",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The four claws are two pairs, aero and ice, and each pair takes one head "
                + "each.\n"
                + "Which head is yours is your place in the group's claw order.",
    };

    public static (string Text, string Speech) ClawWords(bool aero, bool west)
    {
        if (aero)
            return west
                ? ("Southwest, intercept head", "Southwest, intercept the head")
                : ("Southeast, intercept head", "Southeast, intercept the head");

        return west
            ? ("West, intercept head", "West, intercept the head")
            : ("East, intercept head", "East, intercept the head");
    }

    public static (string Text, string Speech) PopWords(bool west) =>
        west
            ? ("Pop the head at 4", "Pop the head at four")
            : ("Pop the head at 3", "Pop the head at three");

    public static GameEvent? MyClaw(IWorld world) =>
        world.You is null
            ? null
            : world.ActiveStatuses().FirstOrDefault(
                s => s.Id == Wyrmclaw && s.Target is not null
                     && s.Target.ObjectId == world.You.ObjectId);

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

    public static (string Text, string Speech) PurpleWords(ArenaSector north)
    {
        var south = north.Opposite();
        return ($"Purple {north.Spoken()} and {south.Spoken()}",
                $"Purple lights {north.Spoken()} and {south.Spoken()}");
    }

    public static ArenaSector CleanseAt(uint debuff) => debuff switch
    {
        Eruption => ArenaSector.West,
        BlueIce => ArenaSector.Southwest,
        Unholy => ArenaSector.East,
        Water => ArenaSector.Southeast,
        _ => ArenaSector.Unknown,
    };

    public static (string Text, string Speech)? BlueWords(uint debuff, ArenaSector north)
    {
        var at = CleanseAt(debuff);
        if (!at.IsPoint()) return null;

        var spot = debuff == Eruption ? north : north.Opposite();

        return ($"{spot.Name()}, cleanse {at.Short()}",
                $"{spot.Name()}, then cleanse at {at.SpokenMark()}");
    }

    public static ArenaSector AeroSpot(bool west) =>
        west ? ArenaSector.Southwest : ArenaSector.Southeast;

    public static bool IceJoinsStack(bool west, ArenaSector north) =>
        west == (north == ArenaSector.Northwest);

    public static (bool Aero, bool West)? MySide(IWorld world)
    {
        var mine = MyClaw(world);
        if (mine is null) return null;

        var claws = world.ActiveStatuses().Where(s => s.Id == Wyrmclaw).ToList();
        if (claws.Count < Claws) return null;

        var seat = SeatCalls.MySeat(world);
        var west = FruAssignments.ClawWest(seat, Partner(world, claws, seat));

        return west is null ? null : (IsAero(mine), west.Value);
    }

    public static (string Text, string Speech)? WaterWords(
        uint blue, (bool Aero, bool West)? claw, ArenaSector north)
    {
        if (blue is BlueIce or Unholy or Water)
            return ($"Water stack, knocked {north.Spoken()}",
                    $"Water stack, then knocked {north.Spoken()}");

        if (claw is { Aero: true } side && AeroSpot(side.West) == north.Opposite())
            return ("Water stack, stay south", "Water stack, then stay south");

        return null;
    }

    public static (string Text, string Speech)? UnholyWords(
        uint blue, (bool Aero, bool West)? claw, ArenaSector north)
    {
        if (blue == Unholy)
            return ("Unholy stack, off the crystal", "Unholy stack, keep off the crystal");

        if (blue is Eruption or BlueIce or Water)
            return ("Unholy stack, then north", "Unholy stack, then move north");

        if (claw is { Aero: false } side)
        {
            if (IceJoinsStack(side.West, north))
                return ($"{north.Name()}, unholy stack",
                        $"{north.Name()}, join the unholy stack");

            var wide = side.West ? "west" : "east";
            return ($"Stay wide {wide}", $"Stay wide {wide}, out of the stack");
        }

        return null;
    }

    public static uint MyBlue(IWorld world)
    {
        if (world.You is null) return 0;

        var mine = world.ActiveStatuses()
            .Where(s => s.Target is not null && s.Target.ObjectId == world.You.ObjectId)
            .Select(s => s.Id)
            .ToHashSet();

        if (!mine.Contains(Wyrmfang)) return 0;

        foreach (var debuff in new[] { Eruption, BlueIce, Unholy, Water })
            if (mine.Contains(debuff)) return debuff;

        return 0;
    }

    public static (string Text, string Speech) TrafficWords(IReadOnlyList<ArenaSector> pair)
    {
        var order = pair
            .OrderByDescending(s => s is ArenaSector.North or ArenaSector.Northeast
                                         or ArenaSector.Northwest)
            .ThenBy(s => (int)s)
            .ToList();

        var said = string.Join(" and ", order.Select(s => s.Spoken()));
        return ($"Dodge {said}", $"Dodge {said}");
    }

    public static (string Text, string Speech) ExalineWords(ArenaSector from)
    {
        var side = from.Spoken();
        return from is ArenaSector.East or ArenaSector.West
            ? ($"Middle north, wave from {side}", $"Middle north, wave from the {side}")
            : ($"Step in, wave from {side}", $"Step in toward the wave from the {side}");
    }

    private static bool OnTheLeft(int seat) => seat % 2 == 0;

    public static (string Text, string Speech) RewindWords(int seat, ArenaSector corner)
    {
        if (seat < 0 || seat >= Slots.Count)
            return ($"Drop rewind {corner.Short()}", $"Drop rewind {corner.Spoken()}");

        var side = OnTheLeft(seat) ? "left" : "right";
        var mark = corner.Short();
        var spoken = corner.SpokenMark();

        return Slots.RoleOf(seat) == SlotRole.Tank
            ? ($"Drop rewind {mark}, {side} edge",
               $"Drop rewind {spoken}, {side} edge by the wall")
            : ($"Drop rewind {mark}, {side} corner",
               $"Drop rewind {spoken}, {side} corner facing the wall");
    }

    public static (string Text, string Speech) SpreadWords(int seat, ArenaSector corner)
    {
        if (seat < 0 || seat >= Slots.Count || !corner.IsPoint())
            return ("Spread off the rewind", "Spread off the rewind corner");

        var left = OnTheLeft(seat);
        var side = left ? "left" : "right";
        var role = Slots.RoleOf(seat);

        if (role == SlotRole.Tank)
            return ($"Spread past the rewind, {side}",
                    $"Spread out past the rewind, {side}");

        var steps = role switch
        {
            SlotRole.Healer => 1,
            SlotRole.Ranged => 2,
            _ => 3,
        };

        var spot = corner.PlusEighths(left ? -steps : steps).Spoken();

        return role == SlotRole.Ranged
            ? ($"Spread {spot}, near the wall", $"Spread {spot}, near the wall")
            : ($"Spread {spot}, near the middle", $"Spread {spot}, near the middle");
    }

    public static (string Text, string Speech) KnockbackWords(bool second) =>
        second
            ? ("Knockback again, same spot", "Knockback again, the same spot")
            : ("Knockback to the rewind", "Knockback into the rewind spot");

    private static bool IsLight(Actor? actor) =>
        actor is not null && actor.BaseId == SorrowsHourglass;

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, CrystallizeTime),
            async (start, run) =>
            {
                Window.Open(start.At, QuietSeconds);

                var tethers = await run.WaitEvents(LongLights, EventKind.Tether,
                    e => e.Id == LightTether && (IsLight(e.Source) || IsLight(e.Target)));
                if (tethers.Count < LongLights) return;

                var lights = tethers
                    .Select(t => FruArena.SectorOf(
                        world, IsLight(t.Source) ? t.Source : t.Target, FruArena.Middle))
                    .ToList();

                var north = Safe(lights);
                if (!north.IsPoint()) return;

                var blue = MyBlue(world);
                var spot = blue == 0 ? null : BlueWords(blue, north);
                if (spot is not null || MyClaw(world) is null)
                    Say(run, crystallizePurple, start, spot ?? PurpleWords(north));

                var side = MySide(world);
                await Stack(world, run, Water, WaterLead, crystallizeWaterStack,
                            WaterWords(blue, side, north));
                await Stack(world, run, Unholy, UnholyLead, crystallizeUnholyStack,
                            UnholyWords(blue, side, north));

                var first = await run.FindOrWaitForCast(world, e => e.Id == TidalLight);
                if (first is null) return;
                var one = FruArena.SectorOf(world, first.Source);
                var claw = MyClaw(world);
                if (claw is null || !IsAero(claw))
                    Say(run, crystallizeExaline, first, ExalineWords(one));

                var second = await run.WaitEvent(EventKind.CastStart, TidalLight);
                if (second is null) return;
                var two = FruArena.SectorOf(world, second.Source);
                Say(run, crystallizeExaline, second, ExalineWords(two));

                var corner = ArenaSectors.Between(one, two);
                if (!corner.IsPoint()) return;

                var seat = SeatCalls.MySeat(world);

                await run.WaitSeconds(RewindSeconds);
                Say(run, crystallizeRewind, second, RewindWords(seat, corner));

                var taker = await run.WaitEvent(EventKind.CastStart, SpiritTaker);
                if (taker is null) return;
                Say(run, crystallizeSpread, taker, SpreadWords(seat, corner));

                var wings = await run.WaitEvent(EventKind.CastStart, WildCharge);
                if (wings is null) return;
                Say(run, crystallizeKnockback, wings, KnockbackWords(second: false));

                var again = await run.WaitEvent(EventKind.CastStart, WildCharge);
                if (again is null) return;
                Say(run, crystallizeKnockback, again, KnockbackWords(second: true));
            });

    public static Sequence Traffic(IWorld world)
    {
        var gate = new CallCooldown(PairSeconds);

        return Sequence.Repeat(TrafficSequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, FirstLights),
            async (start, run) =>
            {
                if (!gate.Ready(crystallizeTraffic, start.At)) return;

                var rest = await run.WaitEventsWithin(
                    1, e => e.Is(EventKind.CastStart, FirstLights), PairSeconds);

                var pair = new List<GameEvent> { start };
                pair.AddRange(rest);

                var sectors = pair
                    .Select(c => FruArena.SectorOf(world, c.Source, FruArena.Middle))
                    .Where(s => s.IsPoint())
                    .Distinct()
                    .OrderBy(s => (int)s)
                    .ToList();

                if (sectors.Count != 2) return;
                Say(run, crystallizeTraffic, start, TrafficWords(sectors));
            });
    }

    private static bool Blue(IWorld world, Actor? target) =>
        target is not null && world.ActiveStatuses().Any(
            s => s.Id == Wyrmfang && s.Target is not null
                 && s.Target.ObjectId == target.ObjectId);

    private static async Task Stack(IWorld world, SequenceRun run, uint debuff, double lead,
                                    Callout call, (string Text, string Speech)? words)
    {
        var held = await run.FindOrWaitForStatusWithin(
            world, e => e.Id == debuff && Blue(world, e.Target), StackFindSeconds);
        if (held is null) return;

        await run.WaitSeconds(run.Remaining(held) - lead);
        if (words is { } said) Say(run, call, held, said);
    }

    private static void Say(SequenceRun run, Callout call, GameEvent on,
                            (string Text, string Speech) words)
    {
        run.SetParam(SeatCalls.TextParam, words.Text);
        run.SetParam(SeatCalls.SpeechParam, words.Speech);
        run.Call(call, on);
    }

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

                var mine = claws.FirstOrDefault(
                    c => c.Target is not null && world.SeatOf(c.Target) == seat);
                if (mine is null) return;
                var aero = IsAero(mine);

                Say(run, crystallizeClaw, claws[^1], ClawWords(aero, west.Value));
                if (!aero) return;

                var wave = await run.WaitEvent(EventKind.CastStart, TidalLight);
                if (wave is null) return;
                Say(run, crystallizeClaw, wave, PopWords(west.Value));
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 5, new FruCrystallize(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world), Claw(world), Traffic(world)],
        });
}
