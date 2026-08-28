namespace FrenRaidTools.Engine;

public static class PartyPool
{
    public static bool Holds(IReadOnlyList<PartyMember> found, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;

        foreach (var member in found)
            if (string.Equals(member.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    public static int Take(List<PartyMember> found, List<(float Away, PartyMember Member)> nearby,
        int limit)
    {
        var added = 0;

        foreach (var (_, member) in nearby.OrderBy(p => p.Away))
        {
            if (found.Count >= limit) break;
            if (Holds(found, member.Name)) continue;

            found.Add(member);
            added++;
        }

        return added;
    }
}
