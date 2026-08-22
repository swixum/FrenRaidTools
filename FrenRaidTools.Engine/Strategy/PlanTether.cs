using System.Text.RegularExpressions;

namespace FrenRaidTools.Engine;

public sealed record TetherDuty(int Set, int Hit, bool Both, int? Tether);

public static partial class PlanTether
{
    public const string AccretionJoin = "in Line, Accretion";
    public const string AccretionName = "in Line Accretion";
    public const string BothMark = "BOTH TETHERS";
    public const string Accretion = "Accretion";
    public const string Idle = "Stay middle";
    public const string Support = "Support";
    public const string Dps = "DPS";

    public static readonly string[] Places = ["First", "Second", "Third"];
    public static readonly string[] Groups = [Dps, Support, Accretion];

    [GeneratedRegex(@"^Set\s+(\d+)\s+(\d+)(?:st|nd|rd|th)\s+hit:\s*(.+)$",
        RegexOptions.IgnoreCase)]
    private static partial Regex HitLine();

    [GeneratedRegex(@"take\s+(\d+)(?:st|nd|rd|th)\s+tether", RegexOptions.IgnoreCase)]
    private static partial Regex TakeTether();

    [GeneratedRegex(@"#(\d+)\s+([A-Za-z]+)")]
    private static partial Regex OrderPair();

    public static string Name(string? place, string? group) =>
        string.IsNullOrWhiteSpace(place) || string.IsNullOrWhiteSpace(group)
            ? ""
            : $"{place} in Line {Full(group)}";

    public static string Full(string group) =>
        group.StartsWith(Support, StringComparison.OrdinalIgnoreCase) ? Support
        : group.StartsWith(Accretion, StringComparison.OrdinalIgnoreCase) ? Accretion
        : Dps;

    public static string Ordinal(int n) => n switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ => $"{n}th",
    };

    public static IReadOnlyList<string> Assignments(string payload)
    {
        var joined = payload.Replace(AccretionJoin, AccretionName, StringComparison.OrdinalIgnoreCase);
        var parts = new List<string>();

        foreach (var part in joined.Split(',', '+'))
        {
            var trimmed = part.Trim();
            if (trimmed.Length > 0) parts.Add(trimmed);
        }

        return parts;
    }

    public static IReadOnlyList<TetherDuty> Duties(IEnumerable<string> timeline, string me)
    {
        var found = new List<TetherDuty>();
        if (me.Length == 0) return found;

        foreach (var raw in timeline)
        {
            var match = HitLine().Match(raw.Trim());
            if (!match.Success) continue;

            var set = int.Parse(match.Groups[1].Value);
            var hit = int.Parse(match.Groups[2].Value);

            foreach (var assignment in Assignments(match.Groups[3].Value))
            {
                if (!assignment.StartsWith(me, StringComparison.OrdinalIgnoreCase)) continue;

                var both = assignment.Contains(BothMark, StringComparison.OrdinalIgnoreCase);
                var take = TakeTether().Match(assignment);
                int? tether = take.Success ? int.Parse(take.Groups[1].Value) : null;

                found.Add(new TetherDuty(set, hit, both, tether));
            }
        }

        return found;
    }

    public static IReadOnlyDictionary<string, int> Order(IEnumerable<string> lines)
    {
        var order = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            if (!line.Contains("CW Order", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (Match pair in OrderPair().Matches(line))
                order[Full(pair.Groups[2].Value)] = int.Parse(pair.Groups[1].Value);

            if (order.Count > 0) break;
        }

        return order;
    }

    public static IReadOnlyList<string> Lines(
        IReadOnlyList<string> timeline, IReadOnlyList<string> rules, int set,
        string? place, string? group, IReadOnlyList<string>? spots,
        IReadOnlyList<string>? pair = null)
    {
        var me = Name(place, group);
        if (me.Length == 0) return [];

        var duties = Duties(timeline, me);
        var mine = duties.Where(d => d.Set == set).ToList();
        if (mine.Count == 0) return duties.Count > 0 ? [Idle] : [];

        var order = Order(rules);
        var grouped = new List<(string Where, List<int> Hits)>();

        foreach (var duty in mine)
        {
            var index = duty.Tether ?? (group is null ? null : Rank(order, group));
            var where = Where(duty, index, spots, pair);

            var slot = grouped.FindIndex(x => string.Equals(x.Where, where, StringComparison.Ordinal));
            if (slot < 0) grouped.Add((where, [duty.Hit]));
            else if (!grouped[slot].Hits.Contains(duty.Hit)) grouped[slot].Hits.Add(duty.Hit);
        }

        return PlanStep.Once([.. grouped.Select(x => Say(x.Where, x.Hits))]);
    }

    public static string Say(string where, IReadOnlyList<int> hits)
    {
        var when = $"{Named([.. hits.Order().Select(Ordinal)])} hit";
        return where.Length == 0 ? when : $"{where}, {when}";
    }

    public static int? Rank(IReadOnlyDictionary<string, int> order, string group) =>
        order.TryGetValue(Full(group), out var rank) ? rank : null;

    public static string Where(
        TetherDuty duty, int? tether, IReadOnlyList<string>? spots,
        IReadOnlyList<string>? pair = null)
    {
        if (duty.Both)
        {
            var all = Named(pair ?? spots);
            return all.Length > 0 ? $"Both tethers, {all}" : "Both tethers";
        }

        var where = At(spots, tether);
        if (where.Length > 0) return where;

        return tether is null ? "" : $"{Ordinal(tether.Value)} tether";
    }

    public static string At(IReadOnlyList<string>? spots, int? tether) =>
        spots is not null && tether is { } n && n >= 1 && n <= spots.Count ? spots[n - 1] : "";

    public static string Named(IReadOnlyList<string>? spots)
    {
        if (spots is null || spots.Count == 0) return "";
        if (spots.Count == 1) return spots[0];
        return $"{string.Join(", ", spots.Take(spots.Count - 1))} and {spots[^1]}";
    }
}
