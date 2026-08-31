using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruBoundOfFaith
{
    public const string Group = "fru.boundOfFaith";

    public const string MechanicName = "Bound of Faith";

    public const string SequenceName = Group + ".sides";

    public const uint BoundOfFaith = 0x9CE5;

    public const uint FireTether = 0x00F9;

    public const int Tethers = 2;

    public const double TimeoutSeconds = 20;

    public static readonly Callout boundSide = new()
    {
        Description = "Bound of Faith",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "boundSide",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Supports take the north stack and the DPS the south one.\n"
                + "Both tethers on one side moves two people, and the call says so to the "
                + "two it moves.",
    };

    public static (string Text, string Speech) Words(bool north, bool flexed)
    {
        var side = north ? "North" : "South";
        var line = flexed ? $"{side} stack, flex" : $"{side} stack";
        return (line, line);
    }

    public static Sequence Build(IWorld world) =>
        Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Is(EventKind.Tether, FireTether),
            async (first, run) =>
            {
                var rest = await run.WaitEvents(Tethers - 1, EventKind.Tether,
                    e => e.Id == FireTether);
                if (rest.Count < Tethers - 1) return;

                var start = await run.FindOrWaitForCast(world, e => e.Id == BoundOfFaith);
                if (start is null) return;

                var tethers = new List<GameEvent> { first };
                tethers.AddRange(rest);

                var seat = SeatCalls.MySeat(world);
                var held = FruFallOfFaith.TetheredSeats(world, tethers);
                if (seat < 0 || held.Count != Tethers) return;

                var north = FruAssignments.BoundNorth(seat, held);
                if (north is null) return;

                var (text, speech) = Words(
                    north.Value, north.Value != FruAssignments.BoundBaseNorth(seat));

                run.SetParam(SeatCalls.TextParam, text);
                run.SetParam(SeatCalls.SpeechParam, speech);
                run.Call(boundSide, start);
            });

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 1, new FruBoundOfFaith(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
