using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FrenRaidTools.Feed;

public sealed class ParserSocket : IDisposable
{
    public const int MaxQueued = 4096;
    public const int ReceiveBufferBytes = 64 * 1024;

    public static readonly string[] Defaults =
    [
        "ws://127.0.0.1:10501/ws",
        "ws://localhost:10501/ws",
    ];

    private const string Subscribe = """{"call":"subscribe","events":["LogLine"]}""";

    private readonly ConcurrentQueue<string> _lines = new();
    private CancellationTokenSource? _stopping;
    private Task? _worker;
    private string[] _endpoints = Defaults;

    public static string[] Order(string address)
    {
        var wanted = address.Trim();
        if (wanted.Length == 0) return Defaults;

        var order = new List<string> { wanted };
        foreach (var fallback in Defaults)
            if (!order.Contains(fallback, StringComparer.OrdinalIgnoreCase)) order.Add(fallback);

        return [.. order];
    }

    public bool Enabled { get; private set; }

    public string Wanted { get; private set; } = "";

    public bool Connected { get; private set; }

    public string Endpoint { get; private set; } = "";

    public string? LastError { get; private set; }

    public long Received { get; private set; }

    public long Dropped { get; private set; }

    public void Start(string address = "")
    {
        var wanted = address.Trim();

        if (Enabled)
        {
            if (string.Equals(wanted, Wanted, StringComparison.Ordinal)) return;
            Stop();
        }

        Wanted = wanted;
        _endpoints = Order(wanted);
        Enabled = true;

        var stopping = new CancellationTokenSource();
        _stopping = stopping;
        _worker = Task.Run(() => Run(stopping.Token));
    }

    public void Stop()
    {
        Enabled = false;
        Connected = false;

        var stopping = _stopping;
        _stopping = null;
        _worker = null;
        Wanted = "";

        stopping?.Cancel();
        while (_lines.TryDequeue(out _)) { }
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

    private async Task Run(CancellationToken token)
    {
        var delay = TimeSpan.FromSeconds(2);

        while (!token.IsCancellationRequested)
        {
            foreach (var endpoint in _endpoints)
            {
                if (token.IsCancellationRequested) return;

                try
                {
                    await Listen(endpoint, token);
                    delay = TimeSpan.FromSeconds(2);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Connected = false;
                    LastError = ex.Message;
                }
            }

            try
            {
                await Task.Delay(delay, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            delay = TimeSpan.FromSeconds(Math.Min(30, delay.TotalSeconds * 2));
        }
    }

    private async Task Listen(string endpoint, CancellationToken token)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            LastError = $"{endpoint} does not read as a URL.";
            return;
        }

        using var socket = new ClientWebSocket();
        await socket.ConnectAsync(uri, token);

        Connected = true;
        Endpoint = endpoint;
        LastError = null;

        await socket.SendAsync(
            Encoding.UTF8.GetBytes(Subscribe), WebSocketMessageType.Text, true, token);

        var buffer = new byte[ReceiveBufferBytes];
        var message = new StringBuilder();

        while (!token.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            message.Clear();
            WebSocketReceiveResult result;

            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Connected = false;
                    return;
                }
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            if (token.IsCancellationRequested) return;
            Take(message.ToString());
        }

        Connected = false;
    }

    private void Take(string payload)
    {
        if (payload.Length == 0) return;

        string? line;
        try
        {
            using var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            if (!root.TryGetProperty("type", out var type)) return;
            if (type.GetString() != "LogLine") return;

            line = root.TryGetProperty("rawLine", out var raw)
                ? raw.GetString()
                : Rebuild(root);
        }
        catch (JsonException)
        {
            return;
        }

        if (string.IsNullOrEmpty(line)) return;

        Received++;

        if (_lines.Count >= MaxQueued)
        {
            _lines.TryDequeue(out _);
            Dropped++;
        }

        _lines.Enqueue(line);
    }

    private static string? Rebuild(JsonElement root)
    {
        if (!root.TryGetProperty("line", out var parts) || parts.ValueKind != JsonValueKind.Array)
            return null;

        var fields = new List<string>();
        foreach (var part in parts.EnumerateArray())
            fields.Add(part.ValueKind == JsonValueKind.String ? part.GetString() ?? "" : part.ToString());

        return string.Join('|', fields);
    }

    public void Dispose() => Stop();
}
