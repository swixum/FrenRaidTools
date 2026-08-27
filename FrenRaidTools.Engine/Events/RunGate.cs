namespace FrenRaidTools.Engine;

public sealed class RunGate
{
    public bool Installed { get; set; }

    public ushort Zone { get; set; }

    public ushort FightZone { get; set; } = EngineInfo.DancingMadTerritory;

    public bool Replaying { get; set; }

    public bool ParserOn { get; set; }

    public bool ParserLive { get; set; }

    public bool HooksBroken { get; set; }

    public bool SourceIpcOnly { get; set; }

    public bool SourceSocketOnly { get; set; }

    public bool IpcFeeding { get; set; }

    public bool InTheFight => Zone == FightZone;

    public bool Running => Installed && InTheFight;

    public bool WantsIpc => ParserOn && !SourceSocketOnly;

    public bool WantsSocket => ParserOn && !SourceIpcOnly && !IpcFeeding;

    public bool ClientReadsActors => Running && (Replaying || !ParserLive);

    public bool ClientOwnsEverything => Replaying && !HooksBroken;
}
