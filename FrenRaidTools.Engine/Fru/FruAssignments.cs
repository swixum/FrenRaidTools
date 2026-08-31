namespace FrenRaidTools.Engine.Fru;

public static class FruAssignments
{
    public static readonly ArenaSector[] DiamondDustCardinals =
    [
        ArenaSector.North, ArenaSector.South, ArenaSector.West, ArenaSector.East,
        ArenaSector.West, ArenaSector.South, ArenaSector.North, ArenaSector.East,
    ];

    public static readonly int[] BoundOfFaithOrder = [0, 1, 2, 3, 0, 1, 2, 3];

    public static bool BoundBaseNorth(int seat) => Slots.IsSupport(seat);

    public static bool? BoundNorth(int seat, IReadOnlyList<int> tethered)
    {
        if (seat < 0 || seat >= Slots.Count || tethered.Count != 2) return null;
        if (tethered.Any(t => t < 0 || t >= Slots.Count)) return null;

        var north = new bool[Slots.Count];
        for (var one = 0; one < Slots.Count; one++) north[one] = BoundBaseNorth(one);

        if (north[tethered[0]] == north[tethered[1]])
        {
            var flexed = BoundOfFaithOrder[tethered[0]] < BoundOfFaithOrder[tethered[1]]
                ? tethered[0]
                : tethered[1];
            north[flexed] = !north[flexed];

            for (var other = 0; other < Slots.Count; other++)
                if (other != flexed && !tethered.Contains(other)
                    && BoundOfFaithOrder[other] == 0 && north[other] == north[flexed])
                {
                    north[other] = !north[other];
                    break;
                }
        }

        return north[seat];
    }

    public static readonly int[] FallOfFaithBaitOrder = [4, 5, 2, 7, 3, 6, 1, 8];

    public static readonly string[] ExplosionFixed = ["H1", "R2", "H2"];

    public static readonly string[] ExplosionFlex = ["M1", "M2", "R1"];

    public static readonly int[] DarklitOrder = [2, 3, 0, 1, 4, 5, 6, 7];

    public static readonly int[] CrystallizeOrder = [4, 3, 2, 1, 5, 6, 7, 8];

    public static bool? ClawWest(int seat, int other)
    {
        if (seat < 0 || seat >= Slots.Count || other < 0 || other >= Slots.Count) return null;
        return seat != other ? CrystallizeOrder[seat] < CrystallizeOrder[other] : null;
    }

    public static readonly int[] RelativityOrder = [3, 4, 2, 1, 3, 4, 2, 1];

    public static bool TakesEast(int seat, int other) =>
        seat >= 0 && seat < Slots.Count && other >= 0 && other < Slots.Count
        && RelativityOrder[seat] > RelativityOrder[other];

    public static ArenaSector DiamondDustSpot(int seat, bool supportsOnCardinals)
    {
        if (seat < 0 || seat >= Slots.Count) return ArenaSector.Unknown;
        var cardinal = DiamondDustCardinals[seat];
        var onCardinals = Slots.IsSupport(seat) == supportsOnCardinals;
        return onCardinals ? cardinal : cardinal.PlusEighths(-1);
    }

    public static int BaitRank(int seat, IReadOnlyCollection<int> tethered)
    {
        if (seat < 0 || seat >= Slots.Count || tethered.Contains(seat)) return -1;
        var lower = 0;
        for (var other = 0; other < Slots.Count; other++)
            if (other != seat && !tethered.Contains(other)
                && FallOfFaithBaitOrder[other] < FallOfFaithBaitOrder[seat])
                lower++;
        return lower;
    }
}
