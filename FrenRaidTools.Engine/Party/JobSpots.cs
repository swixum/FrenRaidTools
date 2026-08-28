namespace FrenRaidTools.Engine;

public sealed class JobSpots
{
    public Dictionary<string, string> Picks { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static readonly string[] Jobs =
    [
        "PLD", "WAR", "DRK", "GNB",
        "WHM", "SCH", "AST", "SGE",
        "MNK", "DRG", "NIN", "SAM", "RPR", "VPR",
        "BRD", "MCH", "DNC",
        "BLM", "SMN", "RDM", "PCT", "BLU",
    ];

    public void Normalize()
    {
        var kept = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (job, spot) in Picks ?? [])
        {
            if (string.IsNullOrWhiteSpace(job) || string.IsNullOrWhiteSpace(spot)) continue;
            var slot = Slots.IndexOf(spot.Trim());
            if (!Fits(job.Trim(), slot)) continue;
            kept[job.Trim()] = Slots.Names[slot];
        }

        Picks = kept;
    }

    public int Count => Picks.Count;

    public bool Any => Picks.Count > 0;

    public static bool Fits(string job, int slot)
    {
        if (slot < 0 || slot >= Slots.Count) return false;
        var (first, second) = Slots.Pair(JobKinds.Kind(job));
        return slot == first || slot == second;
    }

    public int SpotOf(string job)
    {
        if (string.IsNullOrWhiteSpace(job)) return -1;
        if (!Picks.TryGetValue(job.Trim(), out var spot)) return -1;

        var slot = Slots.IndexOf(spot);
        return Fits(job, slot) ? slot : -1;
    }

    public bool Set(string job, int slot)
    {
        if (string.IsNullOrWhiteSpace(job) || !Fits(job, slot)) return false;
        Picks[job.Trim()] = Slots.Names[slot];
        return true;
    }

    public void Unset(string job)
    {
        if (!string.IsNullOrWhiteSpace(job)) Picks.Remove(job.Trim());
    }

    public void Clear() => Picks.Clear();

    public bool Prefers(int slot, string job)
    {
        var wanted = SpotOf(job);
        return wanted < 0 ? Slots.Prefers(slot, job) : wanted == slot;
    }
}
