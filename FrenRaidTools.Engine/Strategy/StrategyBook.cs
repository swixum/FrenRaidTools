using System.Text;

namespace FrenRaidTools.Engine;

public sealed record StrategyCue(string Display, string Speech)
{
    public static readonly StrategyCue None = new("", "");

    public bool Empty => Display.Length == 0;
}

public sealed class StrategyBook
{
    private readonly StrategyAsset _asset;
    private readonly StrategyPick _pick;

    public StrategyBook(StrategyAsset asset, StrategyPick pick)
    {
        _asset = asset;
        _pick = pick;
    }

    public StrategyAsset Asset => _asset;

    public StrategyPick Pick => _pick;

    public bool Ready => SeatKnown;

    public bool SeatKnown => SeatOf(_pick.Seat) is not null;

    public StrategySeatKey? SeatOf(string key)
    {
        foreach (var seat in _asset.Seats)
            if (seat.Key == key) return seat;
        return null;
    }

    public string ChosenOrDefault(string key)
    {
        var option = OptionOf(key);
        if (option is null) return _pick.Value(key) ?? "";

        return option.Choice(_pick.Value(key))?.Value ?? option.DefaultValue ?? "";
    }

    public StrategyOption? OptionOf(string key)
    {
        foreach (var option in _asset.Options)
            if (option.Key == key) return option;
        return null;
    }

    public bool Active(StrategyPhase phase) =>
        phase.OptionKey is null || ChosenOrDefault(phase.OptionKey) == phase.OptionValue;

    public IEnumerable<StrategyPhase> Phases()
    {
        foreach (var phase in _asset.Phases)
            if (Active(phase)) yield return phase;
    }

    public StrategyPhase? Phase(string name)
    {
        foreach (var phase in Phases())
            if (phase.Name == name) return phase;
        return null;
    }

    public StrategyMechanic? Mechanic(string phaseName, string mechanicName)
    {
        var phase = Phase(phaseName);
        if (phase is null) return null;
        foreach (var mechanic in phase.Mechanics)
            if (mechanic.Name == mechanicName) return mechanic;
        return null;
    }

    public StrategyCue Say(string phaseName, string mechanicName, int? step = null)
    {
        var mechanic = Mechanic(phaseName, mechanicName);
        return mechanic is null ? StrategyCue.None : Say(mechanic, step);
    }

    public StrategyCue Spot(
        string callKey, IReadOnlyDictionary<string, object?>? args = null, int? step = null)
    {
        if (!Ready) return StrategyCue.None;

        var spot = StrategySpots.For(_asset.FightKey, callKey);
        if (spot is null) return StrategyCue.None;

        if (spot.Branch is { } branch)
        {
            var taken = Flag(args, branch.ParamKey);
            if (taken is null) return StrategyCue.None;

            var chosen = Mechanic(spot.Phase, taken.Value ? branch.WhenTrue : branch.WhenFalse);
            return chosen is null ? StrategyCue.None : Say(chosen, step);
        }

        foreach (var name in spot.Mechanics)
        {
            var mechanic = Mechanic(spot.Phase, name);
            if (mechanic is not null) return Say(mechanic, step);
        }

        return StrategyCue.None;
    }

    private static bool? Flag(IReadOnlyDictionary<string, object?>? args, string key) =>
        args is not null && args.TryGetValue(key, out var value) && value is bool flag ? flag : null;

    public StrategyCue Say(StrategyMechanic mechanic, int? step = null, string? fluid = null)
    {
        if (!Ready) return StrategyCue.None;

        if (mechanic.Seats.Count == 0)
        {
            var shared = mechanic.Action is { Text.Length: > 0 } line
                ? TextLines.Of(line.Text)
                : [];

            return shared.Count == 1
                ? Read(shared, mechanic.RotationFor(_pick.Alignment), fluid)
                : StrategyCue.None;
        }

        if (!mechanic.Seats.TryGetValue(_pick.Seat, out var seat)) return StrategyCue.None;

        var lines = step is null
            ? TextLines.Of(seat.Text)
            : seat.Step(step.Value)?.Lines ?? [];

        return Read(lines, mechanic.RotationFor(_pick.Alignment), fluid);
    }

    public StrategyCue Tell(StrategyMechanic mechanic)
    {
        var turn = mechanic.RotationFor(_pick.Alignment);
        var lines = new List<string>();
        if (mechanic.Action is { } action) lines.AddRange(TextLines.Of(action.Text));
        if (mechanic.Description is { } description) lines.AddRange(TextLines.Of(description.Text));
        return Read(lines, turn);
    }

    public string Aligned(string text, StrategyMechanic mechanic) =>
        Compass.Rotate(text, mechanic.RotationFor(_pick.Alignment));

    private StrategyCue Read(IReadOnlyList<string> lines, double turn, string? fluid = null)
    {
        if (lines.Count == 0) return StrategyCue.None;

        var display = new StringBuilder();
        var speech = new StringBuilder();

        foreach (var line in lines)
        {
            var settled = PlanStep.Settled(line, fluid);
            if (string.IsNullOrEmpty(settled)) continue;

            settled = PlanStep.SettleDepth(settled, _pick.Seat);
            var turned = Compass.Rotate(settled, turn);
            if (display.Length > 0) display.Append('\n');
            display.Append(turned);

            var spoken = Spoken(turned);
            if (spoken.Length == 0) continue;
            if (speech.Length > 0) speech.Append(". ");
            speech.Append(spoken);
        }

        return new StrategyCue(display.ToString(), speech.ToString());
    }

    private static string Spoken(string line) => SpeechText.Of(line);
}
