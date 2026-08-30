using System.Text.Json;

namespace FrenRaidTools.Engine;

public readonly record struct WaymarkPoint(string Name, double X, double Y);

public sealed record WaymarkPreset
{
    private readonly IReadOnlyList<WaymarkPoint> _marks = [];

    public static readonly WaymarkPreset None = new();

    public double CenterX { get; init; }

    public double CenterY { get; init; }

    public double CenterRadius { get; init; } = 1.0;

    public IReadOnlyList<WaymarkPoint> Marks
    {
        get => _marks;
        init => _marks = value ?? [];
    }

    public bool Any => _marks.Count > 0;

    public ArenaSector Sector(string mark)
    {
        foreach (var point in _marks)
            if (string.Equals(point.Name, mark, StringComparison.OrdinalIgnoreCase))
                return SectorOf(point);
        return ArenaSector.Unknown;
    }

    public string? Mark(ArenaSector sector)
    {
        if (!sector.IsPoint()) return null;
        foreach (var point in _marks)
            if (SectorOf(point) == sector)
                return point.Name;
        return null;
    }

    private ArenaSector SectorOf(WaymarkPoint point)
    {
        var dx = point.X - CenterX;
        var dy = point.Y - CenterY;
        if (Math.Sqrt(dx * dx + dy * dy) <= CenterRadius) return ArenaSector.Center;
        var degrees = Math.Atan2(dx, -dy) * 180.0 / Math.PI;
        if (degrees < 0) degrees += 360.0;
        return ArenaSectors.Wrap((int)Math.Round(degrees / 45.0));
    }
}

public static class WaymarkPresets
{
    private static readonly Dictionary<string, WaymarkPreset> Loaded = new(StringComparer.Ordinal);

    public static WaymarkPreset For(string fightKey)
    {
        lock (Loaded)
        {
            if (Loaded.TryGetValue(fightKey, out var known)) return known;
            var built = Read(fightKey);
            Loaded[fightKey] = built;
            return built;
        }
    }

    public static WaymarkPreset Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return WaymarkPreset.None;
            if (!root.TryGetProperty("center", out var center)) return WaymarkPreset.None;
            if (!root.TryGetProperty("preset", out var preset)) return WaymarkPreset.None;
            var marks = new List<WaymarkPoint>();
            foreach (var property in preset.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Object) continue;
                if (!property.Value.TryGetProperty("Active", out var active)) continue;
                if (active.ValueKind != JsonValueKind.True) continue;
                if (!property.Value.TryGetProperty("X", out var x)) continue;
                if (!property.Value.TryGetProperty("Z", out var z)) continue;
                marks.Add(new WaymarkPoint(property.Name, x.GetDouble(), z.GetDouble()));
            }
            return new WaymarkPreset
            {
                CenterX = center.GetProperty("x").GetDouble(),
                CenterY = center.GetProperty("y").GetDouble(),
                Marks = marks,
            };
        }
        catch (JsonException)
        {
            return WaymarkPreset.None;
        }
    }

    private static WaymarkPreset Read(string fightKey)
    {
        var fight = FightPlans.ByKey(fightKey);
        if (fight is null) return WaymarkPreset.None;
        using var stream = typeof(WaymarkPreset).Assembly
            .GetManifestResourceStream(fight.MarksResource);
        if (stream is null) return WaymarkPreset.None;
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }
}
