namespace FrenRaidTools.Engine;

public static class DebuffCalls
{
    public const string TextParam = "seat";
    public const string SpeechParam = "seatSpeech";

    public const double Tolerance = 2.0;

    public sealed record Rule(uint Status, double Seconds, string Text, string Speech)
    {
        public uint With { get; init; }

        public double WithSeconds { get; init; }

        public bool Absent { get; init; }

        public bool? Support { get; init; }
    }

    private static bool Holds(IReadOnlyList<GameEvent> mine, uint status, double seconds) =>
        mine.Any(s => s.Id == status
                      && (seconds <= 0 || Math.Abs(s.Duration - seconds) <= Tolerance));

    public static Rule? Match(IWorld world, IReadOnlyList<Rule> rules)
    {
        var you = world.You;
        if (you is null) return null;

        var mine = world.ActiveStatuses()
            .Where(s => s.Target is not null && s.Target.ObjectId == you.ObjectId)
            .ToList();

        bool Fits(Rule rule) => rule.Support is null || rule.Support == you.Support;

        foreach (var rule in rules)
        {
            if (rule.Absent || rule.Status == 0) continue;
            if (!Fits(rule)) continue;
            if (!Holds(mine, rule.Status, rule.Seconds)) continue;
            if (rule.With != 0 && !Holds(mine, rule.With, rule.WithSeconds)) continue;
            return rule;
        }

        foreach (var rule in rules)
        {
            if (!rule.Absent || !Fits(rule)) continue;
            if (Holds(mine, rule.Status, rule.Seconds)) continue;
            return rule;
        }

        foreach (var rule in rules)
            if (rule.Status == 0 && !rule.Absent && Fits(rule)) return rule;

        return null;
    }

    public static void Say(SequenceRun run, Callout call, GameEvent on, IWorld world,
                           IReadOnlyList<Rule> rules)
    {
        var got = Match(world, rules);
        if (got is null) return;

        run.SetParam(TextParam, got.Text);
        run.SetParam(SpeechParam, got.Speech);
        run.Call(call, on);
    }

    public sealed record Held(
        IReadOnlyList<uint> Casts, Callout Call, IReadOnlyList<Rule> Rules)
    {
        public Held(uint cast, Callout call, IReadOnlyList<Rule> rules)
            : this([cast], call, rules)
        {
        }
    }

    public static Sequence Cooled(
        string name, double cooldown, IWorld world, params Held[] held)
    {
        var byId = new Dictionary<uint, Held>();
        foreach (var one in held)
            foreach (var cast in one.Casts)
                byId[cast] = one;

        var gate = new CallCooldown(cooldown);

        return Sequence.Indexed(name, CastCalls.TimeoutSeconds,
            e => e.Kind == EventKind.CastStart && byId.ContainsKey(e.Id),
            (start, run, invocation) =>
            {
                var one = byId[start.Id];
                var got = Match(world, one.Rules);
                if (got is null) return Task.CompletedTask;

                if (!gate.Ready(one.Call, start.At)) return Task.CompletedTask;

                run.SetParam(TextParam, got.Text);
                run.SetParam(SpeechParam, got.Speech);
                run.Call(one.Call, start);
                return Task.CompletedTask;
            });
    }
}
