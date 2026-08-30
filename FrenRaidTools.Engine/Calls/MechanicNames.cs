namespace FrenRaidTools.Engine;

public sealed class MechanicNames
{
    public const string Loose = "Other calls";

    private readonly Dictionary<string, string> _byGroup = new(StringComparer.Ordinal);

    public void Claim(string group, string mechanic)
    {
        if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(mechanic)) return;
        _byGroup[group] = mechanic;
    }

    public string For(string group) => _byGroup.GetValueOrDefault(group, "");

    public string Fold(CatalogEntry entry)
    {
        var claimed = For(entry.Group);
        if (claimed.Length > 0) return claimed;
        if (entry.Call.Mechanic.Length > 0) return entry.Call.Mechanic;
        return entry.Group.Length > 0 ? entry.Group : Loose;
    }
}
