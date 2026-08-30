using System.Globalization;

namespace FrenRaidTools.Engine;

public enum CallRank
{
    Critical = 0,
    High = 1,
    Normal = 2,
    Low = 3,
    Informational = 4,
}

public sealed record Callout
{
    public string Key { get; init; } = "";

    public int Phase { get; init; }

    public string Mechanic { get; init; } = "";

    public required string Description { get; init; }

    public string Title => Mechanic.Length > 0 ? Mechanic : Description;

    public required string Speech { get; init; }

    public required string Text { get; init; }

    public double LingerSeconds { get; init; } = DefaultLinger;

    public bool FromDuration { get; init; }

    public double CountdownOffsetSeconds { get; init; }

    public double CountdownFromStartSeconds { get; init; }

    public double SpeechDelaySeconds { get; init; }

    public bool OnByDefault { get; init; } = true;

    private readonly CallRank? _rank;

    public CallRank Rank
    {
        get => _rank ?? (OnByDefault ? CallRank.Normal : CallRank.Low);
        init => _rank = value;
    }

    public IReadOnlyList<uint> StatusIcons { get; init; } = [];

    public uint AbilityIcon { get; init; }

    public bool IconFromEvent { get; init; }

    public bool FromPlan { get; init; }

    public string? Notes { get; init; }

    public const double DefaultLinger = 5.0;
    public const double DurationLinger = 3.0;
    public const string CountdownToken = " ({remaining})";

    public const double MaxCountdownHold = 20.0;

    public static bool ShowsNumber(double left) => left <= MaxCountdownHold;

    public static double Expiry(double now, double linger, double countdownEnds, bool ticking)
    {
        if (!ticking) return now + linger;
        if (countdownEnds - now > MaxCountdownHold) return now + linger;

        return Math.Max(now + linger, countdownEnds);
    }

    public double CountdownDuration(double? eventDuration, double fallback = 0.0) =>
        CountdownFromStartSeconds > 0 ? CountdownFromStartSeconds : eventDuration ?? fallback;

    public static double Remaining(double? begunAt, double duration, double fightNow) =>
        begunAt is null ? duration : Math.Max(0, begunAt.Value + duration - fightNow);

    public const double ResyncDeadband = 0.15;

    public static double Resync(double ends, double? reportedEnds) =>
        reportedEnds is { } live && Math.Abs(live - ends) > ResyncDeadband ? live : ends;

    public static Callout Of(string description, string both) =>
        new() { Description = description, Speech = both, Text = both };

    public static Callout Of(string descriptionAndBoth) =>
        Of(descriptionAndBoth, descriptionAndBoth);

    public static Callout Of(string description, string speech, string text) =>
        new() { Description = description, Speech = speech, Text = text };

    public static Callout Duration(string description, string speech, string text) =>
        new()
        {
            Description = description,
            Speech = speech,
            Text = text + CountdownToken,
            FromDuration = true,
            LingerSeconds = DurationLinger,
        };

    public static Callout Duration(string description, string both) =>
        Duration(description, both, both);

    public static Callout Duration(string descriptionAndBoth) =>
        Duration(descriptionAndBoth, descriptionAndBoth);

    public static Callout DurationPlus(string description, string text, double offsetSeconds) =>
        new()
        {
            Description = description,
            Speech = text,
            Text = text + CountdownToken,
            FromDuration = true,
            CountdownOffsetSeconds = offsetSeconds,
            LingerSeconds = DurationLinger + offsetSeconds,
        };

    public Callout Icon(params uint[] statusIds) => this with { StatusIcons = statusIds };

    public Callout Ability(uint abilityId) => this with { AbilityIcon = abilityId };

    public Callout AutoIcon() => this with { IconFromEvent = true };

    public Callout Quiet() => this with { OnByDefault = false };

    public Callout Ranked(CallRank rank) => this with { Rank = rank };

    public Callout Urgent() => Ranked(CallRank.Critical);

    public Callout Note(string notes) => this with { Notes = notes };

    public Callout In(int phase, string mechanic) => this with { Phase = phase, Mechanic = mechanic };

    public Callout Linger(double seconds) => this with { LingerSeconds = seconds };

    public Callout Planned() => this with { FromPlan = true };

    public Callout SpeakAfter(double seconds) => this with { SpeechDelaySeconds = seconds };

    public bool NeedsParams =>
        Text.Contains('{', StringComparison.Ordinal) ||
        Speech.Contains('{', StringComparison.Ordinal);

    public static string Seconds(double value) =>
        value.ToString("0.#", CultureInfo.InvariantCulture);
}
