using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruLightRampant
{
    public const string Group = "fru.lightRampant";

    public const string MechanicName = "Light Rampant";

    public const uint LightRampant = 0x9D14;

    public const uint PuddleMarker = 0x177;

    public const double TimeoutSeconds = 45;

    public const double SoakSeconds = 4.0;

    public static readonly Callout lightRampantLineUp = new()
    {
        Description = "Light Rampant",
        Mechanic = MechanicName,
        Phase = 2,
        Key = "lightRampantLineUp",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "Answered from who takes the two puddles.\n"
                + "Puddle: your side, west or east\n"
                + "Chained: your place in the lineup, read off the sim's own order",
    };

    public static readonly Callout lightRampantTower = new()
    {
        Description = "Light Rampant",
        Mechanic = MechanicName,
        Phase = 2,
        Key = "lightRampantTower",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text,
        Notes = "Answered from who takes the two puddles.\n"
                + "The six chained take a tower each; Burst tells the two on puddles "
                + "where to drop.",
    };

    public static readonly IReadOnlyList<int> NorthFirst = [1, 0, 3, 2];

    public static readonly IReadOnlyList<int> SouthFirst = [6, 7, 4, 5];

    public static readonly IReadOnlyList<int> WestToEast = [6, 2, 7, 3, 4, 0, 5, 1];

    public static readonly IReadOnlyList<string> Places =
        ["n0", "n1", "n2", "s0", "s1", "s2", "nPuddle", "sPuddle"];

    public static readonly Dictionary<string, (string Text, string Speech)> LineUpAt = new()
    {
        ["n0"] = ("Line up 2", "Line up northeast at two"),
        ["n1"] = ("Line up A", "Line up north at A"),
        ["n2"] = ("Line up 1", "Line up northwest at one"),
        ["s0"] = ("Line up 4", "Line up southwest at four"),
        ["s1"] = ("Line up C", "Line up south at C"),
        ["s2"] = ("Line up 3", "Line up southeast at three"),
        ["nPuddle"] = ("Puddle D", "Puddle, west at D"),
        ["sPuddle"] = ("Puddle B", "Puddle, east at B"),
    };

    public static readonly Dictionary<string, (string Text, string Speech)> TowerAt = new()
    {
        ["n0"] = ("Tower 1", "Tower northwest at one"),
        ["n1"] = ("Tower C", "Tower south at C"),
        ["n2"] = ("Tower 2", "Tower northeast at two"),
        ["s0"] = ("Tower 4", "Tower southwest at four"),
        ["s1"] = ("Tower A", "Tower north at A"),
        ["s2"] = ("Tower 3", "Tower southeast at three"),
    };

    public static Dictionary<int, string> Places4And4(IReadOnlyList<int> puddles)
    {
        if (puddles.Count != 2) return [];

        var order = new List<int>(puddles);
        if (WestOf(order[0]) > WestOf(order[1]))
        {
            var first = order[0];
            order.RemoveAt(0);
            order.Add(first);
        }

        var north = new List<int>(NorthFirst);
        var south = new List<int>(SouthFirst);
        var spread = new List<int>();

        foreach (var seat in order)
        {
            var at = WhereInOrder(seat);
            if (at < 0) return [];

            var line = at < 4 ? north : south;
            if (!line.Remove(seat)) return [];
            spread.Add(seat);
        }

        if (north.Count > 3)
        {
            south.Add(north[0]);
            north.RemoveAt(0);
        }
        else if (south.Count > 3)
        {
            north.Add(south[0]);
            south.RemoveAt(0);
        }

        if (north.Count < 3 || south.Count < 3 || spread.Count != 2) return [];

        return new Dictionary<int, string>
        {
            [north[0]] = "n0", [north[1]] = "n1", [north[2]] = "n2",
            [south[0]] = "s0", [south[1]] = "s1", [south[2]] = "s2",
            [spread[0]] = "nPuddle", [spread[1]] = "sPuddle",
        };
    }

    private static int WestOf(int seat)
    {
        for (var i = 0; i < WestToEast.Count; i++)
            if (WestToEast[i] == seat) return i;
        return WestToEast.Count;
    }

    private static int WhereInOrder(int seat)
    {
        for (var i = 0; i < NorthFirst.Count; i++)
            if (NorthFirst[i] == seat) return i;
        for (var i = 0; i < SouthFirst.Count; i++)
            if (SouthFirst[i] == seat) return i + 4;
        return -1;
    }

    private static List<int> PuddleSeats(IWorld world, IReadOnlyList<GameEvent> markers)
    {
        var seats = new List<int>();
        foreach (var marker in markers)
        {
            if (marker.Target is null) continue;
            var seat = world.SeatOf(marker.Target);
            if (seat >= 0 && !seats.Contains(seat)) seats.Add(seat);
        }

        return seats;
    }

    private static bool Say(SequenceRun run, Callout call, GameEvent on, IWorld world,
                            Dictionary<int, string> places,
                            Dictionary<string, (string Text, string Speech)> words)
    {
        var seat = SeatCalls.MySeat(world);
        if (seat < 0 || !places.TryGetValue(seat, out var place)) return false;
        if (!words.TryGetValue(place, out var line)) return false;

        run.SetParam(SeatCalls.TextParam, line.Text);
        run.SetParam(SeatCalls.SpeechParam, line.Speech);
        run.Call(call, on);
        return true;
    }

    public const string SequenceName = Group + ".spots";

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.CastStart, LightRampant),
            async (start, run) =>
            {
                var markers = await run.WaitEvents(2, EventKind.HeadMarker,
                    e => e.Id == PuddleMarker);
                if (markers.Count < 2) return;

                var places = Places4And4(PuddleSeats(world, markers));
                if (places.Count != 8) return;

                Say(run, lightRampantLineUp, markers[^1], world, places, LineUpAt);
                await run.WaitMs((int)(SoakSeconds * 1000));
                Say(run, lightRampantTower, markers[^1], world, places, TowerAt);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 2, new FruLightRampant(), null)
        {
            PhaseNames = ["P1 Fatebreaker", "P2 Usurper of Frost", "Intermission Crystals",
                          "P3 Oracle of Darkness", "P4 Usurper and Oracle", "P5 Pandora"],
            Extra = world => [Build(world)],
        });
}
