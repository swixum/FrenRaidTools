namespace FrenRaidTools.Engine;

public static class QuietSeed
{
    public const int Round = 4;

    public static bool Wake(ISet<string> muted, ISet<string> seeded)
    {
        var changed = false;
        foreach (var key in seeded) changed |= muted.Remove(key);
        changed |= seeded.Count > 0;
        seeded.Clear();
        return changed;
    }
}
