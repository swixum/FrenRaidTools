namespace FrenRaidTools.Engine;

public sealed class SpeechQueue
{
    public const int MaxPending = 32;
    public const double DefaultMaxAgeSeconds = 8.0;

    private readonly record struct Pending(string Text, double Due, CallRank Rank, object? Tag);

    private readonly List<Pending> _waiting = [];

    public const double StutterSeconds = 2.0;

    private double _spokeAt = double.NegativeInfinity;
    private string _spoke = "";

    public double MinGapSeconds { get; set; }

    public double MaxAgeSeconds { get; set; } = DefaultMaxAgeSeconds;

    public double StutterGuardSeconds { get; set; } = StutterSeconds;

    public int Dropped { get; private set; }

    public int Stale { get; private set; }

    public int Stuttered { get; private set; }

    public int Spoken { get; private set; }

    public int Waiting => _waiting.Count;

    public void Add(string text, double due, CallRank rank = CallRank.Normal,
        bool repeatsAloud = false, object? tag = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!repeatsAloud && Stutter(text, due))
        {
            Stuttered++;
            return;
        }

        if (_waiting.Count >= MaxPending)
        {
            _waiting.RemoveAt(Crowded());
            Dropped++;
        }

        _waiting.Add(new Pending(text, due, rank, tag));
    }

    private int Crowded()
    {
        var worst = 0;
        for (var i = 1; i < _waiting.Count; i++)
            if (_waiting[i].Rank > _waiting[worst].Rank) worst = i;
        return worst;
    }

    private bool Stutter(string text, double due)
    {
        if (StutterGuardSeconds <= 0) return false;

        foreach (var pending in _waiting)
            if (Echoes(pending.Text, text)) return true;

        return due - _spokeAt < StutterGuardSeconds && Echoes(_spoke, text);
    }

    public const string Break = ". ";

    public static bool Echoes(string said, string next)
    {
        if (said.Length == 0 || next.Length == 0) return false;
        if (string.Equals(said, next, StringComparison.Ordinal)) return true;

        var longer = said.Length >= next.Length ? said : next;
        var shorter = said.Length >= next.Length ? next : said;

        foreach (var sentence in longer.Split(Break, StringSplitOptions.TrimEntries))
            if (string.Equals(sentence, shorter, StringComparison.Ordinal)) return true;

        return false;
    }

    public int Pump(double now, Func<string, bool> speak) =>
        Pump(now, (line, _) => speak(line));

    public int Pump(double now, Func<string, object?, bool> speak)
    {
        DropStale(now);

        var said = 0;

        while (_waiting.Count > 0)
        {
            var next = Ready(now);
            if (next < 0) break;

            if (MinGapSeconds > 0 && now - _spokeAt < MinGapSeconds
                && _waiting[next].Rank != CallRank.Critical) break;

            if (!speak(_waiting[next].Text, _waiting[next].Tag)) break;

            _spoke = _waiting[next].Text;
            _waiting.RemoveAt(next);
            _spokeAt = now;
            Spoken++;
            said++;
        }

        return said;
    }

    private int Ready(double now)
    {
        var best = -1;

        for (var i = 0; i < _waiting.Count; i++)
        {
            if (now < _waiting[i].Due) continue;
            if (best < 0)
            {
                best = i;
                continue;
            }

            if (_waiting[i].Rank < _waiting[best].Rank) best = i;
        }

        return best;
    }

    private void DropStale(double now)
    {
        if (MaxAgeSeconds <= 0) return;

        for (var i = _waiting.Count - 1; i >= 0; i--)
        {
            if (now - _waiting[i].Due < MaxAgeSeconds) continue;
            _waiting.RemoveAt(i);
            Stale++;
        }
    }

    public void Clear() => _waiting.Clear();

    public void Reset()
    {
        _waiting.Clear();
        _spokeAt = double.NegativeInfinity;
        _spoke = "";
    }
}
