using System.Text;

namespace FrenRaidTools.Engine;

public static class Compass
{
    public const double StepDegrees = 45;

    private static readonly string[] Long =
    [
        "North", "Northeast", "East", "Southeast",
        "South", "Southwest", "West", "Northwest",
    ];

    private static readonly string[] Short = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

    private static readonly Dictionary<string, int> LongPoints = BuildPoints(Long);
    private static readonly Dictionary<string, int> ShortPoints = BuildPoints(Short);

    private static Dictionary<string, int> BuildPoints(string[] names)
    {
        var points = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < names.Length; i++) points[names[i]] = i;
        return points;
    }

    public static bool IsPoint(string word) =>
        LongPoints.ContainsKey(word) || (IsUpper(word) && ShortPoints.ContainsKey(word));

    public static string Rotate(string text, double degrees)
    {
        if (text.Length == 0) return text;

        var steps = Steps(degrees);
        if (steps == 0) return text;

        var built = new StringBuilder(text.Length);
        var start = 0;
        while (start < text.Length)
        {
            if (!char.IsLetter(text[start]))
            {
                built.Append(text[start++]);
                continue;
            }

            var end = start;
            while (end < text.Length && char.IsLetter(text[end])) end++;

            var word = text[start..end];
            built.Append(Turn(word, steps) ?? word);
            start = end;
        }

        return built.ToString();
    }

    public static int Steps(double degrees)
    {
        var raw = degrees / StepDegrees;
        var steps = (int)Math.Round(raw);
        if (Math.Abs(raw - steps) > 1e-6)
            throw new ArgumentOutOfRangeException(
                nameof(degrees), degrees, "A turn has to land on one of the eight points.");
        return ((steps % Long.Length) + Long.Length) % Long.Length;
    }

    private static string? Turn(string word, int steps)
    {
        if (LongPoints.TryGetValue(word, out var longPoint))
            return MatchCase(word, Long[(longPoint + steps) % Long.Length]);

        if (IsUpper(word) && ShortPoints.TryGetValue(word, out var shortPoint))
            return Short[(shortPoint + steps) % Short.Length];

        return null;
    }

    private static bool IsUpper(string word)
    {
        foreach (var c in word)
            if (!char.IsUpper(c)) return false;
        return word.Length > 0;
    }

    private static string MatchCase(string original, string turned)
    {
        if (IsUpper(original) && original.Length > 1) return turned.ToUpperInvariant();
        if (char.IsUpper(original[0])) return turned;
        return turned.ToLowerInvariant();
    }
}
