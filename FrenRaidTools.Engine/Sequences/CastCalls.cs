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
}
