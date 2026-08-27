using System.Numerics;
using Dalamud.Configuration;
using FrenRaidTools.Engine;
using FrenRaidTools.Engine.DancingMad;
using FrenRaidTools.Ui;

namespace FrenRaidTools;

public sealed class CallEdit
{
    public string Speech { get; set; } = "";

    public string Text { get; set; } = "";

    public bool Any => Speech.Length > 0 || Text.Length > 0;

    public void Normalize()
    {
        Speech ??= "";
        Text ??= "";
    }
}

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool CallsOn { get; set; } = true;
    public bool OnlyInFight { get; set; } = true;
    public HashSet<string> MutedCalls { get; set; } = new(StringComparer.Ordinal);
    public HashSet<string> SeededQuiet { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, CallEdit> CallEdits { get; set; } = new(StringComparer.Ordinal);

    public CallEdit? EditFor(string key) =>
        key.Length > 0 && CallEdits.TryGetValue(key, out var edit) && edit.Any ? edit : null;

    public CallEdit EditSlot(string key)
    {
        if (CallEdits.TryGetValue(key, out var edit)) return edit;
        edit = new CallEdit();
        CallEdits[key] = edit;
        return edit;
    }

    public void DropEdit(string key) => CallEdits.Remove(key);

    public void CarryRenamedCalls()
    {
        if (CallKeys.Carry(MutedCalls) + CallKeys.Carry(CallEdits) > 0) _dirty = true;
    }

    public bool DiagOn { get; set; }
    public bool DiagInReplay { get; set; }

    public bool ParserOn { get; set; } = true;
    public string ParserAddress { get; set; } = "ws://127.0.0.1:10501/ws";
    public bool ParserPreferIinact { get; set; } = true;

    public bool OverlayOn { get; set; } = true;
    public bool OverlayLocked { get; set; } = true;
    public bool OverlayBackground { get; set; } = true;
    public Vector4 OverlayBackgroundColor { get; set; } = new(0.04f, 0.04f, 0.04f, 0.64f);
    public Vector4 OverlayTextColor { get; set; } = new(1f, 1f, 1f, 1f);
    public Vector2 OverlayPosition { get; set; } = new(0.5f, 0.24f);
    public float OverlayTextScale { get; set; } = 1.7f;
    public int OverlayFontPx { get; set; }
    public int OverlayMaxLines { get; set; } = 3;
    public bool OverlayOutline { get; set; } = true;
    public bool OverlayCountdown { get; set; } = true;
    public bool OverlayIcons { get; set; } = true;
    public float OverlayLingerScale { get; set; } = 1f;
    public int OverlayAlign { get; set; } = 1;
    public float OverlayPadding { get; set; } = 14f;
    public float OverlayRounding { get; set; } = 8f;
    public float OverlayLineGap { get; set; } = 4f;
    public bool OverlayNewestOnTop { get; set; }

    public bool TtsOn { get; set; } = true;
    public string TtsVoice { get; set; } = "";
    public int TtsRate { get; set; } = 1;
    public int TtsVolume { get; set; } = 90;
    public float TtsMinGap { get; set; } = 0.4f;
    public bool TtsOnlyInFight { get; set; } = true;

    [Newtonsoft.Json.JsonProperty(ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace)]
    public List<Roster> Setups { get; set; } = [];
    public int ActiveSetup { get; set; }
    public bool FillRolesOnJoin { get; set; } = true;

    public CleanseCalls CleanseCallMode { get; set; } = CleanseCalls.PriorSet;
    public bool DoubleTowerOnlyWithNoDebuff { get; set; }

    public Dictionary<string, StrategyPick> Plans { get; set; } = new(StringComparer.Ordinal);
    public string PlanFight { get; set; } = "";
    public bool SeatFromRoles { get; set; } = true;

    [Newtonsoft.Json.JsonProperty("Plan")]
    public StrategyPick? PlanBeforeFights { get; set; }

    public StrategyPick PlanFor(string fightKey)
    {
        if (Plans.TryGetValue(fightKey, out var pick)) return pick;
        pick = new StrategyPick();
        Plans[fightKey] = pick;
        return pick;
    }

    public int Skin { get; set; }
    public uint AccentColor { get; set; } = Theme.DefaultAccent;
    public float UiScale { get; set; } = 1f;
    public bool Colorblind { get; set; }

    private bool _dirty;
    private double _dirtySince;

    public Roster Roles
    {
        get
        {
            if (Setups.Count == 0) Setups.Add(new Roster());
            ActiveSetup = Math.Clamp(ActiveSetup, 0, Setups.Count - 1);
            var roster = Setups[ActiveSetup];
            roster.Normalize();
            return roster;
        }
    }

    public void Normalize()
    {
        MutedCalls ??= new HashSet<string>(StringComparer.Ordinal);
        SeededQuiet ??= new HashSet<string>(StringComparer.Ordinal);
        CallEdits ??= new Dictionary<string, CallEdit>(StringComparer.Ordinal);
        CarryRenamedCalls();
        foreach (var edit in CallEdits.Values) edit.Normalize();
        ParserAddress = string.IsNullOrWhiteSpace(ParserAddress) ? "ws://127.0.0.1:10501/ws" : ParserAddress;
        Plans ??= new Dictionary<string, StrategyPick>(StringComparer.Ordinal);
        if (PlanBeforeFights is not null)
        {
            Plans.TryAdd(FightPlans.First.Key, PlanBeforeFights);
            PlanBeforeFights = null;
        }
        foreach (var pick in Plans.Values)
            pick.Options ??= new Dictionary<string, string>(StringComparer.Ordinal);
        if (FightPlans.ByKey(PlanFight) is null) PlanFight = FightPlans.First.Key;
        Setups ??= [];
        foreach (var setup in Setups) setup.Normalize();
        DropSpareBlankSetups();
        if (Setups.Count == 0) Setups.Add(new Roster());
        ActiveSetup = Math.Clamp(ActiveSetup, 0, Setups.Count - 1);
        UiScale = Math.Clamp(UiScale, 0.8f, 1.6f);
        OverlayTextScale = Math.Clamp(OverlayTextScale, 0.8f, 4f);
        if (OverlayFontPx <= 0) OverlayFontPx = Ui.Fonts.Snap(OverlayTextScale * BaseTextPx);
        OverlayFontPx = Ui.Fonts.Snap(OverlayFontPx);
        OverlayMaxLines = Math.Clamp(OverlayMaxLines, 1, 8);
        OverlayLingerScale = Math.Clamp(OverlayLingerScale, 0.4f, 3f);
        OverlayAlign = Math.Clamp(OverlayAlign, 0, 2);
        OverlayPadding = Math.Clamp(OverlayPadding, 0f, 40f);
        OverlayRounding = Math.Clamp(OverlayRounding, 0f, 20f);
        OverlayLineGap = Math.Clamp(OverlayLineGap, 0f, 24f);
        OverlayPosition = new Vector2(
            Math.Clamp(OverlayPosition.X, 0.02f, 0.98f),
            Math.Clamp(OverlayPosition.Y, 0.01f, 0.97f));
        TtsRate = Math.Clamp(TtsRate, -10, 10);
        TtsVolume = Math.Clamp(TtsVolume, 0, 100);
        TtsMinGap = Math.Clamp(TtsMinGap, 0f, 5f);
        TtsVoice ??= "";
        Skin = Math.Clamp(Skin, 0, Theme.SkinNames.Length - 1);
        if (AccentColor >> 24 == 0) AccentColor = Theme.DefaultAccent;
    }

    private void DropSpareBlankSetups()
    {
        var before = Setups.Count;
        ActiveSetup = Roster.DropSpares(Setups, ActiveSetup);
        if (Setups.Count != before) _dirty = true;
    }

    public void Save(double now)
    {
        if (!_dirty) _dirtySince = now;
        _dirty = true;
    }

    public void Flush(double now, bool force = false)
    {
        if (!_dirty) return;
        if (!force && now - _dirtySince < SaveDelaySeconds) return;
        _dirty = false;
        Service.PluginInterface.SavePluginConfig(this);
    }

    private const double SaveDelaySeconds = 0.6;

    private const float BaseTextPx = 17f;
}
