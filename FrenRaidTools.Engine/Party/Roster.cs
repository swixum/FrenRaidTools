namespace FrenRaidTools.Engine;

public enum SlotRole
{
    Tank,
    Healer,
    Melee,
    Ranged,
}

public enum JobKind
{
    Unknown,
    Tank,
    Healer,
    Melee,
    PhysRanged,
    Caster,
}

public static class Slots
{
    public static readonly string[] Names = ["MT", "OT", "H1", "H2", "M1", "M2", "R1", "R2"];

    public static int Count => Names.Length;

    public static int IndexOf(string slot) =>
        Array.FindIndex(Names, n => string.Equals(n, slot, StringComparison.OrdinalIgnoreCase));

    public static SlotRole RoleOf(int slot) => slot switch
    {
        0 or 1 => SlotRole.Tank,
        2 or 3 => SlotRole.Healer,
        4 or 5 => SlotRole.Melee,
        _ => SlotRole.Ranged,
    };

    public static bool IsSupport(int slot) => slot < 4;

    public static readonly int[] Partners = [2, 3, 0, 1, 6, 7, 4, 5];

    public static readonly int[] TowerPrio = [2, 3, 0, 1, 4, 5, 6, 7];

    public static int PrioOf(int slot) =>
        slot >= 0 && slot < TowerPrio.Length ? TowerPrio[slot] : -1;

    public static int PartnerOf(int slot) =>
        slot >= 0 && slot < Partners.Length ? Partners[slot] : -1;

    public static string PartnerSlot(string slot)
    {
        var at = PartnerOf(IndexOf(slot));
        return at < 0 ? "" : Names[at];
    }

    public static string Hint(int slot) => slot switch
    {
        0 => "Main tank",
        1 => "Off tank",
        2 => "Pure healer",
        3 => "Shield healer",
        4 => "Melee",
        5 => "Melee",
        6 => "Phys ranged",
        _ => "Caster",
    };

    public static bool Prefers(int slot, string job) => slot switch
    {
        0 or 1 => JobKinds.Kind(job) == JobKind.Tank,
        2 => JobKinds.Regen(job),
        3 => JobKinds.Shield(job),
        4 or 5 => JobKinds.Kind(job) == JobKind.Melee,
        6 => JobKinds.Kind(job) == JobKind.PhysRanged,
        7 => JobKinds.Kind(job) == JobKind.Caster,
        _ => false,
    };

    public static bool Accepts(int slot, JobKind kind) => RoleOf(slot) switch
    {
        SlotRole.Tank => kind == JobKind.Tank,
        SlotRole.Healer => kind == JobKind.Healer,
        SlotRole.Melee => kind == JobKind.Melee,
        _ => kind is JobKind.PhysRanged or JobKind.Caster,
    };
}

public static class JobKinds
{
    private static readonly Dictionary<string, JobKind> ByAbbr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GLA"] = JobKind.Tank,
        ["MRD"] = JobKind.Tank,
        ["PLD"] = JobKind.Tank,
        ["WAR"] = JobKind.Tank,
        ["DRK"] = JobKind.Tank,
        ["GNB"] = JobKind.Tank,

        ["CNJ"] = JobKind.Healer,
        ["WHM"] = JobKind.Healer,
        ["SCH"] = JobKind.Healer,
        ["AST"] = JobKind.Healer,
        ["SGE"] = JobKind.Healer,

        ["PGL"] = JobKind.Melee,
        ["LNC"] = JobKind.Melee,
        ["ROG"] = JobKind.Melee,
        ["MNK"] = JobKind.Melee,
        ["DRG"] = JobKind.Melee,
        ["NIN"] = JobKind.Melee,
        ["SAM"] = JobKind.Melee,
        ["RPR"] = JobKind.Melee,
        ["VPR"] = JobKind.Melee,

        ["ARC"] = JobKind.PhysRanged,
        ["BRD"] = JobKind.PhysRanged,
        ["MCH"] = JobKind.PhysRanged,
        ["DNC"] = JobKind.PhysRanged,

        ["THM"] = JobKind.Caster,
        ["ACN"] = JobKind.Caster,
        ["BLM"] = JobKind.Caster,
        ["SMN"] = JobKind.Caster,
        ["RDM"] = JobKind.Caster,
        ["BLU"] = JobKind.Caster,
        ["PCT"] = JobKind.Caster,
    };

    public static string Abbr(uint jobId) => Jobs.Name(jobId);

    public static JobKind Kind(uint jobId) => Kind(Jobs.Name(jobId));

    public static JobKind Kind(string abbr) =>
        !string.IsNullOrEmpty(abbr) && ByAbbr.TryGetValue(abbr, out var kind) ? kind : JobKind.Unknown;

    public static bool Support(string abbr) => Kind(abbr) is JobKind.Tank or JobKind.Healer;

    private static readonly HashSet<string> RegenHealers =
        new(["WHM", "AST", "CNJ"], StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> ShieldHealers =
        new(["SCH", "SGE"], StringComparer.OrdinalIgnoreCase);

    public static bool Regen(string abbr) => !string.IsNullOrEmpty(abbr) && RegenHealers.Contains(abbr);

    public static bool Shield(string abbr) => !string.IsNullOrEmpty(abbr) && ShieldHealers.Contains(abbr);

    private static readonly JobKind[] Order =
        [JobKind.Tank, JobKind.Healer, JobKind.Melee, JobKind.PhysRanged, JobKind.Caster];

    public static int Rank(JobKind kind)
    {
        var at = Array.IndexOf(Order, kind);
        return at < 0 ? Order.Length : at;
    }
}

public readonly record struct PartyMember(string Name, string Job)
{
    public JobKind Kind => JobKinds.Kind(Job);
}

public sealed class Roster
{
    public const string DefaultName = "Static";

    public string Name { get; set; } = DefaultName;

    public List<string> Players { get; set; } = [];

    public List<string> Jobs { get; set; } = [];

    public static List<string> Blank() => ["", "", "", "", "", "", "", ""];

    public bool Untouched =>
        Filled == 0 && string.Equals(Name, DefaultName, StringComparison.Ordinal);

    public static int DropSpares(List<Roster> setups, int active)
    {
        if (setups is null || setups.Count == 0) return 0;

        var wasOn = active >= 0 && active < setups.Count ? setups[active] : null;

        for (var i = setups.Count - 1; i >= 0; i--)
        {
            if (setups.Count <= 1) break;
            if (setups[i].Untouched) setups.RemoveAt(i);
        }

        var moved = wasOn is null ? -1 : setups.IndexOf(wasOn);
        return moved >= 0 ? moved : Math.Clamp(active, 0, setups.Count - 1);
    }

    public void Normalize()
    {
        Players ??= [];
        Jobs ??= [];

        while (Players.Count < Slots.Count) Players.Add("");
        while (Jobs.Count < Slots.Count) Jobs.Add("");
        if (Players.Count > Slots.Count) Players.RemoveRange(Slots.Count, Players.Count - Slots.Count);
        if (Jobs.Count > Slots.Count) Jobs.RemoveRange(Slots.Count, Jobs.Count - Slots.Count);

        for (var i = 0; i < Slots.Count; i++)
        {
            Players[i] ??= "";
            Jobs[i] ??= "";
        }
    }

    public int Filled => Players.Count(p => !string.IsNullOrWhiteSpace(p));

    public bool Complete => Filled == Slots.Count;

    public Roster Copy() => new() { Name = Name, Players = [.. Players], Jobs = [.. Jobs] };

    public void Clear()
    {
        Normalize();
        for (var i = 0; i < Slots.Count; i++)
        {
            Players[i] = "";
            Jobs[i] = "";
        }
    }

    public void Set(int slot, string player, string job)
    {
        Normalize();
        if (slot < 0 || slot >= Slots.Count) return;
        Players[slot] = player ?? "";
        Jobs[slot] = job ?? "";
    }

    public void Swap(int a, int b)
    {
        Normalize();
        if (a < 0 || b < 0 || a >= Slots.Count || b >= Slots.Count || a == b) return;
        (Players[a], Players[b]) = (Players[b], Players[a]);
        (Jobs[a], Jobs[b]) = (Jobs[b], Jobs[a]);
    }

    public int SlotOf(string player)
    {
        if (string.IsNullOrWhiteSpace(player)) return -1;
        Normalize();
        for (var i = 0; i < Slots.Count; i++)
            if (string.Equals(Players[i], player, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    public string SlotName(string player)
    {
        var at = SlotOf(player);
        return at < 0 ? "" : Slots.Names[at];
    }

    public string PartnerName(string player)
    {
        var at = Slots.PartnerOf(SlotOf(player));
        return at < 0 ? "" : Players[at];
    }

    public int Fill(IReadOnlyList<PartyMember> members, bool keepExisting)
    {
        Normalize();

        if (!keepExisting) Clear();

        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in Players)
            if (!string.IsNullOrWhiteSpace(name)) taken.Add(name);

        var pool = members
            .Where(m => !string.IsNullOrWhiteSpace(m.Name) && !taken.Contains(m.Name))
            .OrderBy(m => JobKinds.Rank(m.Kind))
            .ToList();

        var placed = Place(pool, (slot, m) => Slots.Prefers(slot, m.Job));
        placed += Place(pool, (slot, m) => Slots.Accepts(slot, m.Kind));
        placed += Place(pool, (_, _) => true);
        return placed;
    }

    private int Place(List<PartyMember> pool, Func<int, PartyMember, bool> fits)
    {
        var placed = 0;

        for (var slot = 0; slot < Slots.Count; slot++)
        {
            if (pool.Count == 0) break;
            if (!string.IsNullOrWhiteSpace(Players[slot])) continue;

            var pick = pool.FindIndex(m => fits(slot, m));
            if (pick < 0) continue;

            Players[slot] = pool[pick].Name;
            Jobs[slot] = pool[pick].Job;
            pool.RemoveAt(pick);
            placed++;
        }

        return placed;
    }

    public List<int> Duplicates()
    {
        Normalize();

        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var bad = new List<int>();

        for (var i = 0; i < Slots.Count; i++)
        {
            var name = Players[i];
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (seen.TryGetValue(name, out var first))
            {
                if (!bad.Contains(first)) bad.Add(first);
                bad.Add(i);
                continue;
            }

            seen[name] = i;
        }

        return bad;
    }
}
