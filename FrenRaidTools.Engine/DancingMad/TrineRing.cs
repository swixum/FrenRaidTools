namespace FrenRaidTools.Engine.DancingMad;

public static class TrineRing
{
    public const double CenterX = 100.0;
    public const double CenterY = 100.0;
    public const double CenterRadius = 5.0;
    public const double Tolerance = 25.0;

    public const string PartyStart = "A";
    public const string TankStart = "1";

    public static readonly string[] Names = ["A", "2B", "3", "C", "D4", "1"];

    public static readonly double[] Bearings = [11.0, 71.0, 131.0, 191.0, 251.0, 311.0];

    public static int IndexOf(string name) => Array.IndexOf(Names, name);

    public static string? Name(int spot) =>
        spot >= 0 && spot < Names.Length ? Names[spot] : null;

    public static int Spot(Position pos)
    {
        if (!pos.Known) return -1;

        var dx = pos.X - CenterX;
        var dy = pos.Y - CenterY;

        if (Math.Sqrt(dx * dx + dy * dy) <= CenterRadius) return -1;

        var degrees = Math.Atan2(dx, -dy) * 180.0 / Math.PI;

        var best = -1;
        var closest = Tolerance;

        for (var i = 0; i < Bearings.Length; i++)
        {
            var away = Math.Abs(Turn(degrees - Bearings[i]));
            if (away > closest) continue;
            closest = away;
            best = i;
        }

        return best;
    }

    public static string? FirstPopped(IReadOnlyCollection<int> opening, string from, int step)
    {
        var start = IndexOf(from);
        if (start < 0) return null;

        for (var i = 0; i < Names.Length; i++)
        {
            var at = ((start + i * step) % Names.Length + Names.Length) % Names.Length;
            if (opening.Contains(at)) return Names[at];
        }

        return null;
    }

    private static double Turn(double degrees)
    {
        var wrapped = degrees % 360.0;
        if (wrapped > 180.0) wrapped -= 360.0;
        if (wrapped < -180.0) wrapped += 360.0;
        return wrapped;
    }
}
