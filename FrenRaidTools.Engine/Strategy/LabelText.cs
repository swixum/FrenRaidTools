namespace FrenRaidTools.Engine;

public static class LabelText
{
    private static readonly (string From, string To)[] Shorthand =
    [
        ("P1 ", "Phase 1 "),
        ("P2 ", "Phase 2 "),
        ("P3 ", "Phase 3 "),
        ("P4 ", "Phase 4 "),
        ("P5 ", "Phase 5 "),
        ("/LC", " and Limit Cut"),
        ("LC ", "Limit Cut "),
    ];

    public static string Of(string label)
    {
        foreach (var (from, to) in Shorthand)
            label = label.Replace(from, to, StringComparison.Ordinal);

        return label;
    }

    public static string Under(string section, string label)
    {
        if (section.Length == 0) return label;
        if (!label.StartsWith(section, StringComparison.Ordinal)) return label;
        if (label.Length <= section.Length + 1) return label;
        if (label[section.Length] != ':') return label;

        var rest = label[(section.Length + 1)..].TrimStart();
        return rest.Length == 0 ? label : rest;
    }
}
