namespace FrenRaidTools.Engine;

public sealed record PlannedFight(string Key, string Name, string Tier, ushort Territory)
{
    public string Resource => $"FrenRaidTools.Engine.Assets.plan-{Key}.json";

    public string Expansion => Tier.Split(' ')[0];

    public string Category => Tier.IndexOf(' ') is var at && at >= 0 ? Tier[(at + 1)..] : Tier;

    public string FullName => Category.Length > 0 ? $"{Name} {Category}" : Name;
}

public static class FightPlans
{
    public static readonly IReadOnlyList<PlannedFight> All =
    [
        new("umad", "Dancing Mad", "Dawntrail Ultimate", EngineInfo.DancingMadTerritory),
    ];

    public static PlannedFight? ByKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        foreach (var fight in All)
            if (fight.Key == key) return fight;
        return null;
    }

    public static PlannedFight? InZone(uint territory)
    {
        foreach (var fight in All)
            if (fight.Territory == territory) return fight;
        return null;
    }

    public static IEnumerable<string> Expansions
    {
        get
        {
            var seen = new List<string>();
            foreach (var fight in All)
                if (!seen.Contains(fight.Expansion)) seen.Add(fight.Expansion);
            return seen;
        }
    }

    public static IEnumerable<PlannedFight> In(string expansion)
    {
        foreach (var fight in All)
            if (fight.Expansion == expansion) yield return fight;
    }

    public static PlannedFight First => All[0];
}
