using System.Text;

namespace FrenRaidTools.Engine;

public static class CallText
{
    private static readonly Dictionary<string, string> Plain =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["target"] = "name",
            ["source"] = "the caster",
            ["stacks"] = "names",
            ["buddy"] = "partner",
            ["spot"] = "your spot",
            ["spotSpeech"] = "your spot",
            ["safe"] = "safe side",
            ["remaining"] = "countdown",
            ["myNumber"] = "your number",
            ["myPosition"] = "your spot",
            ["myNextMech"] = "mechanic",
        };

    private static readonly string[] Names = ["Kefka", "Exdeath", "Chaos"];

    public const string CountsDown = "counts down";

    public static string Says(string speech, string text)
    {
        var source = speech.Length > 0 ? speech : text;
        return Opening(Readable(Strip(source)));
    }

    public static string Screen(string speech, string text)
    {
        if (speech.Length == 0 || text.Length == 0) return "";
        var screen = Opening(Readable(Strip(text)));
        return screen == Says(speech, "") ? "" : screen;
    }

    public static (string Name, string Tags) Head(Callout call, string mechanic)
    {
        var text = Readable(LabelText.Under(mechanic, call.Description));
        var tags = new List<string>();

        text = PullTags(text, tags);
        text = text.Replace(" Resolving", "", StringComparison.Ordinal)
            .Replace("Debuff Set", "Set", StringComparison.Ordinal);
        text = DropAbbreviation(text, mechanic);
        text = LabelText.Of(text.Replace(": ", ", ", StringComparison.Ordinal));

        if (call.FromDuration) tags.Add(CountsDown);

        return (Opening(Sentence(Placeholders.Tidy(text))), string.Join("   ", tags));
    }

    private static string Strip(string text) =>
        text.Replace(Callout.CountdownToken, "", StringComparison.Ordinal);

    private static string PullTags(string text, List<string> tags)
    {
        var built = new StringBuilder(text.Length);
        var at = 0;

        while (at < text.Length)
        {
            var open = text.IndexOf('(', at);
            var close = open < 0 ? -1 : text.IndexOf(')', open + 1);
            if (close < 0)
            {
                built.Append(text, at, text.Length - at);
                break;
            }

            built.Append(text, at, open - at);
            at = close + 1;

            var inside = text[(open + 1)..close].Trim();
            if (inside.EndsWith(" Applied", StringComparison.OrdinalIgnoreCase))
                inside = inside[..^" Applied".Length].Trim();
            if (inside.Length == 0) continue;
            if (inside.Equals("All Sets", StringComparison.OrdinalIgnoreCase)) continue;
            if (inside.Equals("Applied", StringComparison.OrdinalIgnoreCase)) continue;

            tags.Add(Sentence(inside));
        }

        return built.ToString();
    }

    private static string DropAbbreviation(string text, string mechanic)
    {
        if (mechanic.Length == 0) return text;

        var colon = text.IndexOf(':');
        if (colon <= 0 || colon > 4) return text;

        var head = text[..colon];
        if (!head.All(char.IsUpper)) return text;

        return text[(colon + 1)..].TrimStart();
    }

    private static string Opening(string text) =>
        text.Length == 0 || !char.IsLower(text[0])
            ? text
            : char.ToUpperInvariant(text[0]) + text[1..];

    private static string Sentence(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 1; i < words.Length; i++)
            if (Lowerable(words[i])) words[i] = words[i].ToLowerInvariant();

        return string.Join(' ', words);
    }

    private static bool Lowerable(string word)
    {
        var runs = 0;
        var at = 0;

        while (at < word.Length)
        {
            if (!char.IsLetter(word[at])) { at++; continue; }

            var end = at;
            while (end < word.Length && char.IsLetter(word[end])) end++;

            var run = word[at..end];
            at = end;
            runs++;

            if (run.Length == 1) return false;
            if (!char.IsUpper(run[0])) continue;
            if (run[1..].Any(char.IsUpper)) return false;
            if (Names.Contains(run, StringComparer.OrdinalIgnoreCase)) return false;
        }

        return runs > 0;
    }

    private static string Readable(string template)
    {
        if (!template.Contains('{', StringComparison.Ordinal)) return template;

        var built = new StringBuilder(template.Length);
        var at = 0;

        while (at < template.Length)
        {
            var open = template.IndexOf('{', at);
            if (open < 0)
            {
                built.Append(template, at, template.Length - at);
                break;
            }

            var close = template.IndexOf('}', open + 1);
            if (close < 0)
            {
                built.Append(template, at, template.Length - at);
                break;
            }

            built.Append(template, at, open - at);
            built.Append(Choice(template[(open + 1)..close]));
            at = close + 1;
        }

        return Placeholders.Tidy(built.ToString());
    }

    private static string Choice(string expression)
    {
        var text = expression.Trim();
        var question = Outside(text, '?');

        if (question >= 0)
        {
            var colon = Outside(text, ':', question + 1);
            if (colon > question)
                return Choice(text[(question + 1)..colon]) + " / " + Choice(text[(colon + 1)..]);
        }

        return Word(text);
    }

    private static string Word(string expression)
    {
        var text = expression.Trim();
        if (text.Length == 0) return "";
        if (text.Length >= 2 && text[0] == '\'' && text[^1] == '\'') return text[1..^1];

        var dot = text.LastIndexOf('.');
        var name = dot >= 0 ? text[(dot + 1)..] : text;
        if (Plain.TryGetValue(name, out var plain)) return plain;

        var built = new StringBuilder(name.Length + 4);
        foreach (var c in name)
        {
            if (char.IsDigit(c)) continue;
            if (char.IsUpper(c) && built.Length > 0) built.Append(' ');
            built.Append(char.ToLowerInvariant(c));
        }

        return built.ToString().Trim();
    }

    private static int Outside(string text, char wanted, int from = 0)
    {
        var quoted = false;
        for (var i = from; i < text.Length; i++)
        {
            if (text[i] == '\'') quoted = !quoted;
            else if (!quoted && text[i] == wanted) return i;
        }

        return -1;
    }
}
