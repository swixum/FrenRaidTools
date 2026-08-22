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

    public static (string First, string Second)? Ring(string? option, string pair) =>
        option == BigBox && BigBoxRing.TryGetValue(pair, out var spots) ? spots : null;

    public static string? Text(string? option, string pair) =>
        Ring(option, pair) is { } spots ? spots.First + Join + spots.Second : null;

    public static string? Speech(string? option, string pair) =>
        Ring(option, pair) is { } spots
            ? SpeechText.Of(spots.First) + Join + SpeechText.Of(spots.Second)
            : null;
}
