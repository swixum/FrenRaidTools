namespace FrenRaidTools.Engine;

public static class CastCalls
{
    public const double TimeoutSeconds = 30;

    public static Sequence For(string name, params (uint Cast, Callout Call)[] pairs)
    {
        var byId = new Dictionary<uint, Callout>();
        foreach (var (cast, call) in pairs)
            byId[cast] = call;

        return Sequence.Indexed(name, TimeoutSeconds,
            e => e.Kind == EventKind.CastStart && byId.ContainsKey(e.Id),
            (start, run, invocation) =>
            {
                run.Call(byId[start.Id], start);
                return Task.CompletedTask;
            });
    }

    public const double DefaultCooldown = 4.0;

    public static Sequence Cooled(
        string name, double cooldown, params (uint Cast, Callout Call)[] pairs) =>
        CooledWhen(name, cooldown, null, pairs);

    public static Sequence CooledWhen(
        string name, double cooldown, Func<bool>? allow,
        params (uint Cast, Callout Call)[] pairs)
    {
        var byId = new Dictionary<uint, Callout>();
        foreach (var (cast, call) in pairs)
            byId[cast] = call;

        var gate = new CallCooldown(cooldown);

        return Sequence.Indexed(name, TimeoutSeconds,
            e => e.Kind == EventKind.CastStart && byId.ContainsKey(e.Id),
            (start, run, invocation) =>
            {
                if (allow is not null && !allow()) return Task.CompletedTask;

                var call = byId[start.Id];
                if (!gate.Ready(call, start.At)) return Task.CompletedTask;

                run.Call(call, start);
                return Task.CompletedTask;
            });
    }
}
