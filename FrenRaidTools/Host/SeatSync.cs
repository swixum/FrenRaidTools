namespace FrenRaidTools;

public static class SeatSync
{
    private const double EverySeconds = 1.5;

    private static double _next;

    public static string LastSeat { get; private set; } = "";

    public static void Apply(Configuration config, double now)
    {
        if (now < _next) return;
        _next = now + EverySeconds;

        var seat = SeatFor(config);
        LastSeat = seat;

        if (Party.YouName().Length == 0) return;

        var moved = false;
        foreach (var fight in Engine.FightPlans.All)
        {
            var pick = config.PlanFor(fight.Key);
            if (pick.Seat == seat) continue;
            pick.Seat = seat;
            moved = true;
        }

        if (moved) config.Save(now);
    }

    public static string SeatFor(Configuration config)
    {
        var you = Party.YouName();
        return you.Length == 0 ? "" : config.Roles.SlotName(you);
    }

    public static void Reset() => _next = 0;
}
