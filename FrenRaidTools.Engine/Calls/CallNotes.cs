namespace FrenRaidTools.Engine;

public static class CallNotes
{
    private static readonly string[] Internal =
    [
        "This one varies",
        "Before the reference wording:",
        "Answered from the debuff you are holding.",
    ];

    public static IReadOnlyList<string> Lines(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return [];

        var kept = new List<string>();

        foreach (var raw in notes.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (Internal.Any(head => line.StartsWith(head, StringComparison.Ordinal))) continue;
            kept.Add(line);
        }

        return kept;
    }
}
