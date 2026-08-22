namespace FrenRaidTools.Engine;

public sealed class PhaseGate
{
    public int Phase { get; private set; }

    public int Dropped { get; private set; }

    public bool Enter(int phase)
    {
        if (phase <= Phase) return false;
        Phase = phase;
        return true;
    }

    public bool LeftBehind(int phase) => phase > 0 && phase < Phase;

    public void Dropping(int count) => Dropped += count;

    public void Reset()
    {
        Phase = 0;
        Dropped = 0;
    }
}
