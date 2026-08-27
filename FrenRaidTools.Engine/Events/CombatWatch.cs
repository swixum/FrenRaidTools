namespace FrenRaidTools.Engine;

public sealed class CombatWatch
{
    public const double SettleSeconds = 2.0;

    private bool _fighting;
    private double? _quietSince;

    public GameEvent? Take(bool fighting, double at)
    {
        if (fighting)
        {
            _quietSince = null;
            if (_fighting) return null;

            _fighting = true;
            return new GameEvent { Kind = EventKind.CombatStart, At = at };
        }

        if (!_fighting) return null;

        _quietSince ??= at;
        if (at - _quietSince.Value < SettleSeconds) return null;

        _fighting = false;
        _quietSince = null;
        return new GameEvent { Kind = EventKind.CombatEnd, At = at };
    }

    public void Forget()
    {
        _fighting = false;
        _quietSince = null;
    }
}
