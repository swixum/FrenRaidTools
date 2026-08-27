using FrenRaidTools.Engine;

namespace FrenRaidTools;

public static class RosterGlance
{
    public const double EverySeconds = 1.0;

    private static double _next;
    private static List<PartyMember> _members = [];

    public static List<PartyMember> Members(double now)
    {
        if (now >= _next)
        {
            _next = now + EverySeconds;
            _members = Party.Read();
        }

        return _members;
    }

    public static SpotVerdict[]? Verdicts(Configuration config, double now)
    {
        if (!Game.InDuty && !Game.InReplay) return null;

        var party = Members(now);
        if (party.Count == 0) return null;

        return RosterCheck.Against(config.Roles, [.. party.Select(m => m.Name)]);
    }

    public static void Reset() => _next = 0;
}
