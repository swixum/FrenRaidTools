using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruDarklit
{
    public const string Group = "fru.darklit";

    public const string MechanicName = "Darklit Dragonsong";

    public const string SequenceName = Group + ".roles";

    public const uint DarklitDragonsong = 0x9D2F;

    public const uint PathOfLight = 0x9CFB;

    public const uint SpiritTaker = 0x9CFE;

    public const uint DarkWater = 0x99D;

    public const uint Chain = 0x6E;

    public const int Stacks = 2;

    public const int Chains = 4;

    public const double TimeoutSeconds = 50;

    public static readonly Callout darklitHolding = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "darklitHolding",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "The tower or the bait, read off the chain and the water as they arrive.\n"
                + "Four players take a chain and two take the water, and the pull rolls "
                + "which, so there is no seat answer to be had.",
    };

    public static readonly Callout darklitTower = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = MechanicName,
        Phase = 5,
        Key = "darklitTower",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Chained players soak, the rest bait. The sim puts the four towers either "
                + "side of the north and south line and the four baits either side of east "
                + "and west. The bowtie decides which of the two is yours.",
    };

    public static (string Text, string Speech) Holding(bool chained, bool water) =>
        (chained, water) switch
        {
            (true, true) => ("Tower, water stack", "Tower, water stack"),
            (true, false) => ("Tower only", "Tower only"),
            (false, true) => ("Bait, water stack", "Bait, water stack"),
            _ => ("Bait only", "Bait only"),
        };

    public static (string Text, string Speech) TowerOrBait(bool chained) =>
        chained
            ? ("Soak, north or south", "Soak tower, north or south")
            : ("Bait, east or west", "Bait cleave, east or west");

    public readonly record struct Spot(bool North, bool East, bool Soaks);

    public static (string Text, string Speech) SpotWords(Spot spot)
    {
        var side = spot.North ? "north" : "south";
        var half = spot.East ? "east" : "west";
        var line = spot.Soaks
            ? $"{Capital(side)} tower, {half}"
            : $"Bait {half}, {side}";
        return (line, line);
    }

    private static string Capital(string word) => char.ToUpperInvariant(word[0]) + word[1..];

    private static List<int> Seats(IWorld world, IEnumerable<Actor?> actors)
    {
        var seats = new List<int>();
        foreach (var actor in actors)
        {
            if (actor is null) continue;
            var seat = world.SeatOf(actor);
            if (seat >= 0 && !seats.Contains(seat)) seats.Add(seat);
        }

        return seats;
    }

    public static Dictionary<int, Spot> Solve(
        IWorld world, IReadOnlyList<GameEvent> chains, IReadOnlyList<GameEvent> waters)
    {
        var spots = new Dictionary<int, Spot>();
        var links = new List<(int From, int To)>();
        foreach (var chain in chains)
        {
            var from = chain.Source is null ? -1 : world.SeatOf(chain.Source);
            var to = chain.Target is null ? -1 : world.SeatOf(chain.Target);
            if (from < 0 || to < 0) return spots;
            links.Add((from, to));
        }

        var soakers = Seats(world, chains.Select(c => c.Source));
        if (soakers.Count != Chains || links.Count != Chains) return spots;

        var anchor = soakers.MinBy(seat => FruAssignments.DarklitOrder[seat]);
        var south = new HashSet<int>();
        foreach (var (from, to) in links)
        {
            if (from == anchor) south.Add(to);
            else if (to == anchor) south.Add(from);
        }

        south.Remove(anchor);
        if (south.Count != 2 || south.Any(seat => !soakers.Contains(seat))) return spots;

        var north = soakers.Where(seat => !south.Contains(seat)).ToList();
        if (north.Count != 2) return spots;

        foreach (var pair in new[] { south.ToList(), north })
        {
            var east = EastOf(pair[0], pair[1]);
            foreach (var seat in pair)
                spots[seat] = new Spot(!south.Contains(seat), seat == east, Soaks: true);
        }

        var baiters = Enumerable.Range(0, Slots.Count)
            .Where(seat => !soakers.Contains(seat))
            .OrderBy(seat => FruAssignments.DarklitOrder[seat])
            .ToList();
        if (baiters.Count != Chains) return spots;

        for (var place = 0; place < baiters.Count; place++)
            spots[baiters[place]] =
                new Spot(place is 0 or 3, place >= 2, Soaks: false);

        Flex(world, waters, soakers, baiters, spots);
        return spots;
    }

    private static int EastOf(int one, int other)
    {
        var first = FruAssignments.DarklitOrder[one];
        var second = FruAssignments.DarklitOrder[other];
        if (first < 4 && second < 4) return first < second ? one : other;
        if (first >= 4 && second >= 4) return first > second ? one : other;
        return first < 4 ? other : one;
    }

    private static void Flex(IWorld world, IReadOnlyList<GameEvent> waters,
                             IReadOnlyList<int> soakers, IReadOnlyList<int> baiters,
                             Dictionary<int, Spot> spots)
    {
        var holders = Seats(world, waters.Select(w => w.Target));
        if (holders.Count != Stacks) return;

        var southHolders = holders.Count(seat => spots.TryGetValue(seat, out var at) && !at.North);
        if (southHolders == 1) return;

        var free = holders.FirstOrDefault(seat => !soakers.Contains(seat), -1);
        if (free < 0 || !spots.TryGetValue(free, out var mine)) return;

        foreach (var seat in baiters)
            if (spots.TryGetValue(seat, out var at) && at.East == mine.East)
                spots[seat] = at with { North = !at.North };
    }

    private static bool OnMe(GameEvent got, IWorld world) => FruArena.Mine(got, world);

    private static void Say(SequenceRun run, Callout call, GameEvent on,
                            (string Text, string Speech) words)
    {
        run.SetParam(SeatCalls.TextParam, words.Text);
        run.SetParam(SeatCalls.SpeechParam, words.Speech);
        run.Call(call, on);
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, DarklitDragonsong),
            async (start, run) =>
            {
                var waters = await run.WaitEvents(Stacks, EventKind.StatusGain,
                    e => e.Id == DarkWater);
                var chains = await run.WaitEvents(Chains, EventKind.Tether,
                    e => e.Id == Chain);
                if (chains.Count < Chains) return;

                var chained = chains.Any(t => OnMe(t, world));
                var water = waters.Any(s => OnMe(s, world));

                Say(run, darklitHolding, start, Holding(chained, water));

                var tower = await run.FindOrWaitForCast(world, e => e.Id == PathOfLight);
                if (tower is null) return;

                await run.WaitMs(500);

                var seat = SeatCalls.MySeat(world);
                var spots = Solve(world, chains, waters);
                Say(run, darklitTower, tower,
                    spots.TryGetValue(seat, out var mine)
                        ? SpotWords(mine)
                        : TowerOrBait(chained));
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 5, new FruDarklit(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
