using FrenRaidTools.Engine.DancingMad;

namespace FrenRaidTools.Engine;

public sealed record VfxTell(string Path, uint BaseId, uint Id, bool Real);

public static class VfxTells
{
    public const string Folder = "vfx/common/eff/";
    public const string Lead = "vfx/common/eff/z3oy_stlp";

    public static readonly IReadOnlyList<VfxTell> All =
    [
        new($"{Folder}z3oy_stlp4_c0c.avfx", GrandCross.NpcChaos, GrandCross.FakeChaos, false),
        new($"{Folder}z3oy_stlp5_c0c.avfx", GrandCross.NpcChaos, GrandCross.RealChaos, true),
        new($"{Folder}z3oy_stlp6_c0c.avfx",
            GrandCross.NpcNeoExdeath, GrandCross.FakeNeoExdeath, false),
        new($"{Folder}z3oy_stlp7_c0c.avfx",
            GrandCross.NpcNeoExdeath, GrandCross.RealNeoExdeath, true),
    ];

    private static readonly Dictionary<string, VfxTell> ByPath =
        All.ToDictionary(t => t.Path, StringComparer.OrdinalIgnoreCase);

    public static VfxTell? For(string? path) =>
        string.IsNullOrEmpty(path) ? null : ByPath.GetValueOrDefault(Trim(path));

    public static string Trim(string path)
    {
        var end = path.IndexOf('\0');
        return (end < 0 ? path : path[..end]).Trim();
    }

    public const string LockonLead = "vfx/lockon/eff/";
    public const string LockonTail = ".avfx";

    public static string? Icon(string? path)
    {
        if (path is null) return null;
        if (!path.StartsWith(LockonLead, StringComparison.OrdinalIgnoreCase)) return null;

        var rest = Trim(path[LockonLead.Length..]);
        var end = rest.IndexOf(LockonTail, StringComparison.OrdinalIgnoreCase);
        if (end <= 0) return null;

        return rest[..end];
    }

    public static bool Names(uint baseId) =>
        baseId == GrandCross.NpcChaos || baseId == GrandCross.NpcNeoExdeath;

    public static GameEvent Event(VfxTell tell, uint objectId, double at) => new()
    {
        Kind = EventKind.StatusLoopVfx,
        Id = tell.Id,
        At = at,
        Target = new Actor { ObjectId = objectId, BaseId = tell.BaseId },
    };
}
