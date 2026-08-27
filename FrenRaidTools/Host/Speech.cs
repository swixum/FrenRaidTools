using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace FrenRaidTools;

public sealed class Speech : IDisposable
{
    private const int QueueCap = 8;

    private readonly record struct Job(string Text, int Rate, int Volume, string Voice);

    private readonly BlockingCollection<Job> _queue = new(QueueCap);
    private readonly CancellationTokenSource _stopping = new();
    private readonly Thread _worker;

    private object? _voice;
    private Type? _type;
    private string _appliedVoice = "";
    private int _appliedRate = int.MinValue;
    private int _appliedVolume = int.MinValue;

    private volatile string _status = "Starting.";
    private volatile string[] _voices = [];

    public Speech()
    {
        _worker = new Thread(Run)
        {
            IsBackground = true,
            Name = "FrenRaidTools.Speech",
        };
        _worker.Start();
    }

    public string Status => _status;

    public IReadOnlyList<string> Voices => _voices;

    public int Dropped { get; private set; }

    public bool Say(string text, int rate, int volume, string voice)
    {
        if (string.IsNullOrWhiteSpace(text) || _stopping.IsCancellationRequested) return false;

        if (_queue.TryAdd(new Job(text, rate, volume, voice))) return true;

        Dropped++;
        return false;
    }

    private void Run()
    {
        if (!Connect()) return;

        LoadVoices();
        _status = _voices.Length > 0 ? $"Ready, {_voices.Length} voices." : "Ready.";

        try
        {
            foreach (var job in _queue.GetConsumingEnumerable(_stopping.Token))
                Speak(job);
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

            if (job.Volume != _appliedVolume)
            {
                Set(_voice, "Volume", Math.Clamp(job.Volume, 0, 100));
                _appliedVolume = job.Volume;
            }

            Invoke(_voice, "Speak", job.Text, 0);
        }
        catch (Exception ex)
        {
            _status = $"Speak failed: {ex.Message}";
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
            _stopping.Cancel();
            _queue.CompleteAdding();
            stopped = _worker.Join(TimeSpan.FromMilliseconds(StopWaitMs));
        }
        catch
        {
        }

        try
        {
            _queue.Dispose();
            _stopping.Dispose();
        }
        catch
        {
        }

        var voice = _voice;
        _voice = null;

        if (!stopped) return;

        if (voice is not null && System.Runtime.InteropServices.Marshal.IsComObject(voice))
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(voice);
    }
}
