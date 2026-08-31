namespace FrenRaidTools.Engine.Fru;

public static class FruArena
{
    public const double CenterX = 100.0;

    public const double CenterY = 100.0;

    public static readonly ArenaPos Ring = new(CenterX, CenterY, 8.0, 8.0);

    public static readonly ArenaPos Middle = new(CenterX, CenterY, 5.0, 5.0);

    public static readonly ArenaPos Close = new(CenterX, CenterY, 1.0, 1.0);

    public static readonly string[] PhaseNames =
    [
        "P1 Fatebreaker", "P2 Usurper of Frost", "Intermission Crystals",
        "P3 Oracle of Darkness", "P4 Usurper and Oracle", "P5 Pandora",
    ];

    public static ArenaSector SectorOf(IWorld world, Actor? actor, ArenaPos? pos = null) =>
        actor is null ? ArenaSector.Unknown
            : (pos ?? Ring).For(world.Latest(actor) ?? actor);

    public static bool Mine(GameEvent got, IWorld world) =>
        world.You is not null
        && ((got.Target is not null && got.Target.ObjectId == world.You.ObjectId)
            || (got.Source is not null && got.Source.ObjectId == world.You.ObjectId));

    public static string Spoken(this ArenaSector sector) =>
        sector.Name().ToLowerInvariant();

    public const string Key = "fru";

    private static readonly Dictionary<string, string> Digits =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["One"] = "1", ["Two"] = "2", ["Three"] = "3", ["Four"] = "4",
        };

    public static string? MarkAt(this ArenaSector sector) =>
        WaymarkPresets.For(Key).Mark(sector);

    public static string Short(this ArenaSector sector)
    {
        var mark = sector.MarkAt();
        return mark is null ? sector.Name() : Digits.GetValueOrDefault(mark, mark);
    }

    public static string SpokenMark(this ArenaSector sector)
    {
        var mark = sector.MarkAt();
        return mark is null ? "" : mark.Length == 1 ? mark : mark.ToLowerInvariant();
    }
}
