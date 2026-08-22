namespace FrenRaidTools.Engine.DancingMad;

public sealed class Celestriad
{
    public const string Group = "celestriad";

    public const int PhaseNumber = 5;

    public const string MechanicName = "Celestriad";

    public const uint CelestriadCast = 0xBB42;
    public const uint FireDown = 0xB56;
    public const uint LightningDown = 0xBB6;
    public const uint IceDown = 0xB57;

    public const uint TowerControl = 0x19D;
    public const uint TowerArg1 = 0x10;
    public const uint TowerArg2 = 0x20;

    public const uint FireTowerNpc = 2015294;
    public const uint IceTowerNpc = 2015295;
    public const uint ThunderTowerNpc = 2015296;

    public const uint CataOut = 0xC24E;
    public const uint CataIn = 0xC24F;

    public const int TowerSets = 3;

    public bool DoubleTowerOnlyWithNoDebuff { get; set; }

    public readonly Callout celestriad =
        Callout.Duration("Celestriad", "Elemental Towers");

    public readonly Callout celestriadFireResDown =
        Callout.Duration("Celestriad: Fire Res Down", "Ice and Lightning, Fire Last").AutoIcon();

    public readonly Callout celestriadLightningResDown =
        Callout.Duration("Celestriad: Lightning Res Down", "Fire and Ice, Lightning Last").AutoIcon();

    public readonly Callout celestriadIceResDown =
        Callout.Duration("Celestriad: Ice Res Down", "Fire and Lightning, Ice Last").AutoIcon();

    public readonly Callout celestriadNoResDown =
        Callout.Of("Celestriad: No Res Down", "No Debuff");

    public readonly Callout celestriadIn =
        Callout.Duration("Celestriad: Catastrophic Choice (In)", "In");

    public readonly Callout celestriadOut =
        Callout.Duration("Celestriad: Catastrophic Choice (Out)", "Out");

    public readonly Callout doubleFire =
        Callout.Of("Celestriad: Double Fire Tower", "Double Fire").Note("You can use the setting 'Double Tower Call only when no debuff' to make this (and the ice/lightning equivalents) only call when you have no initial debuff (on the Settings tab above).");

    public readonly Callout doubleIce =
        Callout.Of("Celestriad: Double Ice Tower", "Double Ice");

    public readonly Callout doubleLightning =
        Callout.Of("Celestriad: Double Lightning Tower", "Double Lightning");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, CelestriadCast),
            (start, run) => Run(start, run, world));

    public Sequence BuildCatastrophic(IWorld world) =>
        Sequence.Repeat(Group + "Cata", 180, e => e.Is(EventKind.CastStart, CelestriadCast),
            async (start, run) =>
            {
                for (var i = 0; i < 2; i++)
                {
                    var cata = await run.WaitEvent(EventKind.CastStart, CataOut, CataIn);
                    run.Call(cata.Id == CataOut ? celestriadOut : celestriadIn, cata);
                }
            });

    private static bool IsTowerLight(GameEvent e) =>
        e.Kind == EventKind.ActorControl && e.Id == TowerControl &&
        e.Arg1 == TowerArg1 && e.Arg2 == TowerArg2 && e.Arg3 == 0 && e.Arg4 == 0;

    public static uint DoubleTowerColour(IEnumerable<GameEvent> towers)
    {
        var counts = new Dictionary<uint, int>();
        foreach (var tower in towers)
        {
            var id = tower.Target?.BaseId ?? 0;
            if (id == 0) continue;
            counts[id] = counts.GetValueOrDefault(id) + 1;
        }

        foreach (var colour in new[] { FireTowerNpc, IceTowerNpc, ThunderTowerNpc })
            if (counts.GetValueOrDefault(colour) == 2)
                return colour;

        return 0;
    }

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(celestriad, start);

        await run.WaitEventsQuickSuccession(
            6, e => e.Kind == EventKind.StatusGain && e.Id is FireDown or LightningDown or IceDown);

        var myFire = Mine(world, FireDown);
        var myLightning = Mine(world, LightningDown);
        var myIce = Mine(world, IceDown);

        var noDebuff = false;
        if (myFire is not null) run.Call(celestriadFireResDown, myFire);
        else if (myLightning is not null) run.Call(celestriadLightningResDown, myLightning);
        else if (myIce is not null) run.Call(celestriadIceResDown, myIce);
        else
        {
            run.Call(celestriadNoResDown);
            noDebuff = true;
        }

        run.SetParam("noDebuff", noDebuff);

        var shouldCall = noDebuff || !DoubleTowerOnlyWithNoDebuff;

        for (var i = 0; i < TowerSets; i++)
        {
            var towers = await run.WaitEventsQuickSuccession(4, IsTowerLight);
            var colour = DoubleTowerColour(towers);
            if (colour == 0 || !shouldCall) continue;

            run.Call(colour switch
            {
                FireTowerNpc => doubleFire,
                IceTowerNpc => doubleIce,
                _ => doubleLightning,
            });
        }
    }

    private static GameEvent? Mine(IWorld world, uint statusId) =>
        world.ActiveStatuses().FirstOrDefault(s => s.Id == statusId && s.Target?.IsYou == true);
}
