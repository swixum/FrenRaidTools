namespace FrenRaidTools.Engine;

public static class PlayerName
{
    public static string First(string full)
    {
        if (string.IsNullOrWhiteSpace(full)) return "";

        var trimmed = full.AsSpan().Trim();
        var space = trimmed.IndexOf(' ');
        return space < 0 ? trimmed.ToString() : trimmed[..space].ToString();
    }
}
