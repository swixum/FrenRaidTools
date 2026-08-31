using System.Runtime.CompilerServices;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruExplosion
{
    public const string Group = "fru.explosion";

    public const string MechanicName = "Final Towers";

    public const string SequenceName = Group + ".towers";

    public static readonly uint[] Towers =
        [0x9CBA, 0x9CBB, 0x9CBC, 0x9CBD, 0x9CBE, 0x9CBF, 0x9CC3, 0x9CC7];

    public const int Count = 3;

    public const int Tries = 20;

    public const int WaitMs = 100;

    public const double TimeoutSeconds = 20;

    public static readonly Callout explosionTower = new()
    {
        Description = "Final Towers",
        Mechanic = MechanicName,
        Phase = 1,
        Key = "explosionTower",
        Speech = SeatCalls.Speech,
        Text = SeatCalls.Text + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "The three towers ask for six bodies in a pattern the pull rolls, so which "
                + "tower a filler joins is read off the casts rather than fixed.\n"
                + "The tanks take the half away from the tower column.",
    };

    public static int Soakers(uint cast) => cast switch
    {
        0x9CC3 or 0x9CC7 => 1,
        0x9CBA or 0x9CBD => 2,
        0x9CBB or 0x9CBE => 3,
        0x9CBC or 0x9CBF => 4,
        _ => 0,
    };

    public static List<GameEvent> Standing(IWorld world) =>
        world.ActiveCasts()
            .Where(c => Towers.Contains(c.Id) && c.Source is not null && c.Source.Pos.Known)
            .GroupBy(c => c.Source!.ObjectId)
            .Select(g => g.First())
            .OrderBy(c => c.Source!.Pos.Y)
            .ToList();

    public static Dictionary<int, int> TowerPerSeat(IReadOnlyList<int> soakers)
    {
        var towers = new Dictionary<int, int>();
        if (soakers.Count != Count || soakers.Sum() != 6) return towers;

        for (var tower = 0; tower < Count; tower++)
            towers[Slots.IndexOf(FruAssignments.ExplosionFixed[tower])] = tower;

        var next = 0;
        for (var tower = 0; tower < Count; tower++)
            for (var extra = 1; extra < soakers[tower] && next < FruAssignments.ExplosionFlex.Length; extra++)
                towers[Slots.IndexOf(FruAssignments.ExplosionFlex[next++])] = tower;

        return towers;
    }

    public static ArenaSector TankSpot(int seat, ArenaSector towerSide)
    {
        if (!towerSide.IsCardinal()) return ArenaSector.Unknown;
        var away = towerSide.Opposite();
        var half = seat == Slots.IndexOf("MT") ? ArenaSector.North : ArenaSector.South;
        return ArenaSectors.Between(half, away);
    }

    public static ArenaSector TowerSide(IReadOnlyList<GameEvent> towers)
    {
        if (towers.Count == 0) return ArenaSector.Unknown;
        var middle = towers.Average(t => t.Source!.Pos.X);
        return middle > FruArena.CenterX ? ArenaSector.East : ArenaSector.West;
    }

    public static readonly IReadOnlyList<string> Places = ["North", "Middle", "South"];

    public static (string Text, string Speech) TowerWords(int tower)
    {
        var place = Places[Math.Clamp(tower, 0, Places.Count - 1)];
        return ($"{place} tower", $"{place} tower");
    }

    public static (string Text, string Speech) TankWords(ArenaSector spot) =>
        ($"{spot.Name()}, off towers", $"{spot.Name()}, off towers");

    public static (string Text, string Speech)? Line(
        int seat, IReadOnlyList<GameEvent> towers)
    {
        if (seat < 0 || towers.Count != Count) return null;

        if (seat == Slots.IndexOf("MT") || seat == Slots.IndexOf("OT"))
        {
            var spot = TankSpot(seat, TowerSide(towers));
            return spot.IsPoint() ? TankWords(spot) : null;
        }

        var mine = TowerPerSeat(towers.Select(t => Soakers(t.Id)).ToList());
        return mine.TryGetValue(seat, out var tower) ? TowerWords(tower) : null;
    }

    public static Sequence Build(IWorld world)
    {
        var gate = new CallCooldown(TimeoutSeconds);

        return Sequence.Repeat(SequenceName, TimeoutSeconds,
            e => e.Kind == EventKind.CastStart && Towers.Contains(e.Id),
            async (start, run) =>
            {
                if (!gate.Ready(explosionTower, start.At)) return;

                var towers = Standing(world);
                for (var tries = 0; tries < Tries && towers.Count < Count; tries++)
                {
                    await run.WaitMs(WaitMs);
                    towers = Standing(world);
                }

                var line = Line(SeatCalls.MySeat(world), towers);
                if (line is null) return;

                run.SetParam(SeatCalls.TextParam, line.Value.Text);
                run.SetParam(SeatCalls.SpeechParam, line.Value.Speech);
                run.Call(explosionTower, start);
            });
    }

    [ModuleInitializer]
    internal static void Register() =>
        LocalFights.Register(new LocalFight(
            "fru", Group, MechanicName, 1, new FruExplosion(), null)
        {
            PhaseNames = FruArena.PhaseNames,
            Extra = world => [Build(world)],
        });
}
