namespace FrenRaidTools.Engine;

public sealed class CallWindow
{
    private double _from = double.NegativeInfinity;

    private double _to = double.NegativeInfinity;

    public void Open(double at, double seconds)
    {
        _from = at;
        _to = at + seconds;
    }

    public void Close()
    {
        _from = double.NegativeInfinity;
        _to = double.NegativeInfinity;
    }

    public bool Covers(double at) => at >= _from && at <= _to;
}
