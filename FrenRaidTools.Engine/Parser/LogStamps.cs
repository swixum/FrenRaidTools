namespace FrenRaidTools.Engine;

public sealed class LogStamps
{
    public const double MaxLagSeconds = 30.0;

    private double _offset = double.NaN;

    public bool Anchored => !double.IsNaN(_offset);

    public double Lag { get; private set; }

    public int Anchors { get; private set; }

    public double At(double logSeconds, double now)
    {
        if (double.IsNaN(logSeconds)) return now;

        if (double.IsNaN(_offset)) Anchor(logSeconds, now);

        var at = logSeconds + _offset;

        if (at > now || now - at > MaxLagSeconds)
        {
            Anchor(logSeconds, now);
            at = now;
        }

        Lag = now - at;
        return at;
    }

    private void Anchor(double logSeconds, double now)
    {
        _offset = now - logSeconds;
        Anchors++;
    }

    public void Reset()
    {
        _offset = double.NaN;
        Lag = 0;
    }
}
