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
        return found.Count > 1 || !(Game.InDuty || Game.InReplay) ? found : FromDuty(found);
    }

    private static List<PartyMember> FromDuty(List<PartyMember> found)
    {
        try
        {
            foreach (var obj in Service.ObjectTable)
            {
                if (found.Count >= Max) break;
                if (obj.ObjectKind != ObjectKind.Pc) continue;

                var name = obj.Name.TextValue;
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (found.Any(m => string.Equals(m.Name, name, StringComparison.Ordinal))) continue;

                var job = obj is ICharacter character ? JobKinds.Abbr(character.ClassJob.RowId) : "";
                found.Add(new PartyMember(name, job));
            }
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Duty party read failed.");
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
