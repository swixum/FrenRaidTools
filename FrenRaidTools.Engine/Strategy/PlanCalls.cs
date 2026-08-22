namespace FrenRaidTools.Engine;

public sealed class PlanCalls
{
    public const string KeyPrefix = "plan";
    public const string SpotParam = "spot";
    public const string SpotSpeechParam = "spotSpeech";

    private readonly string _fightKey;
    private readonly IReadOnlyList<PlanAnchor> _anchors;
    private readonly Dictionary<string, List<PlanAnchor>> _byCall;
    private readonly Func<StrategyBook?> _book;
    private readonly Dictionary<string, Callout> _calls = new(StringComparer.Ordinal);

    public PlanCalls(string fightKey, Func<StrategyBook?> book)
    {
        _fightKey = fightKey;
        _book = book;
        _anchors = PlanAnchors.For(fightKey);
        _byCall = new Dictionary<string, List<PlanAnchor>>(StringComparer.Ordinal);

        foreach (var anchor in _anchors)
        {
            if (!_byCall.TryGetValue(anchor.RideCall, out var list))
                _byCall[anchor.RideCall] = list = [];
            list.Add(anchor);
        }
    }

    public IReadOnlyList<PlanAnchor> Anchors => _anchors;

    public int Count => _calls.Count;

    public static string KeyFor(string fightKey, string phase, string mechanic) =>
        $"{KeyPrefix}:{fightKey}:{phase}:{mechanic}";

    public static Callout For(string phase, string mechanic) =>
        Callout.Of(mechanic, $"{{{SpotSpeechParam}}}", $"{{{SpotParam}}}").Planned();

    public void Register(CalloutCatalog catalog, StrategyAsset asset)
    {
        _calls.Clear();

        foreach (var anchor in _anchors)
        {
            if (anchor.Replaces) continue;
            if (anchor.FromAction && !PlanAnchors.Reachable(anchor, asset)) continue;

            foreach (var mechanic in Registered(anchor))
            {
                if (!anchor.FromAction
                    && !PlanAnchors.AnySeatText(asset, anchor.Phase, mechanic)
                    && !PlanAnchors.AnyActionText(asset, anchor.Phase, mechanic)) continue;

                var key = KeyFor(_fightKey, anchor.Phase, mechanic);
                if (_calls.ContainsKey(key)) continue;

                var call = catalog.Find(key)?.Call ?? catalog.Add(
                    anchor.Phase, key, For(anchor.Phase, mechanic),
                    PlanAnchors.PhaseFor(asset, anchor.Phase), anchor.Phase);

                _calls[key] = call;
            }
        }
    }

    public static IReadOnlyList<string> Registered(PlanAnchor anchor) =>
        anchor.FromAction ? [anchor.Named] : anchor.Mechanics;

    public sealed record Ready(Callout Call, Dictionary<string, object?> Args, bool Replaces)
    {
        public bool Repeat { get; init; }
    }

    private readonly Dictionary<string, int> _fired = new(StringComparer.Ordinal);

    private string _said = "";

    public static string CueOf(PlanAnchor anchor, int? step, string display) =>
        $"{anchor.Phase}|{step?.ToString() ?? "-"}|{display}";

    public bool Replaced(string callKey) =>
        callKey.Length > 0 &&
        _byCall.TryGetValue(callKey, out var anchors) &&
        anchors.Any(a => a.Replaces);

    public bool Starred(Callout call) => call.FromPlan || Replaced(call.Key);

    public int FiredCount(string callKey) => _fired.GetValueOrDefault(callKey);

    public void Reset()
    {
        _fired.Clear();
        _said = "";
    }

    public Ready? Riding(string callKey, IReadOnlyDictionary<string, object?>? args)
    {
        if (callKey.Length == 0) return null;
        if (!_byCall.TryGetValue(callKey, out var anchors)) return null;

        var invocation = _fired.GetValueOrDefault(callKey);
        _fired[callKey] = invocation + 1;

        foreach (var anchor in anchors)
        {
            if (anchor.Wildcard || anchor.Invocation != invocation) continue;
            var ready = Resolve(anchor, args);
            if (ready is not null) return ready;
        }

        foreach (var anchor in anchors)
        {
            if (!anchor.Wildcard) continue;
            var ready = Resolve(anchor, args);
            if (ready is not null) return ready;
        }

        return null;
    }

    public Ready? Resolve(PlanAnchor anchor, IReadOnlyDictionary<string, object?>? args)
    {
        var book = _book();
        if (book is null || !book.Ready) return null;

        var name = Chosen(anchor, book, args);
        if (name is null) return null;

        var mechanic = book.Mechanic(anchor.Phase, name);
        if (mechanic is null) return null;

        var step = Step(anchor, args);
        if (anchor.StepParam.Length > 0 && step is null) return null;

        var cue = anchor.FromAction
            ? Timeline(book, mechanic, step, args)
            : step is null
                ? book.Say(mechanic, null, Text(args, PlanStep.FluidParam))
                : Direction(book, mechanic, step.Value, args, anchor.Bait);

        if (cue.Empty) return null;

        Callout call;
        var keyName = anchor.FromAction ? anchor.Named : name;
        if (anchor.Replaces)
            call = For(anchor.Phase, keyName) with { Key = KeyFor(_fightKey, anchor.Phase, keyName) };
        else if (!_calls.TryGetValue(KeyFor(_fightKey, anchor.Phase, keyName), out call!)) return null;

        var carried = args is null
            ? []
            : new Dictionary<string, object?>(args, StringComparer.Ordinal);

        carried[SpotParam] = cue.Display.Replace("\n", ", ");
        carried[SpotSpeechParam] = cue.Speech;

        var cued = CueOf(anchor, step, cue.Display);
        var repeat = cued == _said;
        _said = cued;

        return new Ready(call, carried, anchor.Replaces) { Repeat = repeat };
    }

    public const string PlaceParam = "myLine";
    public const string GroupParam = "myGroup";
    public const string SpotsParam = "tetherSpots";
    public const string PairParam = "tetherPair";

    private static StrategyCue Timeline(
        StrategyBook book, StrategyMechanic mechanic, int? step,
        IReadOnlyDictionary<string, object?>? args)
    {
        if (step is null) return StrategyCue.None;

        var timeline = TextLines.Of(mechanic.Action?.Text);
        var rules = TextLines.Of(mechanic.Description?.Text);

        var lines = PlanTether.Lines(
            timeline, rules, step.Value,
            Text(args, PlaceParam), Text(args, GroupParam),
            Names(args, SpotsParam), Names(args, PairParam));

        return PlanStep.Read(lines, line => book.Aligned(line, mechanic));
    }

    private static IReadOnlyList<string>? Names(
        IReadOnlyDictionary<string, object?>? args, string key)
    {
        if (args is null || !args.TryGetValue(key, out var value)) return null;
        return value switch
        {
            IReadOnlyList<string> names => names,
            IEnumerable<string> names => [.. names],
            _ => null,
        };
    }

    public const string MineParam = "myMech";
    public const string FutureParam = "future";
    public const string TowerParam = "myTower";
    public const string RoleParam = "myRole";
    public const string NextParam = "myNextMech";

    private static StrategyCue Direction(
        StrategyBook book, StrategyMechanic mechanic, int step,
        IReadOnlyDictionary<string, object?>? args, bool bait)
    {
        if (!mechanic.Seats.TryGetValue(book.Pick.Seat, out var seat)) return StrategyCue.None;

        var block = seat.Step(step);
        if (block is null) return StrategyCue.None;

        var lines = bait
            ? PlanStep.BaitOnly(block, Flag(args, FutureParam))
            : PlanStep.Lines(
                block, Text(args, MineParam), Flag(args, FutureParam),
                Text(args, TowerParam), Text(args, RoleParam), Text(args, PlanStep.FluidParam),
                baitsClones: Flag(args, PlanStep.LastTowerParam) != true,
                baits: false,
                ownBait: PlanStep.OwnBait(seat, Text(args, RoleParam)),
                next: Text(args, NextParam),
                partner: PlanStep.SharesWith(block, Mate(mechanic, book.Pick.Seat, step)));

        return PlanStep.Read(
            lines, line => book.Aligned(line, mechanic),
            bait ? null : Text(args, MineParam));
    }

    private static StrategyBlock? Mate(StrategyMechanic mechanic, string seat, int step)
    {
        var mate = Slots.PartnerSlot(seat);
        if (mate.Length == 0) return null;

        return mechanic.Seats.TryGetValue(mate, out var text) ? text.Step(step) : null;
    }

    private static string? Text(IReadOnlyDictionary<string, object?>? args, string key) =>
        args is not null && args.TryGetValue(key, out var value) ? value?.ToString() : null;

    private static bool? Flag(IReadOnlyDictionary<string, object?>? args, string key) =>
        args is not null && args.TryGetValue(key, out var value) && value is bool flag ? flag : null;

    private static int? Step(PlanAnchor anchor, IReadOnlyDictionary<string, object?>? args)
    {
        if (anchor.StepParam.Length == 0) return null;
        if (args is null) return null;
        if (!args.TryGetValue(anchor.StepParam, out var raw)) return null;
        if (raw is not int step) return null;
        if (!anchor.Bait) return step;
        return step > 1 ? step - 1 : null;
    }

    private static string? Chosen(
        PlanAnchor anchor, StrategyBook book, IReadOnlyDictionary<string, object?>? args)
    {
        if (anchor.Branch is { } branch)
        {
            if (args is null) return null;
            if (!args.TryGetValue(branch.ParamKey, out var value)) return null;
            if (value is not bool taken) return null;
            return taken ? branch.WhenTrue : branch.WhenFalse;
        }

        foreach (var mechanic in anchor.Mechanics)
            if (book.Mechanic(anchor.Phase, mechanic) is not null) return mechanic;

        return null;
    }

    public CallSink Wrap(CallSink inner) =>
        (callout, on, args) => Deliver(callout, on, args, inner);

    public void Deliver(
        Callout callout, GameEvent? on, IReadOnlyDictionary<string, object?> args, CallSink inner)
    {
        if (callout.FromPlan)
        {
            inner(callout, on, args);
            return;
        }

        var riding = Riding(callout.Key, args);

        if (riding is { Replaces: true })
        {
            inner(Spoken(callout), on, riding.Args);
            return;
        }

        inner(callout, on, args);

        if (riding is { Repeat: false }) inner(riding.Call, on, riding.Args);
    }

    public static Callout Spoken(Callout call) =>
        call.Text.Contains($"{{{SpotParam}}}", StringComparison.Ordinal)
            ? call
            : call with { Text = $"{{{SpotParam}}}", Speech = $"{{{SpotSpeechParam}}}" };
}
