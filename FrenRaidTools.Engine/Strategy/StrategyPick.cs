namespace FrenRaidTools.Engine;

public sealed class StrategyPick
{
    public const string BossNorth = "original";

    public Dictionary<string, string> Options { get; set; } = new(StringComparer.Ordinal);

    public string Alignment { get; set; } = BossNorth;

    public string Seat { get; set; } = "";

    public bool Enabled { get; set; }

    public string? Value(string key) => Options.TryGetValue(key, out var value) ? value : null;

    public void Set(string key, string? value)
    {
        if (value is null) Options.Remove(key);
        else Options[key] = value;
    }

    public StrategyPick Copy() => new()
    {
        Options = new Dictionary<string, string>(Options, StringComparer.Ordinal),
        Alignment = Alignment,
        Seat = Seat,
        Enabled = Enabled,
    };
}
