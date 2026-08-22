namespace FrenRaidTools.Engine;

public readonly record struct CallTicket(string Key, Action<string>? Expire)
{
    public void ForceExpire()
    {
        if (Key.Length > 0) Expire?.Invoke(Key);
    }
}
