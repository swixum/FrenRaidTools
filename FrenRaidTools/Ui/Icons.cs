using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using FrenRaidTools.Engine;
using Lumina.Excel.Sheets;

namespace FrenRaidTools.Ui;

public static class Icons
{
    public const int MaxRemembered = 256;
    public const int MaxPerCall = 4;

    private static readonly Dictionary<uint, uint> ByStatus = [];
    private static readonly Dictionary<uint, uint> ByAction = [];

    public static void Forget()
    {
        ByStatus.Clear();
        ByAction.Clear();
    }

    public static uint ForStatus(uint statusId)
    {
        if (statusId == 0) return 0;
        if (ByStatus.TryGetValue(statusId, out var known)) return known;
        if (ByStatus.Count >= MaxRemembered) ByStatus.Clear();

        var icon = Service.DataManager.GetExcelSheet<Status>()?.GetRowOrDefault(statusId)?.Icon ?? 0;
        ByStatus[statusId] = icon;
        return icon;
    }

    public static uint ForAction(uint actionId)
    {
        if (actionId == 0) return 0;
        if (ByAction.TryGetValue(actionId, out var known)) return known;
        if (ByAction.Count >= MaxRemembered) ByAction.Clear();

        var icon = (uint)(Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()
            ?.GetRowOrDefault(actionId)?.Icon ?? 0);
        ByAction[actionId] = icon;
        return icon;
    }

    public static IReadOnlyList<uint> For(Callout callout, GameEvent? on)
    {
        var ids = new List<uint>(MaxPerCall);

        foreach (var status in callout.StatusIcons) Keep(ids, ForStatus(status));
        Keep(ids, ForAction(callout.AbilityIcon));
        if (callout.IconFromEvent && on is not null) Keep(ids, FromEvent(on));

        return ids;
    }

    private static void Keep(List<uint> ids, uint icon)
    {
        if (icon == 0 || ids.Count >= MaxPerCall) return;
        ids.Add(icon);
    }

    private static uint FromEvent(GameEvent on) => on.Kind switch
    {
        EventKind.StatusGain or EventKind.StatusLose => ForStatus(on.Id),
        EventKind.CastStart or EventKind.AbilityHit => ForAction(on.Id),
        _ => 0,
    };

    public static IDalamudTextureWrap? Texture(uint icon) =>
        icon == 0 ? null : Service.Textures.GetFromGameIcon(new GameIconLookup(icon)).GetWrapOrDefault();
}
