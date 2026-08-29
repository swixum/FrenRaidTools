namespace FrenRaidTools.Engine;

public enum StallReason
{
    Timeout,
    Reset,
}

public sealed record SequenceStall
{
    public required string Name { get; init; }

    public required StallReason Reason { get; init; }

    public required double StartedAt { get; init; }

    public required double EndedAt { get; init; }

    public required int Calls { get; init; }

    public required string Awaiting { get; init; }

    public required double AwaitingFor { get; init; }

    public double Ran => EndedAt - StartedAt;

    public string Line() =>
        $"{Name} {Reason.ToString().ToLowerInvariant()} after {Ran:0.0}s, " +
        $"{Calls} call{(Calls == 1 ? "" : "s")} made, " +
        (Awaiting.Length > 0 ? $"waiting on {Awaiting} for {AwaitingFor:0.0}s" : "not waiting");
}
