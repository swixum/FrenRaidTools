namespace FrenRaidTools.Engine;

public static class SeatCalls
{
    public const string TextParam = "seat";
    public const string SpeechParam = "seatSpeech";

    public const string Text = "{" + TextParam + "}";
    public const string Speech = "{" + SpeechParam + "}";

    public static int MySeat(IWorld world) =>
        world.You is null ? -1 : world.SeatOf(world.You);

    public static string? Line(string[] bySeat, int seat) =>
        seat >= 0 && seat < bySeat.Length && bySeat[seat].Length > 0 ? bySeat[seat] : null;

    public sealed record Seated(
        IReadOnlyList<uint> Casts, Callout Call, string[] Text, string[] Speech)
    {
        public Seated(uint cast, Callout call, string[] text, string[] speech)
            : this([cast], call, text, speech)
        {
        }
    }

    public static Sequence Cooled(
        string name, double cooldown, IWorld world, params Seated[] seated)
    {
        var byId = new Dictionary<uint, Seated>();
        foreach (var one in seated)
            foreach (var cast in one.Casts)
                byId[cast] = one;

        var gate = new CallCooldown(cooldown);

        return Sequence.Indexed(name, CastCalls.TimeoutSeconds,
            e => e.Kind == EventKind.CastStart && byId.ContainsKey(e.Id),
            (start, run, invocation) =>
            {
                var one = byId[start.Id];
                var seat = MySeat(world);
                var text = Line(one.Text, seat);
                var speech = Line(one.Speech, seat);
                if (text is null || speech is null) return Task.CompletedTask;

                if (!gate.Ready(one.Call, start.At)) return Task.CompletedTask;

                run.SetParam(TextParam, text);
                run.SetParam(SpeechParam, speech);
                run.Call(one.Call, start);
                return Task.CompletedTask;
            });
    }

    public static void Say(SequenceRun run, Callout call, GameEvent on,
                           IWorld world, string[] text, string[] speech)
    {
        var seat = MySeat(world);
        var show = Line(text, seat);
        var said = Line(speech, seat);
        if (show is null || said is null) return;

        run.SetParam(TextParam, show);
        run.SetParam(SpeechParam, said);
        run.Call(call, on);
    }
}
