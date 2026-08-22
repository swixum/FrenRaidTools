using System.Text.RegularExpressions;

namespace FrenRaidTools.Engine;

public static partial class CallWords
{
    public const string OutsideFront = "Outside Front";
    public const string OutsideBack = "Outside Back";

    private static readonly (string Phrase, string Lead)[] Edges =
    [
        ("Out + Front edge", OutsideFront),
        ("Out + Back edge", OutsideBack),
    ];

    private static readonly (string From, string To)[] Shorter =
    [
        ("inner ring at tower edge", "Inner Edge"),
        (" onto ", " on "),
    ];

    public static string Line(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return "";

        var text = line.Trim();
        var lead = "";

        foreach (var (phrase, word) in Edges)
        {
            var at = text.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);
            if (at < 0) continue;

            lead = word;
            text = text.Remove(at, phrase.Length);
            break;
        }

        foreach (var (from, to) in Shorter)
            text = text.Replace(from, to, StringComparison.OrdinalIgnoreCase);

        text = Hedge().Replace(text, "$1");
        text = Doubled().Replace(text, "North");
        text = Ring().Replace(text, "");

        text = Tidy(text);
        return lead.Length == 0 ? text : Tidy($"{lead} {text}");
    }

    private static string Tidy(string text)
    {
        var squeezed = Gaps().Replace(text, " ");
        squeezed = Commas().Replace(squeezed, ", ");
        return squeezed.Trim().Trim(',').Trim();
    }

    [GeneratedRegex(@"\b(North|South|East|West|Northeast|Northwest|Southeast|Southwest)-ish\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex Hedge();

    [GeneratedRegex(@"\bFar North\b", RegexOptions.IgnoreCase)]
    private static partial Regex Doubled();

    [GeneratedRegex(@"\s*\(outer ring\)", RegexOptions.IgnoreCase)]
    private static partial Regex Ring();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex Gaps();

    [GeneratedRegex(@"\s*,\s*(,\s*)*")]
    private static partial Regex Commas();
}
