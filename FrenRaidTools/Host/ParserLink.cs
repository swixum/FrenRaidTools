using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace FrenRaidTools;

public enum ParserState
{
    Off,
    Looking,
    Live,
    Broken,
}

public sealed class ParserLink : IDisposable
{
    private const string IinactListening = "IINACT.Server.Listening";
    private const double ProbeSeconds = 3.0;
    private const double StaleSeconds = 15.0;
    private const int ReceiveBuffer = 8192;

    private readonly Configuration _config;

    private CancellationTokenSource? _socketStop;
    private Task? _socketTask;
    private string _socketAddress = "";

    private double _nextProbe;
    private double _now;

    public ParserLink(Configuration config) => _config = config;

    public ParserState State { get; private set; } = ParserState.Off;

    public string Source { get; private set; } = "";

    public string Detail { get; private set; } = "Not looking.";

    public long Lines { get; private set; }

    public double LastLineAt { get; private set; } = double.NegativeInfinity;

    public bool IinactFound { get; private set; }

    public bool SocketOpen { get; private set; }

    public string SocketDetail { get; private set; } = "Not connected.";

    public double Silence => LastLineAt <= double.NegativeInfinity ? -1 : _now - LastLineAt;

    public uint Dot => State switch
    {
        ParserState.Live => Ui.Theme.Good,
        ParserState.Looking => Ui.Theme.Warn,
        ParserState.Broken => Ui.Theme.Danger,
        _ => Ui.Theme.Muted,
    };

    public void Tick(double now)
    {
        _now = now;

        if (!_config.ParserOn)
        {
            if (State != ParserState.Off) Stop(wait: false);
            State = ParserState.Off;
            Source = "";
            Detail = "Turned off.";
            return;
        }

        if (now < _nextProbe) return;
        _nextProbe = now + ProbeSeconds;

        IinactFound = ProbeIinact();

        if (_config.ParserSource != Configuration.SourceAct && IinactFound)
        {
            Stop(wait: false);
            Source = "IINACT";
            State = ParserState.Live;
            Detail = "IINACT is listening.";
            return;
        }

        if (_config.ParserSource == Configuration.SourceIinact)
        {
            Stop(wait: false);
            Source = "";
            State = ParserState.Broken;
            Detail = "IINACT is not running.";
            return;
        }

        Pump();
    }

    private void Pump()
    {
        var wanted = _config.ParserAddress.Trim();

        if (_socketTask is { IsCompleted: true } || _socketAddress != wanted)
            Stop(wait: false);

        if (_socketTask is null && wanted.Length > 0)
        {
            _socketAddress = wanted;
            _socketStop = new CancellationTokenSource();
            _socketTask = Task.Run(() => Listen(wanted, _socketStop.Token));
        }

        Source = SocketOpen ? "ACT" : "";

        if (SocketOpen)
        {
            var quiet = Silence;
            State = quiet > StaleSeconds ? ParserState.Looking : ParserState.Live;
            Detail = quiet < 0
                ? "Connected, nothing through yet."
                : quiet > StaleSeconds
                    ? $"Connected, quiet for {quiet:0}s."
                    : $"Connected, {Lines} lines.";
            return;
        }

        State = ParserState.Broken;
        Detail = SocketDetail;
    }

    private bool ProbeIinact()
    {
        try
        {
            return Service.PluginInterface.GetIpcSubscriber<bool>(IinactListening).InvokeFunc();
        }
        catch
        {
            return false;
        }
    }

    private async Task Listen(string address, CancellationToken token)
    {
        using var socket = new ClientWebSocket();

        try
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                SocketDetail = "That address does not read as a URL.";
                return;
            }

            await socket.ConnectAsync(uri, token);
            SocketOpen = true;
            SocketDetail = "Connected.";

            var buffer = new byte[ReceiveBuffer];
            var subscribe = Encoding.UTF8.GetBytes(
                "{\"call\":\"subscribe\",\"events\":[\"LogLine\"]}");
            await socket.SendAsync(subscribe, WebSocketMessageType.Text, true, token);

            while (!token.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var got = await socket.ReceiveAsync(buffer, token);
                if (got.MessageType == WebSocketMessageType.Close) break;
                if (got.Count <= 0) continue;

                Lines++;
                LastLineAt = _now;
            }

            SocketDetail = "The parser closed the connection.";
        }
        catch (OperationCanceledException)
        {
            SocketDetail = "Stopped.";
        }
        catch (Exception ex)
        {
            SocketDetail = Short(ex);
        }
        finally
        {
            SocketOpen = false;
        }
    }

    private static string Short(Exception ex) => ex switch
    {
        WebSocketException => "Nothing is listening there. Start OverlayPlugin WSServer.",
        _ => ex.Message,
    };

    public void Retry()
    {
        Stop(wait: false);
        _nextProbe = 0;
        SocketDetail = "Trying again.";
    }

    private void Stop(bool wait)
    {
        try
        {
            _socketStop?.Cancel();
            if (wait) _socketTask?.Wait(TimeSpan.FromMilliseconds(400));
        }
        catch
        {
        }
        finally
        {
            _socketStop?.Dispose();
            _socketStop = null;
            _socketTask = null;
            _socketAddress = "";
            SocketOpen = false;
        }
    }

    public void Dispose() => Stop(wait: true);
}
