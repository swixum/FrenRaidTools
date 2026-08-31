using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruFallOfFaith
{
    public const string Group = "fru.fallOfFaith";

    public const string MechanicName = "Fall of Faith";

    public const string SequenceName = Group + ".tethers";

    public const uint Fire = 0x9CC9;

    public const uint Lightning = 0x9CCC;

    public const uint FireTether = 0x00F9;

    public const uint LightningTether = 0x011F;

    public const int Count = 4;

    public const double TimeoutSeconds = 30;

    public const double ConfirmSeconds = 4;

    public static readonly Callout faithTether = new()
    {
        Description = "Fall of Faith",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "faithTether",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "Your place in the order and which element you drew, both read off the "
                + "tethers as they land.\n"
                + "Lightning is three cones, fire is one stack cone.",
    };

    public static readonly Callout faithBait = new()
    {
        Description = "Fall of Faith",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "faithBait",
        FromPlan = true,
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "Untethered players bait, so this says which you are.",
    };

    public static ArenaSector Side(int place) =>
        place % 2 == 0 ? ArenaSector.West : ArenaSector.East;

    public readonly record struct Bait(string Spot, string Text, string Speech);

    public const string Middle = "middle";

    public static Bait? Spot(int bait, bool lightning, int place, int rank)
    {
        if (bait < 0 || bait >= Count) return null;
        var side = Side(bait).Name().ToLowerInvariant();

        if (place >= 0)
        {
            if (place % 2 != bait % 2) return null;
            return place == bait
                ? Line(Middle, lightning ? $"{side}, middle" : $"{side}, fire, middle")
                : Line("behind", $"{side}, behind");
        }

        if (rank < 0 || rank >= Count) return null;
        if (rank <= 1 != (bait % 2 == 0)) return null;
        if (!lightning) return Line("behind", $"{side}, behind");

        var shoulder = rank is 0 or 3 ? "top" : "bottom";
        return Line(shoulder, $"{side}, {shoulder}");
    }

    private static Bait Line(string spot, string rest)
    {
        var line = char.ToUpperInvariant(rest[0]) + rest[1..];
        return new(spot, line, line);
    }

    public static Bait Again(Bait now, string before, bool lightning)
    {
        if (now.Spot == before) return now with { Text = "Stay", Speech = "Stay" };

        var where = !lightning && now.Spot == Middle ? "fire middle" : now.Spot;
        var line = $"Swap to {where}";
        return now with { Text = line, Speech = line };
    }

    public static (string Text, string Speech) BaitWords(int rank) => rank switch
    {
        0 => ("Bait west, north", "Bait west, north"),
        1 => ("Bait west, south", "Bait west, south"),
        2 => ("Bait east, south", "Bait east, south"),
        3 => ("Bait east, north", "Bait east, north"),
        _ => ("No tether, bait", "No tether, bait"),
    };

    public static List<int> TetheredSeats(IWorld world, IEnumerable<GameEvent> tethers)
    {
        var seats = new List<int>();
        foreach (var tether in tethers)
        {
            var seat = tether.Target is null ? -1 : world.SeatOf(tether.Target);
            if (seat < 0 && tether.Source is not null) seat = world.SeatOf(tether.Source);
            if (seat >= 0 && !seats.Contains(seat)) seats.Add(seat);
        }

        return seats;
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.Tether, FireTether, LightningTether),
            async (first, run) =>
            {
                var confirm = await run.WaitEventsWithin(
                    1, e => e.Is(EventKind.CastStart, Fire, Lightning), ConfirmSeconds);
                if (confirm.Count == 0) return;

                var rest = await run.WaitEvents(Count - 1, EventKind.Tether,
                    e => e.Is(EventKind.Tether, FireTether, LightningTether));

                var tethers = new List<GameEvent> { first };
                tethers.AddRange(rest);

                var seat = SeatCalls.MySeat(world);
                var held = TetheredSeats(world, tethers);
                var place = tethers.FindIndex(t => FruArena.Mine(t, world));
                var rank = held.Count == Count
                    ? FruAssignments.BaitRank(seat, held)
                    : -1;

                var on = confirm[0];
                string? standing = null;
                for (var bait = 0; bait < Count; bait++)
                {
                    if (bait > 0)
                        on = await run.WaitEvent(
                            EventKind.AbilityHit, e => e.Id is Fire or Lightning);

                    var lightning = tethers[bait].Id == LightningTether;
                    var line = Spot(bait, lightning, place, rank);
                    if (line is null) continue;

                    var say = standing is null
                        ? line.Value
                        : Again(line.Value, standing, lightning);
                    standing = line.Value.Spot;

                    run.SetParam(SeatCalls.TextParam, say.Text);
                    run.SetParam(SeatCalls.SpeechParam, say.Speech);
                    run.Call(place == bait ? faithTether : faithBait, on);
                }
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 1, new FruFallOfFaith(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
