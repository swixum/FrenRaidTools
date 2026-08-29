using FrenRaidTools.Engine;

namespace FrenRaidTools;

public static class Offer
{
    private static readonly GroupOffer Group = new();

    public static int Pending => Group.Pending;

    public static bool Waiting => Group.Waiting;

    public static void Arm(double now) => Group.Arm(now);

    public static void Drop() => Group.Drop();

    public static void Dismiss() => Group.Drop();

    public static void Look(Configuration config, IReadOnlyList<PartyMember> party, double now) =>
        Group.Look(config.Setups, party, config.ActiveSetup, now);

    public static string Name(Configuration config) =>
        Group.Pending < 0 ? "" : GroupMatch.Label(config.Setups, Group.Pending);

    public static bool Take(Configuration config)
    {
        var at = Group.Take();
        if (at < 0 || at >= config.Setups.Count) return false;

        config.ActiveSetup = at;
        return true;
    }
}
