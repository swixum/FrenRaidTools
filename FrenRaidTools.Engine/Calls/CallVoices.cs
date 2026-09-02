namespace FrenRaidTools.Engine;

public sealed record CallChoice(string Label, Func<Callout, bool> Wants);

public sealed record CallVoice(string Label, int Phase, Func<Callout, bool> Covers,
    IReadOnlyList<CallChoice> Choices)
{
    public IEnumerable<CatalogEntry> Universe(IEnumerable<CatalogEntry> calls) =>
        calls.Where(e => e.Call.Phase == Phase && !e.Call.Fightwide && Covers(e.Call));

    public CallChoice? Matching(IEnumerable<CatalogEntry> calls, Func<string, bool> on)
    {
        var universe = Universe(calls).ToList();
        if (universe.Count == 0) return null;

        foreach (var choice in Choices)
            if (universe.All(e => on(e.Key) == choice.Wants(e.Call)))
                return choice;

        return null;
    }
}

public static class CallVoices
{
    public const string Raidwides = "Raidwides";

    private static readonly string[] GrandCrossSteps = ["Crosses", "Inferno", "Tsunami"];

    private static readonly string[] AppliedSteps =
        ["First debuff set applied", "Second debuff set applied"];

    private static bool IsRaidwide(Callout call) => call.Step == Raidwides;

    private static bool InPhaseFour(Callout call) =>
        IsRaidwide(call)
        || GrandCrossSteps.Contains(call.Step, StringComparer.Ordinal)
        || AppliedSteps.Contains(call.Step, StringComparer.Ordinal);

    public static readonly CallVoice PhaseFour = new("Say", 4, InPhaseFour,
    [
        new CallChoice(Raidwides, IsRaidwide),
        new CallChoice("Real and fake", call => call.OnByDefault && !IsRaidwide(call)),
        new CallChoice("Both", call => call.OnByDefault),
    ]);

    public static readonly IReadOnlyList<CallVoice> All = [PhaseFour];

    public static CallVoice? ForPhase(int phase)
    {
        foreach (var voice in All)
            if (voice.Phase == phase) return voice;

        return null;
    }
}
