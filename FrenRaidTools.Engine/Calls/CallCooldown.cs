namespace FrenRaidTools.Engine;

public sealed class CallCooldown(double seconds)
{
    private readonly Dictionary<string, double> _lastSaid = new(StringComparer.Ordinal);

    public static string KeyOf(Callout call) =>
        call.Key.Length > 0 ? call.Key : call.Description;

    public int Held
    {
        get { lock (_lastSaid) return _lastSaid.Count; }
    }

    public void Clear()
    {
        lock (_lastSaid) _lastSaid.Clear();
    }

    public bool Ready(Callout call, double now)
    {
        var key = KeyOf(call);

        lock (_lastSaid)
        {
            if (_lastSaid.TryGetValue(key, out var said))
            {
                var since = now - said;
                if (since >= 0 && since < seconds) return false;
            }

            _lastSaid[key] = now;
            return true;
        }
    }
}
