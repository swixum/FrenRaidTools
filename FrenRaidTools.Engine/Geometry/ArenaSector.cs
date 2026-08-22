namespace FrenRaidTools.Engine;

public enum ArenaSector
{
    Unknown = -2,
    Center = -1,
    North = 0,
    Northeast = 1,
    East = 2,
    Southeast = 3,
    South = 4,
    Southwest = 5,
    West = 6,
    Northwest = 7,
}

public static class ArenaSectors
{
    public const int Eighths = 8;

    public static readonly ArenaSector[] Clockwise =
    [
        ArenaSector.North, ArenaSector.Northeast, ArenaSector.East, ArenaSector.Southeast,
        ArenaSector.South, ArenaSector.Southwest, ArenaSector.West, ArenaSector.Northwest,
    ];

    public static bool IsCardinal(this ArenaSector sector) =>
        sector is ArenaSector.North or ArenaSector.East or ArenaSector.South or ArenaSector.West;

    public static bool IsPoint(this ArenaSector sector) =>
        sector is not (ArenaSector.Unknown or ArenaSector.Center);

    public static ArenaSector Opposite(this ArenaSector sector) =>
        sector.IsPoint() ? Wrap((int)sector + 4) : sector;

    public static int EighthsTo(this ArenaSector from, ArenaSector to) =>
        from.IsPoint() && to.IsPoint()
            ? (((int)to - (int)from) % Eighths + Eighths) % Eighths
            : -1;

    public static bool IsStrictlyAdjacentTo(this ArenaSector a, ArenaSector b)
    {
        var step = a.EighthsTo(b);
        return step is 1 or Eighths - 1;
    }

    public static ArenaSector PlusEighths(this ArenaSector sector, int steps) =>
        sector.IsPoint() ? Wrap((int)sector + steps) : sector;

    public static ArenaSector PlusQuads(this ArenaSector sector, int quads) =>
        sector.PlusEighths(quads * 2);

    public static ArenaSector Wrap(int index) =>
        Clockwise[((index % Eighths) + Eighths) % Eighths];

    public static string Name(this ArenaSector sector) => sector switch
    {
        ArenaSector.North => "North",
        ArenaSector.Northeast => "Northeast",
        ArenaSector.East => "East",
        ArenaSector.Southeast => "Southeast",
        ArenaSector.South => "South",
        ArenaSector.Southwest => "Southwest",
        ArenaSector.West => "West",
        ArenaSector.Northwest => "Northwest",
        ArenaSector.Center => "Center",
        _ => "Unknown",
    };

    public static string? Told(this ArenaSector sector) =>
        sector == ArenaSector.Unknown ? null : sector.Name();

    public static List<string>? Told(this IEnumerable<ArenaSector> sectors)
    {
        var named = sectors.Where(s => s != ArenaSector.Unknown).Select(Name).ToList();
        return named.Count == 0 ? null : named;
    }
}

public readonly record struct ArenaBearing(
    double CenterX, double CenterY,
    double X, double Y,
    double OffsetX, double OffsetY,
    double Degrees,
    ArenaSector Sector,
    string From);

public sealed record ArenaPos(double CenterX, double CenterY, double ToleranceX, double ToleranceY)
{
    public static Action<ArenaBearing>? Trace { get; set; }

    private ArenaSector Told(Position pos, double dx, double dy, double degrees, ArenaSector sector, string from)
    {
        Trace?.Invoke(new ArenaBearing(
            CenterX, CenterY, pos.X, pos.Y, dx, dy, degrees, sector, from));
        return sector;
    }

    public ArenaSector For(Position pos)
    {
        if (!pos.Known) return ArenaSector.Unknown;

        var dx = pos.X - CenterX;
        var dy = pos.Y - CenterY;

        var degrees = Math.Atan2(dx, -dy) * 180.0 / Math.PI;

        if (Math.Abs(dx) <= ToleranceX && Math.Abs(dy) <= ToleranceY)
            return Told(pos, dx, dy, degrees, ArenaSector.Center, "point");

        var step = (int)Math.Round(degrees / 45.0);
        return Told(pos, dx, dy, degrees, ArenaSectors.Wrap(step), "point");
    }

    public ArenaSector For(Actor? actor) =>
        actor is null ? ArenaSector.Unknown : For(actor.Pos);

    public ArenaSector For(Position pos, IReadOnlyList<ArenaSector> spots)
    {
        if (!pos.Known) return ArenaSector.Unknown;
        if (spots.Count == 0) return For(pos);

        var dx = pos.X - CenterX;
        var dy = pos.Y - CenterY;

        var degrees = Math.Atan2(dx, -dy) * 180.0 / Math.PI;

        if (Math.Abs(dx) <= ToleranceX && Math.Abs(dy) <= ToleranceY)
            return Told(pos, dx, dy, degrees,
                spots.Contains(ArenaSector.Center) ? ArenaSector.Center : ArenaSector.Unknown,
                "spots");

        var best = ArenaSector.Unknown;
        var closest = double.MaxValue;
        foreach (var spot in spots)
        {
            if (!spot.IsPoint()) continue;
            var away = Math.Abs(Turn(degrees - (int)spot * 45.0));
            if (away >= closest) continue;
            closest = away;
            best = spot;
        }

        return Told(pos, dx, dy, degrees, best, "spots");
    }

    public ArenaSector For(Actor? actor, IReadOnlyList<ArenaSector> spots) =>
        actor is null ? ArenaSector.Unknown : For(actor.Pos, spots);

    private static double Turn(double degrees)
    {
        var wrapped = degrees % 360.0;
        if (wrapped > 180.0) wrapped -= 360.0;
        if (wrapped < -180.0) wrapped += 360.0;
        return wrapped;
    }

    public static ArenaSector PointOf(double heading) =>
        ArenaSectors.Wrap((int)Math.Round(4.0 - 4.0 * heading / Math.PI));

    public static ArenaSector Facing(double heading)
    {
        var sector = PointOf(heading);

        Trace?.Invoke(new ArenaBearing(
            double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
            heading * 180.0 / Math.PI, sector, "facing"));

        return sector;
    }
}
