using Dalamud.Game.ClientState.Conditions;
using FrenRaidTools.Engine;

namespace FrenRaidTools;

public static class Game
{
    public static uint Zone => Service.ClientState.TerritoryType;

    public static Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter? You =>
        Service.ObjectTable?[0] as Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter;

    public static bool InFight => Service.Condition[ConditionFlag.InCombat];

    public static bool InDuty =>
        Service.Condition[ConditionFlag.BoundByDuty]
        || Service.Condition[ConditionFlag.BoundByDuty56]
        || Service.Condition[ConditionFlag.BoundByDuty95];

    public static bool InTheFight => Zone == EngineInfo.DancingMadTerritory;

    public static bool InReplay => Service.Condition[ConditionFlag.DutyRecorderPlayback];

    public static bool Fighting => InFight || InReplay;

    public static bool PartyFighting()
    {
        if (InFight) return true;

        try
        {
            foreach (var member in Service.PartyList)
                if (member?.GameObject is Dalamud.Game.ClientState.Objects.Types.IBattleChara fighter
                    && fighter.StatusFlags.HasFlag(
                        Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat))
                    return true;
        }
        catch
        {
            return InFight;
        }

        return false;
    }

    public const float NormalSpeed = 1f;
    public const float PausedBelow = 0.02f;
    public const float SpeedCeiling = 100f;

    public static unsafe float Speed()
    {
        try
        {
            if (InReplay)
            {
                var replay = FFXIVClientStructs.FFXIV.Client.Game.ContentsReplayManager.Instance();
                if (replay != null)
                {
                    var pace = replay->PlaybackSpeed;
                    if (pace >= 0f && pace <= SpeedCeiling)
                        return pace < PausedBelow ? 0f : pace;
                }
            }

            var framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
            if (framework == null) return NormalSpeed;

            var speed = framework->GameSpeedMultiplier;
            if (speed < 0f || speed > SpeedCeiling) return NormalSpeed;

            return speed < PausedBelow ? 0f : speed;
        }
        catch
        {
            return NormalSpeed;
        }
    }

    public static bool LoggedIn => Service.ClientState.IsLoggedIn;

    public static double? StatusRemaining(uint actorId, uint statusId)
    {
        try
        {
            if (actorId == 0 || statusId == 0) return null;
            if (Service.ObjectTable?.SearchByEntityId(actorId)
                is not Dalamud.Game.ClientState.Objects.Types.IBattleChara fighter) return null;

            foreach (var status in fighter.StatusList)
            {
                if (status is null || status.StatusId != statusId) continue;
                var left = status.RemainingTime;
                return left > 0f ? left : null;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public static string ZoneName()
    {
        var zone = Zone;
        if (zone == EngineInfo.DancingMadTerritory) return "Dancing Mad";

        try
        {
            var row = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.TerritoryType>()
                ?.GetRowOrDefault(zone);
            var name = row?.PlaceName.ValueNullable?.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch
        {
            return $"Zone {zone}";
        }

        return $"Zone {zone}";
    }
}
