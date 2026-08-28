using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FrenRaidTools.Engine;
using FrenRaidTools.Ui;

namespace FrenRaidTools;

public sealed class Plugin : IDalamudPlugin
{
    private const string Command = "/frt";
    private const string CommandAlias = "/fart";

    public readonly WindowSystem Windows = new("FrenRaidTools");

    public Configuration Config { get; }
    public Speech Speech { get; }
    public CallBoard Board { get; }
    public Fight Fight { get; }
    public ParserLink Parser { get; }
    public Runtime Runtime { get; }
    public Fonts Fonts { get; }

    public Diag Diag { get; } = new();
    public MainWindow MainWindow { get; }
    public OverlayWindow Overlay { get; }
    public EntryWindow Entry { get; }
    public PartyWindow Roles { get; }

    public string Version { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";

    private double _clock;
    private double _fightClock;
    private uint _lastZone;

    public double Now => _clock;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Config = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Normalize();

        Speech = new Speech();
        Board = new CallBoard(Config, Speech, Diag);
        Board.StatusRemaining = Game.StatusRemaining;

        Fight = new Fight();
        Board.SetCatalog(Fight.Catalog);
        Parser = new ParserLink(Config);
        Runtime = new Runtime(Config, Board, Fight, Diag);
        Fonts = new Fonts();

        Diag.Describe = Runtime.Setup;
        Board.FightNow = () => _fightClock;

        MainWindow = new MainWindow(this);
        Overlay = new OverlayWindow(this);
        Entry = new EntryWindow(this);
        Roles = new PartyWindow(this);
        Windows.AddWindow(MainWindow);
        Windows.AddWindow(Overlay);
        Windows.AddWindow(Entry);
        Windows.AddWindow(Roles);

        Service.CommandManager.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open Fren Raid Tools. /frt roles jumps to roles, /frt p opens the party list.",
        });

        Service.CommandManager.AddHandler(CommandAlias, new CommandInfo(OnCommand)
        {
            HelpMessage = "Same thing, more fun to type.",
        });

        Service.PluginInterface.UiBuilder.Draw += Windows.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += Open;
        Service.PluginInterface.UiBuilder.OpenMainUi += Open;
        Service.Framework.Update += OnUpdate;
        Service.ClientState.TerritoryChanged += OnZoneChanged;

        _lastZone = Service.ClientState.TerritoryType;

        Service.Log.Information("Fren Raid Tools loaded.");
    }

    private void AddStratSpots()
    {
        if (Fight.PlanReady) return;

        var planned = FightPlans.ByKey(Config.PlanFight) ?? FightPlans.First;
        var asset = PlanSource.Asset(planned);
        if (asset is null) return;

        if (!Fight.UsePlan(asset, () => PlanSource.Book(planned, Config.PlanFor(planned.Key)))) return;

        Board.PlanCarriesItsOwnCalls();
        Board.SetCatalog(Fight.Catalog);
    }

    private void OnUpdate(Dalamud.Plugin.Services.IFramework framework)
    {
        var delta = framework.UpdateDelta.TotalSeconds;
        _clock += delta;
        _fightClock += Game.InReplay ? delta * Game.Speed() : delta;

        try
        {
            Fonts.Warm(Config.OverlayFontPx);
            Diag.Tick(_fightClock);
            Board.Tick(_clock);
            AddStratSpots();
            ResumeDiag();
            FillRoles();
            Parser.Tick(_clock);
            Runtime.Tick(_fightClock);
            Fonts.Tick();
            SeatSync.Apply(Config, _clock);
            Config.Flush(_clock);
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Update failed.");
        }
    }

    private void OnZoneChanged(uint zone)
    {
        if (zone == _lastZone) return;
        _lastZone = zone;

        Board.Clear();
        Runtime.Wipe();
        SeatSync.Reset();
        RosterGlance.Reset();
        ArmRoleFill();

        if (Config.AskOnEntry && zone == EngineInfo.DancingMadTerritory) Entry.Open();
    }

    private bool _diagResumed;

    private void ResumeDiag()
    {
        if (_diagResumed) return;
        _diagResumed = true;

        if (Config.DiagOn && !Diag.On) Diag.Start();
        ArmRoleFill();
    }

    public const double RoleFillWindowSeconds = 60.0;
    public const double RoleFillEverySeconds = 1.0;

    private double _fillUntil;
    private double _nextFill;
    private int _filled;
    private bool _wasReplaying;

    private void ArmRoleFill()
    {
        _fillUntil = _clock + RoleFillWindowSeconds;
        _nextFill = 0;
        _filled = 0;
    }

    private void FillRoles()
    {
        if (Runtime.Replaying != _wasReplaying)
        {
            _wasReplaying = Runtime.Replaying;
            ArmRoleFill();
        }

        if (_fillUntil <= 0) return;

        if (_clock >= _fillUntil)
        {
            Done();
            return;
        }

        if (_clock < _nextFill) return;
        _nextFill = _clock + RoleFillEverySeconds;

        if (!Config.FillRolesOnJoin || Game.Zone != EngineInfo.DancingMadTerritory)
        {
            Done();
            return;
        }

        try
        {
            var members = Party.Read();
            if (members.Count == 0) return;

            _filled += Config.Roles.Fill(members, keepExisting: true, Config.JobSpots, Party.YouName());
            if (Config.Roles.Filled >= Slots.Count || members.Count >= Party.Max) Done();
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Role fill failed.");
            Done();
        }
    }

    private void Done()
    {
        _fillUntil = 0;
        if (_filled <= 0) return;

        Board.Note($"Filled {_filled} role spots from your party.");
        Config.Save(_clock);
        _filled = 0;
    }

    private static readonly Callout Sample = Callout
        .Of("Test line", "Stack north, tank buster", "Stack north, tank buster")
        .Linger(4.0);

    public void FireSample() => Board.Test(Sample with { Key = "frt.sample" });

    private void OnCommand(string command, string args)
    {
        var word = args.Trim().ToLowerInvariant();

        if (word is "p" or "party" or "list")
        {
            Roles.Open();
            return;
        }

        var page = word switch
        {
            "roles" or "role" => MainWindow.Nav.Roles,
            "calls" or "call" => MainWindow.Nav.Calls,
            "strat" or "strats" => MainWindow.Nav.Strats,
            "overlay" => MainWindow.Nav.Overlay,
            "voice" or "tts" => MainWindow.Nav.Voice,
            "parser" or "act" or "iinact" => MainWindow.Nav.Parser,
            _ => MainWindow.Nav.Status,
        };

        MainWindow.Show(page);
    }

    private void Open() => MainWindow.IsOpen = true;

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= Windows.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= Open;
        Service.PluginInterface.UiBuilder.OpenMainUi -= Open;
        Service.Framework.Update -= OnUpdate;
        Service.ClientState.TerritoryChanged -= OnZoneChanged;
        Service.CommandManager.RemoveHandler(Command);
        Service.CommandManager.RemoveHandler(CommandAlias);

        Config.Flush(_clock, force: true);
        Ui.Icons.Forget();
        Windows.RemoveAllWindows();
        Diag.Dispose();
        Runtime.Dispose();
        Parser.Dispose();
        Speech.Dispose();
        Fonts.Dispose();
    }
}
