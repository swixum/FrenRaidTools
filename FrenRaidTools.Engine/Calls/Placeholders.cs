using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FrenRaidTools.Engine;

public static class Placeholders
{
    public sealed record Result(string Text, IReadOnlyList<string> Unresolved)
    {
        public bool Ok => Unresolved.Count == 0;
    }

    public static Result Fill(string template, IReadOnlyDictionary<string, object?> args)
    {
        if (string.IsNullOrEmpty(template) || !template.Contains('{', StringComparison.Ordinal))
            return new Result(template ?? "", []);

        var sb = new StringBuilder(template.Length + 16);
        List<string>? unresolved = null;

        var i = 0;
        while (i < template.Length)
        {
            var open = template.IndexOf('{', i);
            if (open < 0)
            {
                sb.Append(template, i, template.Length - i);
                break;
            }

            var close = template.IndexOf('}', open + 1);
            if (close < 0)
            {
                sb.Append(template, i, template.Length - i);
                (unresolved ??= []).Add(template[open..]);
                break;
            }

            sb.Append(template, i, open - i);

            var expression = template[(open + 1)..close];
            if (TryEvaluate(expression, args, out var value))
                sb.Append(Render(value));
            else
            {
                sb.Append('{').Append(expression).Append('}');
                (unresolved ??= []).Add(expression);
            }

            i = close + 1;
        }

        return new Result(sb.ToString(), (IReadOnlyList<string>?)unresolved ?? []);
    }

    public static string Bare(string text)
    {
        if (!text.Contains('{', StringComparison.Ordinal)) return text;

        var built = new StringBuilder(text.Length);
        var depth = 0;

        foreach (var c in text)
        {
            if (c == '{') { depth++; continue; }
            if (c == '}') { if (depth > 0) depth--; continue; }
            if (depth == 0) built.Append(c);
        }

        return Tidy(built.ToString());
    }

    public static readonly string[] Connectors = ["to", "then", "or", "and", "->", "→", "+", "-", ","];

    public static string Tidy(string text)
    {
        var built = new StringBuilder(text.Length);
        var gap = false;

        foreach (var c in Hollow(text))
        {
            if (char.IsWhiteSpace(c)) { gap = true; continue; }
            if (c == ',' && built.Length > 0 && built[^1] == ',') continue;
            if (gap && built.Length > 0 && c != ',') built.Append(' ');
            gap = false;
            built.Append(c);
        }

        var words = built.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        while (words.Count > 0 && Dangling(words[^1])) words.RemoveAt(words.Count - 1);
        while (words.Count > 0 && Dangling(words[0])) words.RemoveAt(0);

        return string.Join(' ', words).Replace(" ,", ",").Trim(' ', ',');
    }

    private static string Hollow(string text)
    {
        if (!text.Contains('(', StringComparison.Ordinal)) return text;

        var built = new StringBuilder(text.Length);
        var at = 0;

        while (at < text.Length)
        {
            var open = text.IndexOf('(', at);
            if (open < 0) { built.Append(text, at, text.Length - at); break; }

            var close = text.IndexOf(')', open + 1);
            if (close < 0) { built.Append(text, at, text.Length - at); break; }

            built.Append(text, at, open - at);

            var inside = text[(open + 1)..close];
            if (inside.Any(char.IsLetterOrDigit)) built.Append('(').Append(inside).Append(')');

            at = close + 1;
        }

        return built.ToString();
    }

    private static bool Dangling(string word)
    {
        var bare = word.Trim(',');
        return bare.Length == 0 || Connectors.Contains(bare, StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryEvaluate(
        string expression, IReadOnlyDictionary<string, object?> args, out object? value)
    {
        value = null;
        var text = expression.Trim();
        if (text.Length == 0) return false;

        var question = OutsideQuotes(text, '?');
        if (question >= 0)
        {
            var colon = OutsideQuotes(text, ':', question + 1);
            if (colon < 0) return false;

            if (!TryCondition(text[..question], args, out var branch)) return false;
            var taken = branch ? text[(question + 1)..colon] : text[(colon + 1)..];
            return TryEvaluate(taken, args, out value);
        }

        return TryValue(text, args, out value);
    }

    private static bool TryCondition(
        string expression, IReadOnlyDictionary<string, object?> args, out bool result)
    {
        result = false;
        var text = expression.Trim();

        foreach (var op in Comparisons)
        {
            var at = OutsideQuotes(text, op);
            if (at < 0) continue;
            if (!TryValue(text[..at], args, out var left)) return false;
            if (!TryValue(text[(at + op.Length)..], args, out var right)) return false;
            var equal = Equivalent(left, right);
            result = op == "==" ? equal : !equal;
            return true;
        }

        if (!TryValue(text, args, out var single)) return false;
        if (single is null) return false;
        result = Truthy(single);
        return true;
    }

    private static readonly string[] Comparisons = ["==", "!="];

    private static bool TryValue(
        string expression, IReadOnlyDictionary<string, object?> args, out object? value)
    {
        value = null;
        var text = expression.Trim();
        if (text.Length == 0) return false;

        if (text.Length >= 2 && text[0] == '\'' && text[^1] == '\'')
        {
            value = text[1..^1];
            return true;
        }

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var literal))
        {
            value = literal;
            return true;
        }

        var modulo = OutsideQuotes(text, '%');
        if (modulo >= 0)
        {
            if (!TryValue(text[..modulo], args, out var left)) return false;
            if (!TryValue(text[(modulo + 1)..], args, out var right)) return false;
            if (left is null || right is null) return false;
            if (!TryNumber(left, out var l) || !TryNumber(right, out var r) || r == 0) return false;
            value = l % r;
            return true;
        }

        return TryPath(text, args, out value);
    }

    private static bool TryPath(
        string path, IReadOnlyDictionary<string, object?> args, out object? value)
    {
        value = null;

        var parts = path.Split('.', StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return false;
        foreach (var part in parts)
            if (part.Length == 0 || !part.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return false;

        if (!args.TryGetValue(parts[0], out var current)) return false;

        for (var i = 1; i < parts.Length; i++)
        {
            if (current is null) return false;
            if (!TryMember(current, parts[i], out current)) return false;
        }

        value = current;
        return true;
    }

    private static bool TryMember(object target, string name, out object? value)
    {
        value = null;
        var type = target.GetType();

        const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        var property = type.GetProperty(name, Flags);
        if (property is not null && property.GetIndexParameters().Length == 0)
        {
            value = property.GetValue(target);
            return true;
        }

        var field = type.GetField(name, Flags);
        if (field is not null)
        {
            value = field.GetValue(target);
            return true;
        }

        if (target is IReadOnlyDictionary<string, object?> map && map.TryGetValue(name, out var found))
        {
            value = found;
            return true;
        }

        return false;
    }

    private static bool TryNumber(object value, out double number)
    {
        switch (value)
        {
            case double d: number = d; return true;
            case float f: number = f; return true;
            case int i: number = i; return true;
            case long l: number = l; return true;
            case uint u: number = u; return true;
            case short s: number = s; return true;
            case byte b: number = b; return true;
            default:
                return double.TryParse(
                    value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }
    }

    private static bool Equivalent(object? left, object? right)
    {
        if (left is null || right is null) return left is null && right is null;
        if (TryNumber(left, out var l) && TryNumber(right, out var r)) return Math.Abs(l - r) < 1e-9;
        if (left is bool lb && right is bool rb) return lb == rb;
        return string.Equals(Render(left), Render(right), StringComparison.Ordinal);
    }

    private static bool Truthy(object? value) => value switch
    {
        null => false,
        bool b => b,
        string s => s.Length > 0,
        ICollection c => c.Count > 0,
        _ => !TryNumber(value, out var n) || n != 0,
    };

    public static string Render(object? value) => value switch
    {
        null => "",
        string s => s,
        bool b => b ? "true" : "false",
        double d => d == Math.Floor(d)
            ? ((long)d).ToString(CultureInfo.InvariantCulture)
            : d.ToString("0.#", CultureInfo.InvariantCulture),
        float f => Render((double)f),
        IEnumerable<string> strings => string.Join(", ", strings),
        IEnumerable list and not string => string.Join(", ", list.Cast<object?>().Select(Render)),
        _ => value.ToString() ?? "",
    };

    private static int OutsideQuotes(string text, char wanted, int from = 0)
    {
        var quoted = false;
        for (var i = from; i < text.Length; i++)
        {
            if (text[i] == '\'') quoted = !quoted;
            else if (!quoted && text[i] == wanted) return i;
        }
        return -1;
    }

    private static int OutsideQuotes(string text, string wanted)
    {
        var quoted = false;
        for (var i = 0; i + wanted.Length <= text.Length; i++)
        {
            if (text[i] == '\'') { quoted = !quoted; continue; }
            if (!quoted && string.CompareOrdinal(text, i, wanted, 0, wanted.Length) == 0) return i;
        }
        return -1;
    }
}
