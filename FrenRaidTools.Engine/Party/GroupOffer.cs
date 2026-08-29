namespace FrenRaidTools.Engine;

public sealed class GroupOffer
{
    public const double LookForSeconds = 60.0;
    public const double LookEverySeconds = 1.0;

    private double _until;
    private double _next;
    private int _pending = -1;

    public int Pending => _pending;

    public bool Waiting => _until > 0 && _pending < 0;

    public void Arm(double now)
    {
        _until = now + LookForSeconds;
        _next = 0;
        _pending = -1;
    }

    public void Drop()
    {
        _until = 0;
        _next = 0;
        _pending = -1;
    }

    public void Look(IReadOnlyList<Roster> setups, IReadOnlyList<PartyMember> party, int active, double now)
    {
        if (!Waiting) return;

        if (now >= _until)
        {
            Drop();
            return;
        }

        if (now < _next) return;
        _next = now + LookEverySeconds;

        if (setups is null || setups.Count < 2) return;
        if (party is null || party.Count < GroupMatch.Floor) return;

        var pick = GroupMatch.Pick(setups, party, active);
        if (pick == active) return;

        _pending = pick;
    }

    public int Take()
    {
        var at = _pending;
        Drop();
        return at;
    }
}
