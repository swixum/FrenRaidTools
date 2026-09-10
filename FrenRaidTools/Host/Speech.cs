using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace FrenRaidTools;

public sealed class Speech : IDisposable
{
    private const int QueueCap = 8;

    public const int SliceMs = 40;

    public const int Async = 1;
    public const int Purge = 2;

    private sealed record Job(string Text, int Rate, int Volume, string Voice, int Under);

    private readonly BlockingCollection<Job> _queue = new(QueueCap);
    private readonly BlockingCollection<Job> _onTop = new(QueueCap);
    private readonly CancellationTokenSource _stopping = new();
    private readonly Thread _worker;
    private readonly Thread _overWorker;

    private readonly object _level = new();

    private object? _voice;
    private object? _over;
    private Type? _type;
    private string _appliedVoice = "";
    private int _appliedRate = int.MinValue;
    private int _appliedVolume = int.MinValue;
    private int _appliedOverRate = int.MinValue;
    private int _appliedOverVolume = int.MinValue;

    private int _ducked = -1;
    private int _wanted = 90;

    private volatile string _status = "Starting.";
    private volatile string[] _voices = [];
    private volatile int _spoken;
    private volatile bool _speaking;
    private volatile bool _ready;

    public Speech()
    {
        _worker = new Thread(Run)
        {
            IsBackground = true,
            Name = "FrenRaidTools.Speech",
        };
        _worker.Start();

        _overWorker = new Thread(RunOnTop)
        {
            IsBackground = true,
            Name = "FrenRaidTools.SpeechOnTop",
        };
        _overWorker.Start();
    }

    public string Status => _status;

    public IReadOnlyList<string> Voices => _voices;

    public int Dropped { get; private set; }

    public int Spoken => _spoken;

    public bool Speaking => _speaking;

    public int VolumeNow
    {
        get { lock (_level) return _appliedVolume; }
    }

    public bool Say(string text, int rate, int volume, string voice, bool onTop = false,
        int under = -1)
    {
        if (string.IsNullOrWhiteSpace(text) || _stopping.IsCancellationRequested) return false;

        var job = new Job(text, rate, volume, voice, under);
        var lane = onTop ? _onTop : _queue;

        if (lane.TryAdd(job)) return true;

        Dropped++;
        return false;
    }

    private void Run()
    {
        if (!Connect()) return;

        LoadVoices();
        _status = _voices.Length > 0 ? $"Ready, {_voices.Length} voices." : "Ready.";
        _ready = true;

        try
        {
            foreach (var job in _queue.GetConsumingEnumerable(_stopping.Token))
            {
                Speak(job);
                Settle();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _status = $"Stopped: {ex.Message}";
        }
    }

    private void RunOnTop()
    {
        try
        {
            foreach (var job in _onTop.GetConsumingEnumerable(_stopping.Token))
            {
                while (!_ready && !_stopping.IsCancellationRequested) Thread.Sleep(SliceMs);

                SpeakOver(job);

                if (_onTop.Count == 0) Restore();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _status = $"Stopped: {ex.Message}";
        }
    }

    private bool Connect()
    {
        try
        {
            _type = Type.GetTypeFromProgID("SAPI.SpVoice");
            if (_type is null)
            {
                _status = "Windows speech not available.";
                return false;
            }

            _voice = Activator.CreateInstance(_type);
            if (_voice is null)
            {
                _status = "Windows speech would not start.";
                return false;
            }

            _over = Activator.CreateInstance(_type);

            return true;
        }
        catch (Exception ex)
        {
            _status = $"Speech failed: {ex.Message}";
            return false;
        }
    }

    private void LoadVoices()
    {
        try
        {
            var tokens = Invoke(_voice!, "GetVoices", "", "");
            if (tokens is null) return;

            var count = Get(tokens, "Count");
            if (count is not int n || n <= 0) return;

            var names = new List<string>(n);
            for (var i = 0; i < n; i++)
            {
                var token = Invoke(tokens, "Item", i);
                if (token is null) continue;
                if (Invoke(token, "GetDescription", 0) is string description
                    && !string.IsNullOrWhiteSpace(description))
                    names.Add(description);
            }

            _voices = [.. names];
        }
        catch (Exception ex)
        {
            _status = $"Voice list failed: {ex.Message}";
        }
    }

    private void Speak(Job job)
    {
        if (_voice is null) return;

        try
        {
            ApplyVoice(job.Voice);

            if (job.Rate != _appliedRate)
            {
                Set(_voice, "Rate", Math.Clamp(job.Rate, -10, 10));
                _appliedRate = job.Rate;
            }

            lock (_level)
            {
                _wanted = Math.Clamp(job.Volume, 0, 100);
                Level(_ducked >= 0 ? _ducked : _wanted);
            }

            _speaking = true;
            Invoke(_voice, "Speak", job.Text, Async);
            _spoken++;
        }
        catch (Exception ex)
        {
            _speaking = false;
            _status = $"Speak failed: {ex.Message}";
        }
    }

    private void SpeakOver(Job job)
    {
        var over = _over;
        if (over is null)
        {
            Speak(job);
            return;
        }

        try
        {
            if (job.Rate != _appliedOverRate)
            {
                Set(over, "Rate", Math.Clamp(job.Rate, -10, 10));
                _appliedOverRate = job.Rate;
            }

            var loud = Math.Clamp(job.Volume, 0, 100);
            if (loud != _appliedOverVolume)
            {
                Set(over, "Volume", loud);
                _appliedOverVolume = loud;
            }

            if (job.Under >= 0)
                lock (_level)
                {
                    _ducked = Math.Clamp(job.Under, 0, 100);
                    if (_speaking) Level(_ducked);
                }

            Invoke(over, "Speak", job.Text, Async);
            _spoken++;
            Invoke(over, "WaitUntilDone", -1);
        }
        catch (Exception ex)
        {
            _status = $"Speak failed: {ex.Message}";
        }
    }

    private void Restore()
    {
        lock (_level)
        {
            if (_ducked < 0) return;

            _ducked = -1;
            Level(_wanted);
        }
    }

    private void Level(int volume)
    {
        if (_voice is null || volume == _appliedVolume) return;

        try
        {
            Set(_voice, "Volume", volume);
            _appliedVolume = volume;
        }
        catch (Exception ex)
        {
            _status = $"Speak failed: {ex.Message}";
        }
    }

    private void Settle()
    {
        if (_voice is null) return;

        while (!_stopping.IsCancellationRequested)
        {
            try
            {
                if (Invoke(_voice, "WaitUntilDone", SliceMs) is not true) continue;

                _speaking = false;
                return;
            }
            catch (Exception ex)
            {
                _speaking = false;
                _status = $"Speak failed: {ex.Message}";
                return;
            }
        }
    }

    private void ApplyVoice(string wanted)
    {
        if (wanted == _appliedVoice) return;
        _appliedVoice = wanted;
        if (string.IsNullOrWhiteSpace(wanted)) return;

        try
        {
            var tokens = Invoke(_voice!, "GetVoices", "", "");
            if (tokens is null) return;

            if (Get(tokens, "Count") is not int n) return;

            for (var i = 0; i < n; i++)
            {
                var token = Invoke(tokens, "Item", i);
                if (token is null) continue;
                if (Invoke(token, "GetDescription", 0) is not string description) continue;
                if (!string.Equals(description, wanted, StringComparison.OrdinalIgnoreCase)) continue;

                Set(_voice!, "Voice", token);
                return;
            }
        }
        catch (Exception ex)
        {
            _status = $"Voice pick failed: {ex.Message}";
        }
    }

    private static object? Invoke(object target, string member, params object?[] args) =>
        target.GetType().InvokeMember(member, BindingFlags.InvokeMethod, null, target, args);

    private static object? Get(object target, string member) =>
        target.GetType().InvokeMember(member, BindingFlags.GetProperty, null, target, null);

    private static void Set(object target, string member, object value) =>
        target.GetType().InvokeMember(member, BindingFlags.SetProperty, null, target, [value]);

    public const int StopWaitMs = 2000;

    public void Dispose()
    {
        var stopped = false;

        try
        {
            if (_voice is not null) Invoke(_voice, "Speak", "", Async | Purge);
            if (_over is not null) Invoke(_over, "Speak", "", Async | Purge);
        }
        catch
        {
        }

        try
        {
            _stopping.Cancel();
            _queue.CompleteAdding();
            _onTop.CompleteAdding();
            stopped = _worker.Join(TimeSpan.FromMilliseconds(StopWaitMs))
                      && _overWorker.Join(TimeSpan.FromMilliseconds(StopWaitMs));
        }
        catch
        {
        }

        try
        {
            _queue.Dispose();
            _onTop.Dispose();
            _stopping.Dispose();
        }
        catch
        {
        }

        var voice = _voice;
        var over = _over;
        _voice = null;
        _over = null;

        if (!stopped) return;

        if (voice is not null && System.Runtime.InteropServices.Marshal.IsComObject(voice))
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(voice);

        if (over is not null && System.Runtime.InteropServices.Marshal.IsComObject(over))
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(over);
    }
}
