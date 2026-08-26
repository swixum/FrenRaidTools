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
        IReadOnlyList<string>? pair = null,
        IReadOnlyDictionary<string, string>? holders = null)
    {
        var me = Name(place, group);
        if (me.Length == 0) return [];

        var duties = Duties(timeline, me);
        var mine = duties.Where(d => d.Set == set).ToList();
        if (mine.Count == 0) return duties.Count > 0 ? [Idle] : [];

        var order = Order(rules);
        var lines = new List<string>();
        var soaks = new List<(string Where, List<int> Hits)>();

        foreach (var duty in mine)
        {
            if (duty.Both)
            {
                var all = Named(pair ?? spots);
                lines.Add(all.Length > 0 ? $"Take 2 Tether Hits, {all}" : "Take 2 Tether Hits");
                continue;
            }

            if (duty.Tether is { } taken)
            {
                lines.Add(Steal(timeline, order, set, duty.Hit, taken, holders));
                continue;
            }

            var where = At(spots, group is null ? null : Rank(order, group));
            var slot = soaks.FindIndex(x => string.Equals(x.Where, where, StringComparison.Ordinal));
            if (slot < 0) soaks.Add((where, [duty.Hit]));
            else if (!soaks[slot].Hits.Contains(duty.Hit)) soaks[slot].Hits.Add(duty.Hit);
        }

        lines.AddRange(soaks.Select(x => Say(x.Where, x.Hits)));
        return PlanStep.Once(lines);
    }

    public static string Say(string where, IReadOnlyList<int> hits)
    {
        var when = $"{Named([.. hits.Order().Select(Ordinal)])} hit";
        return where.Length == 0 ? when : $"{where}, {when}";
    }

    public static string Steal(
        IReadOnlyList<string> timeline, IReadOnlyDictionary<string, int> order,
        int set, int beforeHit, int tether, IReadOnlyDictionary<string, string>? holders)
    {
        var line = $"Take {Ordinal(tether)} tether";
        if (holders is null) return line;

        var identity = HolderOf(timeline, order, set, beforeHit, tether);
        return identity is not null && holders.TryGetValue(identity, out var who) && who.Length > 0
            ? $"{line} off {who}"
            : line;
    }

    public static string? HolderOf(
        IReadOnlyList<string> timeline, IReadOnlyDictionary<string, int> order,
        int set, int beforeHit, int tether)
    {
        string? holder = null;

        foreach (var raw in timeline)
        {
            var match = HitLine().Match(raw.Trim());
            if (!match.Success) continue;
            if (int.Parse(match.Groups[1].Value) != set) continue;
            if (int.Parse(match.Groups[2].Value) >= beforeHit) continue;

            foreach (var assignment in Assignments(match.Groups[3].Value))
            {
                var identity = IdentityLine().Match(assignment);
                if (!identity.Success) continue;

                var take = TakeTether().Match(assignment);
                var held = take.Success
                    ? int.Parse(take.Groups[1].Value)
                    : Rank(order, identity.Groups[3].Value);

                if (held == tether) holder = identity.Groups[1].Value;
            }
        }

        return holder;
    }

    [GeneratedRegex(@"^((First|Second|Third)\s+in\s+Line\s+(DPS|Support|Accretion))\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex IdentityLine();

    public static int? Rank(IReadOnlyDictionary<string, int> order, string group) =>
        order.TryGetValue(Full(group), out var rank) ? rank : null;

    public static string At(IReadOnlyList<string>? spots, int? tether) =>
        spots is not null && tether is { } n && n >= 1 && n <= spots.Count ? spots[n - 1] : "";

    public static string Named(IReadOnlyList<string>? spots)
    {
        if (spots is null || spots.Count == 0) return "";
        if (spots.Count == 1) return spots[0];
        return $"{string.Join(", ", spots.Take(spots.Count - 1))} and {spots[^1]}";
    }
}
