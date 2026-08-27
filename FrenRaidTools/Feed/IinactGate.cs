using System.Collections.Concurrent;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json.Linq;

namespace FrenRaidTools.Feed;

public sealed class IinactGate : IDisposable
{
    public const string Receiver = "FrenRaidTools.ParserLines";
    public const int MaxQueued = 4096;
    public const double HealthEverySeconds = 30.0;

    private const string Listening = "IINACT.Server.Listening";
    private const string MakeSubscriber = "IINACT.CreateSubscriber";
    private const string DropSubscriber = "IINACT.Unsubscribe";
    private const string SendTo = "IINACT.IpcProvider." + Receiver;
    private const string Subscribe = """{"call":"subscribe","events":["LogLine"]}""";

    private readonly ICallGateProvider<JObject, bool> _gate;
    private readonly ConcurrentQueue<string> _lines = new();
    private double _nextHealth;

    public bool Subscribed { get; private set; }

    public long Received { get; private set; }

    public long Dropped { get; private set; }

    public string? LastError { get; private set; }

    public IinactGate()
    {
        _gate = Service.PluginInterface.GetIpcProvider<JObject, bool>(Receiver);
        _gate.RegisterFunc(Take);
    }

    private bool Take(JObject message)
    {
        if (message["type"]?.ToString() != "LogLine") return true;

        var line = message["rawLine"]?.ToString();
        if (string.IsNullOrEmpty(line)) return true;

        Received++;

        if (_lines.Count >= MaxQueued)
        {
            _lines.TryDequeue(out _);
            Dropped++;
        }

        _lines.Enqueue(line);
        return true;
    }

    public bool Alive()
    {
        try
        {
            return Service.PluginInterface.GetIpcSubscriber<bool>(Listening).InvokeFunc();
        }
        catch
        {
            return false;
        }
    }

    public bool Start()
    {
        if (Subscribed) return true;
        if (!Alive()) return false;

        try
        {
            Drop();

            if (!Service.PluginInterface.GetIpcSubscriber<string, bool>(MakeSubscriber)
                    .InvokeFunc(Receiver))
                return false;

            Service.PluginInterface.GetIpcSubscriber<JObject, bool>(SendTo)
                .InvokeAction(JObject.Parse(Subscribe));

            Subscribed = true;
            LastError = null;
            _nextHealth = 0;
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
    }

    public void Watch(double now)
    {
        if (!Subscribed) return;
        if (now < _nextHealth) return;
        _nextHealth = now + HealthEverySeconds;

        if (!Alive()) Stop();
    }

    public void Stop()
    {
        Subscribed = false;
        Drop();
        while (_lines.TryDequeue(out _)) { }
    }

    private static void Drop()
    {
        try
        {
            Service.PluginInterface.GetIpcSubscriber<string, bool>(DropSubscriber)
                .InvokeFunc(Receiver);
        }
        catch
        {
        }
    }

    public int Drain(Action<string> onLine, int max = 512)
    {
        var taken = 0;
        while (taken < max && _lines.TryDequeue(out var line))
        {
            onLine(line);
            taken++;
        }
        return taken;
    }

    public void Dispose()
    {
        Stop();
        _gate.UnregisterFunc();
    }
}
