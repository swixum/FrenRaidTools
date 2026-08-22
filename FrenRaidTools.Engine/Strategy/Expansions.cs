namespace FrenRaidTools.Engine;

public static class Expansions
{
    public static readonly IReadOnlyList<string> Newest =
    [
        "Dawntrail",
        "Endwalker",
        "Shadowbringers",
        "Stormblood",
        "Heavensward",
        "A Realm Reborn",
    ];

    public static string Current => Newest[0];

    public static int Rank(string expansion)
    {
        for (var i = 0; i < Newest.Count; i++)
            if (string.Equals(Newest[i], expansion, StringComparison.OrdinalIgnoreCase)) return i;
        return Newest.Count;
    }

    public static bool Known(string expansion) => Rank(expansion) < Newest.Count;

    public static IReadOnlyList<string> Order(IEnumerable<string> names)
    {
        var seen = new List<string>();
        foreach (var name in names)
            if (!seen.Contains(name, StringComparer.Ordinal))
                seen.Add(name);

        seen.Sort((a, b) =>
        {
            var gap = Rank(a).CompareTo(Rank(b));
            return gap != 0 ? gap : string.CompareOrdinal(a, b);
        });

        return seen;
    }

    public static IReadOnlyList<PlannedFight> Order(IEnumerable<PlannedFight> fights)
    {
        var list = new List<PlannedFight>(fights);

        list.Sort((a, b) =>
        {
            var gap = Rank(a.Expansion).CompareTo(Rank(b.Expansion));
            if (gap != 0) return gap;
            gap = string.CompareOrdinal(a.Category, b.Category);
            return gap != 0 ? gap : string.CompareOrdinal(a.Name, b.Name);
        });

        return list;
    }
}
