namespace FrenRaidTools.Engine;

public static class TextLines
{
    public static IReadOnlyList<string> Of(string? text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var lines = new List<string>();
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0) lines.Add(trimmed);
        }

        return lines;
    }

    public static string Spots(int count) => count == 1 ? "1 spot" : $"{count} spots";
}
