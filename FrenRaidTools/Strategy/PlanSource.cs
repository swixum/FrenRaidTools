using FrenRaidTools.Engine;

namespace FrenRaidTools;

public static class PlanSource
{
    private sealed class Loaded
    {
        public StrategyAsset? Asset;
        public string Fault = "";
        public Task? Reading;
        public StrategyBook? Book;
        public StrategyPick? BoundTo;
    }

    private static readonly Dictionary<string, Loaded> Fights = new(StringComparer.Ordinal);

    public const string SpotSeparator = "  ·  ";

    public static StrategyAsset? Asset(PlannedFight fight)
    {
        var entry = Entry(fight);
        if (entry.Asset is not null) return entry.Asset;
        entry.Reading ??= Task.Run(() => Read(fight, entry));
        return entry.Asset;
    }

    public static string Fault(PlannedFight fight) => Entry(fight).Fault;

    public static bool Loading(PlannedFight fight)
    {
        var entry = Entry(fight);
        return entry.Asset is null && entry.Fault.Length == 0;
    }

    public static StrategyBook? Book(PlannedFight fight, StrategyPick pick)
    {
        var asset = Asset(fight);
        if (asset is null) return null;

        var entry = Entry(fight);
        if (entry.Book is not null && ReferenceEquals(entry.BoundTo, pick)) return entry.Book;
        entry.BoundTo = pick;
        entry.Book = new StrategyBook(asset, pick);
        return entry.Book;
    }

    public static void Warm(PlannedFight fight) => Asset(fight);

    public static PlannedFight? LiveFight(Configuration config) =>
        FightPlans.InZone(Game.Zone) ?? FightPlans.ByKey(config.PlanFight);

    public static (string Text, string Speech) WithSpot(
        Configuration config, string callKey, IReadOnlyDictionary<string, object?>? args,
        string text, string speech)
    {
        var fight = LiveFight(config);
        if (fight is null) return (text, speech);

        var book = Book(fight, config.PlanFor(fight.Key));
        var spot = book?.Spot(callKey, args) ?? StrategyCue.None;
        if (spot.Empty) return (text, speech);

        var onOneLine = spot.Display.Replace("\n", ", ");
        return (
            text.Length > 0 ? text + SpotSeparator + onOneLine : onOneLine,
            speech.Length > 0 ? speech + ". " + spot.Speech : spot.Speech);
    }

    private static Loaded Entry(PlannedFight fight)
    {
        if (Fights.TryGetValue(fight.Key, out var entry)) return entry;
        entry = new Loaded();
        Fights[fight.Key] = entry;
        return entry;
    }

    private static void Read(PlannedFight fight, Loaded entry)
    {
        try
        {
            entry.Asset = StrategyAsset.Load(fight);
        }
        catch (Exception ex)
        {
            entry.Fault = ex.Message;
            Service.Log.Error(ex, $"The plan for {fight.Name} would not load.");
        }
    }
}
