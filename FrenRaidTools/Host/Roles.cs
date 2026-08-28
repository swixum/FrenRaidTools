using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FrenRaidTools.Engine;

namespace FrenRaidTools;

public static class Party
{
    public const int Max = 8;

    public static List<PartyMember> Read()
    {
        var found = FromPartyList();
        if (found.Count > 1) return found;

        found = FromCrossRealm(found);
        if (found.Count > 1) return found;

        return Game.InDuty || Game.InReplay ? FromWorld(found, Max) : found;
    }

    public static string Source()
    {
        var list = FromPartyList();
        if (list.Count > 1) return $"party list, {list.Count}";

        var cross = FromCrossRealm([.. list]);
        if (cross.Count > 1) return $"cross-world party, {cross.Count}";

        if (!(Game.InDuty || Game.InReplay)) return "no party";

        var here = FromWorld([.. cross], Max);
        return here.Count > cross.Count ? $"duty zone, {here.Count}" : "no party";
    }

    private static unsafe List<PartyMember> FromCrossRealm(List<PartyMember> found)
    {
        try
        {
            var proxy = FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyCrossRealm.Instance();
            if (proxy == null) return found;
            if (!FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyCrossRealm.IsCrossRealmParty())
                return found;

            var group = (int)proxy->LocalPlayerGroupIndex;
            var count = FFXIVClientStructs.FFXIV.Client.UI.Info
                .InfoProxyCrossRealm.GetGroupMemberCount(group);

            for (uint i = 0; i < count && found.Count < Max; i++)
            {
                var member = FFXIVClientStructs.FFXIV.Client.UI.Info
                    .InfoProxyCrossRealm.GetGroupMember(i, group);
                if (member == null) continue;

                var name = member->NameString;
                if (PartyPool.Holds(found, name)) continue;

                found.Add(new PartyMember(name, JobKinds.Abbr(member->ClassJobId)));
            }
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Cross-world party read failed.");
        }

        return found;
    }

    private static List<PartyMember> FromWorld(List<PartyMember> found, int limit)
    {
        try
        {
            var you = Game.You?.Position;

            var players = new List<(float Away, PartyMember Member)>();

            foreach (var obj in Service.ObjectTable)
            {
                if (obj.ObjectKind != ObjectKind.Pc) continue;

                var name = obj.Name.TextValue;
                if (PartyPool.Holds(found, name)) continue;
                if (players.Any(p =>
                        string.Equals(p.Member.Name, name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var job = obj is ICharacter character ? JobKinds.Abbr(character.ClassJob.RowId) : "";
                var away = you is { } spot ? Vector3.Distance(spot, obj.Position) : 0f;
                players.Add((away, new PartyMember(name, job)));
            }

            PartyPool.Take(found, players, limit);
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Nearby player read failed.");
        }

        return found;
    }

    private static List<PartyMember> FromPartyList()
    {
        var found = new List<PartyMember>(Max);

        try
        {
            foreach (var member in Service.PartyList)
            {
                var name = member.Name.TextValue;
                if (string.IsNullOrWhiteSpace(name)) continue;
                found.Add(new PartyMember(name, JobKinds.Abbr(member.ClassJob.RowId)));
            }

            if (Game.You is { } you)
            {
                var name = you.Name.TextValue;
                if (!string.IsNullOrWhiteSpace(name)
                    && !found.Any(m => string.Equals(m.Name, name, StringComparison.Ordinal)))
                    found.Add(new PartyMember(name, JobKinds.Abbr(you.ClassJob.RowId)));
            }
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Party read failed.");
        }

        return found;
    }

    public static string YouName()
    {
        try
        {
            return Game.You?.Name.TextValue ?? "";
        }
        catch
        {
            return "";
        }
    }
}

public static class JobLook
{
    public static uint Color(string abbr) => JobKinds.Kind(abbr) switch
    {
        JobKind.Tank => 0xFFFF9A4C,
        JobKind.Healer => 0xFF9BD64F,
        JobKind.Melee => 0xFF6B6BFF,
        JobKind.PhysRanged => 0xFF7BC2E8,
        JobKind.Caster => 0xFFC77BE8,
        _ => Ui.Theme.Muted,
    };

    public static uint SlotColor(int slot) => Slots.RoleOf(slot) switch
    {
        SlotRole.Tank => 0xFFFF9A4C,
        SlotRole.Healer => 0xFF9BD64F,
        SlotRole.Melee => 0xFF6B6BFF,
        _ => 0xFFC77BE8,
    };
}
