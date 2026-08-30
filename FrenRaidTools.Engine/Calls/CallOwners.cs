namespace FrenRaidTools.Engine;

public sealed class CallOwners
{
    private readonly Dictionary<string, string> _byGroup = new(StringComparer.Ordinal);

    public void Claim(string group, string fightKey)
    {
        if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(fightKey)) return;
        _byGroup[group] = fightKey;
    }

    public string Owner(string group) => _byGroup.GetValueOrDefault(group, "");

    public bool Shows(string fightKey, string group)
    {
        var owner = Owner(group);
        return owner.Length == 0 || owner == fightKey;
    }

    public IEnumerable<CatalogEntry> Only(string fightKey, IEnumerable<CatalogEntry> entries)
    {
        foreach (var entry in entries)
            if (Shows(fightKey, entry.Group)) yield return entry;
    }
}
