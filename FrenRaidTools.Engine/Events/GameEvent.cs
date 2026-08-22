namespace FrenRaidTools.Engine;

public enum EventKind
{
    Unknown = 0,
    CastStart,
    AbilityHit,
    HeadMarker,
    Tether,
    StatusGain,
    StatusLose,
    ActorControl,
    StatusLoopVfx,
    ActorMoved,
    ZoneChange,
    CombatStart,
    CombatEnd,
    Synthetic,
}

public static class Synthetic
{
    public const uint ManaChargeDetail = 1;
}

public readonly record struct Position(float X, float Y, float Z)
{
    public static readonly Position Unknown = new(float.NaN, float.NaN, float.NaN);
    public bool Known => !float.IsNaN(X);

    public Position Forward(double heading, double distance) =>
        Known
            ? new Position(
                (float)(X + Math.Sin(heading) * distance),
                (float)(Y + Math.Cos(heading) * distance),
                Z)
            : this;
}

public sealed record Actor
{
    public required uint ObjectId { get; init; }
    public uint BaseId { get; init; }
    public uint NameId { get; init; }
    public string Name { get; init; } = "";
    public bool IsPlayer { get; init; }
    public bool IsYou { get; init; }
    public Position Pos { get; init; } = Position.Unknown;
    public float Heading { get; init; }
    public string Job { get; init; } = "";

    public bool Support => JobRoles.IsSupport(Job);

    public override string ToString() => Name;
}

public sealed record GameEvent
{
    public required EventKind Kind { get; init; }

    public required double At { get; init; }

    public Actor? Source { get; init; }
    public Actor? Target { get; init; }

    public uint Id { get; init; }

    public double Duration { get; init; }

    public byte Stacks { get; init; }

    public uint Arg1 { get; init; }
    public uint Arg2 { get; init; }
    public uint Arg3 { get; init; }
    public uint Arg4 { get; init; }

    public bool FirstTarget { get; init; }

    public bool Is(EventKind kind, params uint[] ids) =>
        Kind == kind && Array.IndexOf(ids, Id) >= 0;

    public bool EitherEnd(Func<Actor, bool> match) =>
        (Source is not null && match(Source)) || (Target is not null && match(Target));

    public Actor? OtherEnd(Func<Actor, bool> match) =>
        Source is not null && match(Source) ? Target
        : Target is not null && match(Target) ? Source
        : null;
}

public static class JobRoles
{
    private static readonly HashSet<string> SupportJobs =
    [
        "PLD", "WAR", "DRK", "GNB",
        "WHM", "SCH", "AST", "SGE",
    ];

    public static bool IsSupport(string job) =>
        !string.IsNullOrEmpty(job) && SupportJobs.Contains(job.ToUpperInvariant());
}
