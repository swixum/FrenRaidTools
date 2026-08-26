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
        Callout.Duration("Celestriad");

    public readonly Callout celestriadFireResDown =
        Callout.Duration("Celestriad: Fire Res Down", "Ice and Lightning, Fire Last").AutoIcon();

    public readonly Callout celestriadLightningResDown =
        Callout.Duration("Celestriad: Lightning Res Down", "Fire and Ice, Lightning Last").AutoIcon();

    public readonly Callout celestriadIceResDown =
        Callout.Duration("Celestriad: Ice Res Down", "Fire and Lightning, Ice Last").AutoIcon();

    public const string OrderParam = "elementOrder";
    public const string TakeParam = "takeElement";
    public const string ClockParam = "takeClock";
    public const string StepsParam = "takeSteps";
    public const string TurnParam = "takeTurn";
    public const string NthParam = "takeNth";
    public const string TurnSpokenParam = "takeTurnSpoken";
    public const string NextParam = "nextElement";

    public const int RingTowers = 9;
    public const double RingStepDegrees = 360.0 / RingTowers;
    public const int ClockHours = 12;
    public const string OrderSeparator = ", ";

    public readonly Callout celestriadFirstTower =
        Callout.Of("Celestriad: First Tower", "{takeClock} o'clock {takeElement}, then {nextElement}")
            .Note("Names the tower you take first, by the hour it sits on, then the one after it. Nine towers stand 40 degrees apart and an hour covers 30, so two towers can never share an hour, which is why this is an hour and not a compass point. Your resistance down is always last. Use {elementOrder} if you would rather hear all three at once.");

    public readonly Callout celestriadNextTower =
        Callout.Of("Celestriad: Next Tower",
            "{takeNth} {takeTurnSpoken}, {takeElement}",
            "{takeNth} {takeTurn}, {takeElement}")
            .Note("Counts towers of the element you are about to take, walking from the tower you stand on: 1st CW is the next one of that colour clockwise, 2nd CCW the one past the first going the other way. The direction is the shorter way round, measured from where the towers actually are. {takeSteps} still carries the raw tower-spot count if you would rather hear that.");

    public readonly Callout celestriadNoResDown =
        Callout.Of("Celestriad: No Res Down", "No Debuff");

    public readonly Callout celestriadIn =
        Callout.Duration("Celestriad: Catastrophic Choice (In)", "In");

    public readonly Callout celestriadOut =
        Callout.Duration("Celestriad: Catastrophic Choice (Out)", "Out");

    public readonly Callout doubleFire =
        Callout.Of("Celestriad: Double Fire Tower", "Double Fire").Quiet().Note("You can use the setting 'Double Tower Call only when no debuff' to make this (and the ice/lightning equivalents) only call when you have no initial debuff (on the Settings tab above).");

    public readonly Callout doubleIce =
        Callout.Of("Celestriad: Double Ice Tower", "Double Ice").Quiet();

    public readonly Callout doubleLightning =
        Callout.Of("Celestriad: Double Lightning Tower", "Double Lightning").Quiet();

    public static uint TowerFor(uint statusId) => statusId switch
    {
        FireDown => FireTowerNpc,
        IceDown => IceTowerNpc,
        LightningDown => ThunderTowerNpc,
        _ => 0,
    };

    public static string? Element(uint towerNpc) => towerNpc switch
    {
        FireTowerNpc => "Fire",
        IceTowerNpc => "Ice",
        ThunderTowerNpc => "Lightning",
        _ => null,
    };

    public static int Clock(Position pos)
    {
        var hour = (int)Math.Round(Bearing(pos) / (360.0 / ClockHours)) % ClockHours;
        return hour == 0 ? ClockHours : hour;
    }

    public static int NthAlong(
        IEnumerable<GameEvent> towers, string element, Position from, Position to, bool clockwise)
    {
        double Along(Position pos)
        {
            var delta = clockwise ? Bearing(pos) - Bearing(from) : Bearing(from) - Bearing(pos);
            return (delta % 360.0 + 360.0) % 360.0;
        }

        var target = Along(to);
        var margin = RingStepDegrees / 2.0;

        return 1 + towers.Count(t =>
            t.Target is { } lit && lit.Pos.Known && Element(lit.BaseId) == element &&
            Along(lit.Pos) is var d && d > margin && d < target - margin);
    }

    public static string Nth(int n) => n switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ => n + "th",
    };

    public static (int Steps, bool Clockwise)? Turn(Position from, Position to)
    {
        var delta = (Bearing(to) - Bearing(from) + 360.0) % 360.0;
        var steps = (int)Math.Round(delta / RingStepDegrees) % RingTowers;

        if (steps == 0) return null;

        return steps <= RingTowers / 2 ? (steps, true) : (RingTowers - steps, false);
    }

    public static GameEvent? Standing(IEnumerable<GameEvent> towers, string element, Position? from)
    {
        var lit = towers
            .Where(t => t.Target is { } on && on.Pos.Known && Element(on.BaseId) == element)
            .ToList();

        if (lit.Count <= 1) return lit.Count == 1 ? lit[0] : null;
        if (from is not { } here) return null;

        return lit
            .OrderBy(t => (Bearing(t.Target!.Pos) - Bearing(here) + 360.0) % 360.0)
            .First();
    }

    public static double Bearing(Position pos)
    {
        var degrees = Math.Atan2(pos.X - Centre, Centre - pos.Y) * 180.0 / Math.PI;
        return degrees < 0 ? degrees + 360.0 : degrees;
    }

    public const double Centre = 100.0;

    public static GameEvent? FirstSafeTower(IEnumerable<GameEvent> towers, uint vulnTower)
    {
        if (vulnTower == 0) return null;

        var ring = towers
            .Where(t => t.Target is { } lit && lit.Pos.Known && Element(lit.BaseId) is not null)
            .OrderBy(t => Bearing(t.Target!.Pos))
            .ToList();

        var at = ring.FindIndex(t => t.Target!.BaseId == vulnTower);
        if (at < 0) return null;

        for (var step = 1; step < ring.Count; step++)
        {
            var next = ring[(at + step) % ring.Count];
            if (next.Target!.BaseId != vulnTower) return next;
        }

        return null;
    }

    public static string? FirstSafe(IEnumerable<GameEvent> towers, uint vulnTower) =>
        FirstSafeTower(towers, vulnTower) is { Target: { } on } ? Element(on.BaseId) : null;

    public static string? Order(IEnumerable<GameEvent> towers, uint resDownStatus)
    {
        var mine = TowerFor(resDownStatus);
        if (mine == 0) return null;

        var lit = towers.ToList();
        if (FirstSafe(lit, mine) is not { } first) return null;

        var last = Element(mine);
        if (last is null) return null;

        var other = new[] { "Fire", "Ice", "Lightning" }
            .FirstOrDefault(e => e != first && e != last);

        return other is null ? null : string.Join(OrderSeparator, first, other, last);
    }

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
        var mine = 0u;
        if (myFire is not null) { run.Call(celestriadFireResDown, myFire); mine = FireDown; }
        else if (myLightning is not null) { run.Call(celestriadLightningResDown, myLightning); mine = LightningDown; }
        else if (myIce is not null) { run.Call(celestriadIceResDown, myIce); mine = IceDown; }
        else
        {
            run.Call(celestriadNoResDown);
            noDebuff = true;
        }

        run.SetParam("noDebuff", noDebuff);

        var shouldCall = noDebuff || !DoubleTowerOnlyWithNoDebuff;

        string[]? order = null;
        Position? standing = null;

        for (var i = 0; i < TowerSets; i++)
        {
            var towers = await run.WaitEventsQuickSuccession(4, IsTowerLight);

            if (i == 0 && Order(towers, mine) is { } line)
            {
                order = line.Split(OrderSeparator);
                run.SetParam(OrderParam, line);
                run.SetParam(NextParam, order.Length > 1 ? order[1] : null);
            }

            var take = order is null || i >= order.Length
                ? null
                : i == 0
                    ? FirstSafeTower(towers, TowerFor(mine))
                    : Standing(towers, order[i], standing);

            if (take is { Target: { } spot })
            {
                run.SetParam(TakeParam, order![i]);

                if (standing is not null && Turn(standing.Value, spot.Pos) is { } turn)
                {
                    run.SetParam(StepsParam, turn.Steps);
                    run.SetParam(TurnParam, turn.Clockwise ? "CW" : "CCW");
                    run.SetParam(TurnSpokenParam, turn.Clockwise ? "Clockwise" : "Counterclockwise");
                    run.SetParam(NthParam, Nth(NthAlong(towers, order![i], standing.Value, spot.Pos, turn.Clockwise)));
                    run.Call(celestriadNextTower);
                }
                else
                {
                    run.SetParam(ClockParam, Clock(spot.Pos));
                    run.Call(celestriadFirstTower);
                }

                standing = spot.Pos;
            }

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
