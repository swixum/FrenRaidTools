namespace FrenRaidTools.Engine;

public sealed record CallChoice(string Label, IReadOnlyList<string> Keys);

public sealed record CallVoice(string Label, IReadOnlyList<CallChoice> Choices)
{
    public IEnumerable<string> Keys => Choices.SelectMany(c => c.Keys).Distinct(StringComparer.Ordinal);

    public bool Covers(string key) => Keys.Contains(key, StringComparer.Ordinal);

    public CallChoice? Matching(Func<string, bool> on)
    {
        foreach (var choice in Choices)
            if (Keys.All(key => on(key) == choice.Keys.Contains(key, StringComparer.Ordinal)))
                return choice;

        return null;
    }
}

public static class CallVoices
{
    private static readonly string[] Raidwides =
        ["grandCrossRaidwide", "infernoRaidwide", "tsunamiRaidwide"];

    private static readonly string[] Tells =
        ["grandCross1", "grandCross2", "grandCross3", "inferno1", "inferno2", "tsunami1", "tsunami2"];

    public static readonly CallVoice GrandCross = new("Say",
    [
        new CallChoice("Raidwides", Raidwides),
        new CallChoice("Real and fake", Tells),
        new CallChoice("Both", [.. Raidwides, .. Tells]),
    ]);

    public static readonly IReadOnlyList<CallVoice> All = [GrandCross];

    public static CallVoice? For(IEnumerable<string> keys)
    {
        foreach (var key in keys)
            foreach (var voice in All)
                if (voice.Covers(key)) return voice;

        return null;
    }
}
