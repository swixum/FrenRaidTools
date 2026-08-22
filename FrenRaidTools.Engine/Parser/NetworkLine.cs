using System.Globalization;

namespace FrenRaidTools.Engine;

public static class NetworkLine
{
    public const int CastStart = 20;
    public const int Ability = 21;
    public const int AreaAbility = 22;
    public const int StatusAdd = 26;
    public const int HeadMarker = 27;
    public const int StatusRemove = 30;
    public const int Tether = 35;
    public const int AddCombatant = 3;
    public const int RemoveCombatant = 4;
    public const int StatusEffects = 38;
    public const int CombatantMemory = 261;
    public const int ActorMove = 270;
    public const int ActorSetPos = 271;
    public const int ActorControlExtra = 273;

    public const int AbilityTargetIndexField = 45;
    public const uint LoopVfxStatus = 0x808;
    public const int AbilitySourcePosField = 40;
    public const int AbilityTargetPosField = 30;
    public const int CastSourcePosField = 9;
    public const int StatusEffectsPosField = 11;
    public const int MemoryPairsField = 4;

    public static string[] Split(string line) => line.Split('|');

    public static int Kind(string[] fields) =>
        fields.Length > 0 && int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var kind)
            ? kind
            : -1;

    public static uint Hex(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length) return 0;
        var text = fields[index];
        if (text.Length == 0) return 0;
        return uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    public static uint Decimal(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length) return 0;
        var text = fields[index];
        if (text.Length == 0) return 0;
        return uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    public static double Number(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length) return 0;
        var text = fields[index];
        if (text.Length == 0) return 0;
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    public static string Text(string[] fields, int index) =>
        index >= 0 && index < fields.Length ? fields[index] : "";

    public static bool Has(string[] fields, int index) =>
        index >= 0 && index < fields.Length && fields[index].Length > 0;

    public static bool HasRun(string[] fields, int index, int count)
    {
        for (var i = index; i < index + count; i++)
            if (!Has(fields, i)) return false;
        return true;
    }

    public static double Stamp(string[] fields)
    {
        if (fields.Length < 2) return double.NaN;

        return DateTimeOffset.TryParse(
            fields[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out var when)
            ? when.ToUnixTimeMilliseconds() / 1000.0
            : double.NaN;
    }

    public static bool IsPlayerId(uint id) => (id & 0xF0000000) == 0x10000000;
}
