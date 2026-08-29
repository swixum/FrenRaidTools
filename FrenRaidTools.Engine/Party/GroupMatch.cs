namespace FrenRaidTools.Engine;

public static class GroupMatch
{
    public const int Ambiguous = -2;
    public const int Floor = 4;
    public const int Margin = 2;

    public static bool Same(string? a, string? b)
    {
        var left = (a ?? "").Trim();
        var right = (b ?? "").Trim();
        if (left.Length == 0 || right.Length == 0) return false;
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase)) return true;
        if (left.Contains(' ') && right.Contains(' ')) return false;

        return string.Equals(PlayerName.First(left), PlayerName.First(right),
            StringComparison.OrdinalIgnoreCase);
    }

    public static int Score(Roster roster, IReadOnlyList<PartyMember> party)
    {
        if (roster is null || party is null || party.Count == 0) return 0;

        roster.Normalize();
        var found = 0;

        foreach (var player in roster.Players)
        {
            if (string.IsNullOrWhiteSpace(player)) continue;

            foreach (var member in party)
                if (Same(player, member.Name))
                {
                    found++;
                    break;
                }
        }

        return found;
    }

    public static int Pick(IReadOnlyList<Roster> setups, IReadOnlyList<PartyMember> party, int current)
    {
        if (setups is null || setups.Count == 0) return current;
        if (party is null || party.Count < Floor) return current;

        var mine = current >= 0 && current < setups.Count ? Score(setups[current], party) : 0;
        var best = current;
        var top = mine;

        for (var i = 0; i < setups.Count; i++)
        {
            if (i == current) continue;

            var score = Score(setups[i], party);
            if (score <= top) continue;

            top = score;
            best = i;
        }

        if (best == current || top < Floor) return current;
        return top - mine >= Margin ? best : current;
    }

    public static string Label(IReadOnlyList<Roster> setups, int index)
    {
        if (setups is null || index < 0 || index >= setups.Count) return "";
        var name = (setups[index].Name ?? "").Trim();
        return name.Length > 0 ? name : $"Group {index + 1}";
    }

    public static int ByName(IReadOnlyList<Roster> setups, string word)
    {
        if (setups is null) return -1;

        var wanted = (word ?? "").Trim();
        if (wanted.Length == 0) return -1;

        for (var i = 0; i < setups.Count; i++)
            if (string.Equals(Label(setups, i), wanted, StringComparison.OrdinalIgnoreCase)) return i;

        var found = Only(setups, wanted, true);
        return found == -1 ? Only(setups, wanted, false) : found;
    }

    private static int Only(IReadOnlyList<Roster> setups, string wanted, bool fromTheStart)
    {
        var found = -1;

        for (var i = 0; i < setups.Count; i++)
        {
            var name = Label(setups, i);
            var hit = fromTheStart
                ? name.StartsWith(wanted, StringComparison.OrdinalIgnoreCase)
                : name.Contains(wanted, StringComparison.OrdinalIgnoreCase);

            if (!hit) continue;
            if (found >= 0) return Ambiguous;
            found = i;
        }

        return found;
    }
}
