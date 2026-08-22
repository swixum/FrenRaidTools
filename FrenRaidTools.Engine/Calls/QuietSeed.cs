namespace FrenRaidTools.Engine;

public static class QuietSeed
{
    public static bool Apply(
        IEnumerable<CatalogEntry> entries, ISet<string> muted, ISet<string> seeded)
    {
        var changed = false;

        foreach (var entry in entries)
        {
            if (entry.Call.OnByDefault) continue;
            if (!seeded.Add(entry.Key)) continue;

            muted.Add(entry.Key);
            changed = true;
        }

        return changed;
    }
}
