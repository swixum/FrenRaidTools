namespace FrenRaidTools.Feed;

public static class HeadMarkers
{
    public const string Lead = FrenRaidTools.Engine.VfxTells.LockonLead;

    private static Dictionary<string, uint>? _byIcon;

    public static int Count => _byIcon?.Count ?? 0;

    public static string? Fault { get; private set; }

    public static void Forget() => _byIcon = null;

    public static uint? For(string path)
    {
        var icon = FrenRaidTools.Engine.VfxTells.Icon(path);
        if (icon is null) return null;

        var map = Map();
        return map is not null && map.TryGetValue(icon, out var id) ? id : null;
    }

    private static Dictionary<string, uint>? Map()
    {
        if (_byIcon is not null) return _byIcon;
        if (Fault is not null) return null;

        try
        {
            var sheet = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Lockon>();
            if (sheet is null)
            {
                Fault = "The head marker table would not load.";
                return null;
            }

            var built = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
            var shared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in sheet)
            {
                var icon = row.IconName.ExtractText();
                if (string.IsNullOrWhiteSpace(icon)) continue;

                if (!built.TryAdd(icon, row.RowId)) shared.Add(icon);
            }

            foreach (var icon in shared) built.Remove(icon);

            _byIcon = built;
            return _byIcon;
        }
        catch (Exception ex)
        {
            Fault = $"The head marker table would not load: {ex.Message}";
            Service.Log.Error(ex, "Could not read the lockon sheet.");
            return null;
        }
    }
}
