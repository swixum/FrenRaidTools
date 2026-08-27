namespace FrenRaidTools.Engine;

public enum SpotCheck
{
    Empty,
    Unchecked,
    Confirmed,
    NearMiss,
    Absent,
}

public readonly record struct SpotVerdict(SpotCheck Check, string Suggestion);

public static class RosterCheck
{
    public const int CloseEnough = 2;

    public static SpotVerdict[] Against(Roster roster, IReadOnlyList<string> party)
    {
        roster.Normalize();

        var confirmed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in roster.Players)
        {
            var wanted = name.Trim();
            if (wanted.Length == 0) continue;
            if (party.Any(p => string.Equals(p.Trim(), wanted, StringComparison.OrdinalIgnoreCase)))
                confirmed.Add(wanted);
        }

        var verdicts = new SpotVerdict[Slots.Count];
        for (var i = 0; i < Slots.Count; i++)
            verdicts[i] = For(roster.Players[i], party, confirmed);

        return verdicts;
    }

    public static SpotVerdict For(
        string name, IReadOnlyList<string> party, IReadOnlyCollection<string> seated)
    {
        if (string.IsNullOrWhiteSpace(name)) return new(SpotCheck.Empty, "");
        if (party.Count == 0) return new(SpotCheck.Unchecked, "");

        var wanted = name.Trim();

        foreach (var member in party)
            if (string.Equals(member.Trim(), wanted, StringComparison.OrdinalIgnoreCase))
                return new(SpotCheck.Confirmed, "");

        var best = "";
        var bestDistance = int.MaxValue;

        foreach (var member in party)
        {
            var candidate = member.Trim();
            if (candidate.Length == 0) continue;
            if (seated.Contains(candidate)) continue;

            var distance = Distance(wanted.ToUpperInvariant(), candidate.ToUpperInvariant());
            if (distance >= bestDistance) continue;

            bestDistance = distance;
            best = candidate;
        }

        return bestDistance <= CloseEnough
            ? new(SpotCheck.NearMiss, best)
            : new(SpotCheck.Absent, "");
    }

    public static int Distance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++) previous[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var swap = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + swap);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
