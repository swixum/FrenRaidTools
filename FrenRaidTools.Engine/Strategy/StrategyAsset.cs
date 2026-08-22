using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrenRaidTools.Engine;

public sealed record StrategyLink(string Name, string Url);

public sealed record StrategyBlock
{
    private readonly IReadOnlyList<string> _lines = [];

    public int? Step { get; init; }
    public string? Heading { get; init; }
    public string? Label { get; init; }

    public IReadOnlyList<string> Lines
    {
        get => _lines;
        init => _lines = value ?? [];
    }
}

public sealed record StrategyText
{
    private readonly IReadOnlyList<string> _icons = [];
    private readonly IReadOnlyList<StrategyBlock> _blocks = [];

    public required string Text { get; init; }

    public IReadOnlyList<string> Icons
    {
        get => _icons;
        init => _icons = value ?? [];
    }

    public IReadOnlyList<StrategyBlock> Blocks
    {
        get => _blocks;
        init => _blocks = value ?? [];
    }

    public StrategyBlock? Step(int step)
    {
        foreach (var block in Blocks)
            if (block.Step == step) return block;
        return null;
    }
}

public sealed record StrategySeatKey(string Key, string Role, int Party);

public sealed record StrategyMechanic
{
    private static readonly IReadOnlyDictionary<string, double> NoTurns =
        new Dictionary<string, double>();

    private static readonly IReadOnlyDictionary<string, StrategyText> NoSeats =
        new Dictionary<string, StrategyText>();

    private readonly IReadOnlyDictionary<string, double> _rotations = NoTurns;
    private readonly IReadOnlyDictionary<string, StrategyText> _seats = NoSeats;

    public required string Name { get; init; }
    public StrategyText? Description { get; init; }
    public StrategyText? Action { get; init; }
    public StrategyText? Notes { get; init; }

    public IReadOnlyDictionary<string, double> Rotations
    {
        get => _rotations;
        init => _rotations = value ?? NoTurns;
    }

    public IReadOnlyDictionary<string, StrategyText> Seats
    {
        get => _seats;
        init => _seats = value ?? NoSeats;
    }

    public double RotationFor(string alignment) =>
        Rotations.TryGetValue(alignment, out var degrees) ? degrees : 0;
}

public sealed record StrategyPhase
{
    private readonly IReadOnlyList<StrategyMechanic> _mechanics = [];

    public required string Name { get; init; }
    public string? Tag { get; init; }
    public string? OptionKey { get; init; }
    public string? OptionValue { get; init; }
    public StrategyText? Description { get; init; }

    public IReadOnlyList<StrategyMechanic> Mechanics
    {
        get => _mechanics;
        init => _mechanics = value ?? [];
    }
}

public sealed record StrategyChoice
{
    private readonly IReadOnlyList<StrategyLink> _links = [];

    public required string Value { get; init; }
    public required string Label { get; init; }
    public string? Difference { get; init; }

    public IReadOnlyList<StrategyLink> Links
    {
        get => _links;
        init => _links = value ?? [];
    }
}

public sealed record StrategyOption
{
    private readonly IReadOnlyList<StrategyChoice> _options = [];

    public required string Key { get; init; }
    public required string Label { get; init; }
    public string? DefaultValue { get; init; }

    public IReadOnlyList<StrategyChoice> Options
    {
        get => _options;
        init => _options = value ?? [];
    }

    public StrategyChoice? Choice(string? value)
    {
        if (value is null) return null;
        foreach (var choice in Options)
            if (choice.Value == value) return choice;
        return null;
    }
}

public sealed record StrategyAlignment(string Value, string Label);

public sealed record StrategyPlanInfo
{
    private readonly IReadOnlyList<StrategyLink> _links = [];

    public required string Name { get; init; }
    public required string Label { get; init; }

    public IReadOnlyList<StrategyLink> Links
    {
        get => _links;
        init => _links = value ?? [];
    }
}

public sealed record StrategyAsset
{
    private readonly IReadOnlyList<StrategySeatKey> _seats = [];
    private readonly IReadOnlyList<StrategyAlignment> _alignments = [];
    private readonly IReadOnlyList<StrategyOption> _options = [];
    private readonly IReadOnlyList<StrategyPhase> _phases = [];

    public required string Licence { get; init; }
    public required string FightKey { get; init; }
    public required string Title { get; init; }
    public string Subtitle { get; init; } = "";
    public required StrategyPlanInfo Plan { get; init; }

    public IReadOnlyList<StrategySeatKey> Seats
    {
        get => _seats;
        init => _seats = value ?? [];
    }

    public IReadOnlyList<StrategyAlignment> Alignments
    {
        get => _alignments;
        init => _alignments = value ?? [];
    }

    public IReadOnlyList<StrategyOption> Options
    {
        get => _options;
        init => _options = value ?? [];
    }

    public IReadOnlyList<StrategyPhase> Phases
    {
        get => _phases;
        init => _phases = value ?? [];
    }

    public static StrategyAsset Load(PlannedFight fight)
    {
        var asset = Load(fight.Resource);
        if (asset.FightKey != fight.Key)
            throw new InvalidDataException(
                $"{fight.Resource} holds the plan for {asset.FightKey}, not {fight.Key}.");
        return asset;
    }

    private static readonly JsonSerializerOptions Format = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static StrategyAsset Parse(string json) =>
        JsonSerializer.Deserialize<StrategyAsset>(json, Format)
        ?? throw new InvalidDataException("The strategy asset is empty.");

    public static StrategyAsset Load(string resourceName)
    {
        var assembly = typeof(StrategyAsset).GetTypeInfo().Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"No embedded strategy asset {resourceName}.");
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }
}
