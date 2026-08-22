using System.Text;

namespace FrenRaidTools.Engine;

public static class SpeechText
{
    private static readonly Dictionary<string, string> Spoken = new(StringComparer.Ordinal)
    {
        ["N"] = "North",
        ["S"] = "South",
        ["E"] = "East",
        ["W"] = "West",
        ["NE"] = "Northeast",
        ["NW"] = "Northwest",
        ["SE"] = "Southeast",
        ["SW"] = "Southwest",
        ["NNE"] = "North northeast",
        ["ENE"] = "East northeast",
        ["ESE"] = "East southeast",
        ["SSE"] = "South southeast",
        ["SSW"] = "South southwest",
        ["WSW"] = "West southwest",
        ["WNW"] = "West northwest",
        ["NNW"] = "North northwest",
        ["CW"] = "clockwise",
        ["CCW"] = "counterclockwise",
        ["KB"] = "knockback",
        ["TB"] = "tankbuster",
        ["HTMR"] = "H T M R",
    };

    public static string Of(string line)
    {
        var words = Widen(line.Replace("→", " to "));
        return Squeeze(Join(words));
    }

    public static string Plain(string line) =>
        string.IsNullOrEmpty(line) || !line.Contains('/', StringComparison.Ordinal)
            ? line
            : Squeeze(line.Replace('/', ' '));

    private static string Widen(string line)
    {
        var built = new StringBuilder(line.Length + 16);
        var at = 0;
        while (at < line.Length)
        {
            if (!char.IsLetter(line[at]))
            {
                built.Append(line[at++]);
                continue;
            }

            var end = at;
            while (end < line.Length && char.IsLetter(line[end])) end++;

            var word = line[at..end];
            built.Append(Spoken.TryGetValue(word, out var said) ? said : word);
            at = end;
        }
        return built.ToString();
    }

    private static string Join(string line)
    {
        var built = new StringBuilder(line.Length + 8);
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] != '/') { built.Append(line[i]); continue; }
            if (i == 0 || i == line.Length - 1) { built.Append(' '); continue; }
            built.Append(" or ");
        }
        return built.ToString();
    }

    private static string Squeeze(string line)
    {
        var built = new StringBuilder(line.Length);
        var gap = false;
        foreach (var c in line)
        {
            if (char.IsWhiteSpace(c)) { gap = true; continue; }
            if (gap && built.Length > 0) built.Append(' ');
            gap = false;
            built.Append(c);
        }
        return built.ToString();
    }
}
