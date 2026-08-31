namespace FrenRaidTools.Engine;

public static class QuietSeed
{
    public const int Round = 3;

    public static readonly string[] QuietAgain =
    [
        "earthquakePersistentTracker",
        "realDynamicFluid",
        "realEntropy",
        "fakeDynamicFluid",
        "fakeEntropy",
        "secondRealDynamicFluid",
        "secondRealEntropy",
        "secondFakeDynamicFluid",
        "secondFakeEntropy",
    ];

    public static bool Forget(ISet<string> seeded)
    {
        var changed = false;
        foreach (var key in QuietAgain) changed |= seeded.Remove(key);
        return changed;
    }

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
