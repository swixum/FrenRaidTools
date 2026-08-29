namespace FrenRaidTools.Engine.DancingMad;

public static class ArrowSpots
{
    public const string OptionKey = "arrows";
    public const string BigBox = "mgr";
    public const string Param = "arrowSpots";
    public const string SpeechParam = "arrowSpotsSpeech";
    public const string Join = ", then ";

    private static readonly Dictionary<string, (string First, string Second)> BigBoxRing =
        new(StringComparer.Ordinal)
        {
            ["NN"] = ("WSW", "W"),
            ["NE"] = ("WNW", "NW"),
            ["NW"] = ("SW", "SSW"),
            ["EE"] = ("NNW", "N"),
            ["EN"] = ("NW", "WNW"),
            ["ES"] = ("NNE", "NE"),
            ["SS"] = ("ENE", "E"),
            ["SE"] = ("NE", "NNE"),
            ["SW"] = ("ESE", "SE"),
            ["WW"] = ("SSE", "S"),
            ["WN"] = ("SSW", "SW"),
            ["WS"] = ("SE", "ESE"),
        };

    private static readonly Dictionary<string, string> Names =
        new(StringComparer.Ordinal)
        {
            ["N"] = "A",
            ["NNE"] = "Right of A",
            ["NE"] = "Top Right",
            ["ENE"] = "Above B",
            ["E"] = "B",
            ["ESE"] = "Below B",
            ["SE"] = "Bottom Right",
            ["SSE"] = "Right of C",
            ["S"] = "C",
            ["SSW"] = "Left of C",
            ["SW"] = "Bottom Left",
            ["WSW"] = "Below D",
            ["W"] = "D",
            ["WNW"] = "Above D",
            ["NW"] = "Top Left",
            ["NNW"] = "Left of A",
        };

    public static (string First, string Second)? Ring(string? option, string pair) =>
        option == BigBox && BigBoxRing.TryGetValue(pair, out var spots) ? spots : null;

    public static string Name(string spot) =>
        Names.TryGetValue(spot, out var named) ? named : spot;

    public static string? Text(string? option, string pair) =>
        Ring(option, pair) is { } spots ? Name(spots.First) + Join + Name(spots.Second) : null;

    public static string? Speech(string? option, string pair) => Text(option, pair);
}
