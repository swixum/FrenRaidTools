namespace FrenRaidTools.Engine.DancingMad;

public enum ForsakenMech { Cone, Circle, Stack, None }

public sealed class Forsaken
{
    public const string Group = "forsaken";

    public const uint ForsakenCast = 0xBABC;
    public const uint PathOfLight = 0xBABE;
    public const uint FutureCast = 0xBAD2;
    public const uint PastCast = 0xBAD3;
    public const uint AllThingsEndingA = 0xBADC;
    public const uint AllThingsEndingB = 0xBADD;

    public const uint StackMarker = 715;
    public const uint CircleMarker = 716;
    public const uint ConeMarker = 717;

    public const uint DebuffStacks = 0x13DB;
    public const uint UltimateEmbraceCast = 0xC24C;

    public const int FirstTowerSet = 2;
    public const int LastTowerSet = 8;

    public readonly Callout ultimateEmbrace =
        Callout.Duration("Ultimate Embrace", "Buster on {event.target}");

    public readonly Callout forsaken = Callout.Duration("Forsaken", "Raidwide");

    public readonly Callout forsakenDebuffReminder =
        Callout.Of("Forsaken: Debuff Tracker", "", "{event.stacks} Stacks").AutoIcon()
            .Note("This callout shows only your debuff stacks. You should NOT add TTS to this, because the game continuously refreshes this debuff.");

    public readonly Callout forsakenFirstCone =
        Callout.Of("Forsaken: Initial Cone", "Cone, {supportsCone ? 'Supports' : 'DPS'} have cone");
    public readonly Callout forsakenFirstCircle =
        Callout.Of("Forsaken: Initial Circle", "Circle, {supportsCone ? 'Supports' : 'DPS'} have cone");
    public readonly Callout forsakenFirstStack =
        Callout.Of("Forsaken: Initial Stack", "Stack, {supportsCone ? 'Supports' : 'DPS'} have cone");
    public readonly Callout forsakenFirstNothing =
        Callout.Of("Forsaken: Initial Nothing", "Error, {supportsCone ? 'Supports' : 'DPS'} have cone");

    public readonly Callout forsakenTowerCone =
        Callout.Of("Forsaken: Followup Cone + Past/Future (Tower Call)", "Cone with {buddy}, Baits")
            .Note("The 'Tower Call' variants of these call remind you of which tower pattern you will need to do. These indicate that the tower set will be accompanied with Past/Future cast bar, while the other four indicate that it will be accompanied with All Things Ending resolving.\n\nIn addition, the towerSet variable will let you see which set of towers it is (starting at 1 then incrementing each time towers go off). You can use expressions like {towerSet % 2 == 0 ? 'even' : 'odd'} to have conditional logic based on whether it is even or odd towers, or for doing things like swap callouts.\n\nYou can also use the list myMechs (indexed from 0) to recall previous mechanics you had. The entries in the list are CONE, CIRCLE, STACK, or NONE. e.g. to see what you had on the third set, use myMechs[2].");
    public readonly Callout forsakenTowerCircle =
        Callout.Of("Forsaken: Followup Circle + Past/Future (Tower Call)", "Circle with {buddy}, Baits");
    public readonly Callout forsakenTowerStack =
        Callout.Of("Forsaken: Followup Stack + Past/Future (Tower Call)", "Stack, Baits");
    public readonly Callout forsakenTowerNothing =
        Callout.Of("Forsaken: Followup Nothing + Past/Future (Tower Call)", "Nothing, Baits");

    public readonly Callout forsakenTowerNoPfCone =
        Callout.Of("Forsaken: Followup Cone + No Past/Future (Tower Call)", "Cone");
    public readonly Callout forsakenTowerNoPfCircle =
        Callout.Of("Forsaken: Followup Circle + No Past/Future (Tower Call)", "Circle");
    public readonly Callout forsakenTowerNoPfStack =
        Callout.Of("Forsaken: Followup Stack + No Past/Future (Tower Call)", "Stack with {buddy}");
    public readonly Callout forsakenTowerNoPfNothing =
        Callout.Of("Forsaken: Followup Nothing + No Past/Future (Tower Call)", "Nothing");

    public readonly Callout forsakenFollowupPastCone =
        Callout.Of("Forsaken: Followup Cone + Past", "Cone, Bait Past")
            .Note("This set of eight calls tells you that you need to bait for All Things Ending.");
    public readonly Callout forsakenFollowupPastCircle =
        Callout.Of("Forsaken: Followup Circle + Past", "Circle, Bait Past");
    public readonly Callout forsakenFollowupPastStack =
        Callout.Of("Forsaken: Followup Stack + Past", "Stack with {buddy}, Bait Past");
    public readonly Callout forsakenFollowupPastNothing =
        Callout.Of("Forsaken: Followup Nothing + Past", "Nothing, Bait Past");

    public readonly Callout forsakenFollowupFutureCone =
        Callout.Of("Forsaken: Followup Cone + Future", "Cone, Bait Future");
    public readonly Callout forsakenFollowupFutureCircle =
        Callout.Of("Forsaken: Followup Circle + Future", "Circle, Bait Future");
    public readonly Callout forsakenFollowupFutureStack =
        Callout.Of("Forsaken: Followup Stack + Future", "Stack with {buddy}, Bait Future");
    public readonly Callout forsakenFollowupFutureNothing =
        Callout.Of("Forsaken: Followup Nothing + Future", "Nothing, Bait Future");

    public readonly Callout forsakenNewDebuff =
        Callout.Of("Forsaken: New Debuff (Tower 3)", "New debuff {myNextMech}")
            .Note("Soaking the third tower hands you the debuff you keep all the way to the last tower, and it lands after the tower 3 call has already been said. This fires the moment that marker appears.");

    public const string FinalBait = "Bait between for final tower";

    public readonly Callout forsakenFinalFuture =
        Callout.Of("Forsaken: Final Future Nothing", $"{FinalBait} (Future)");
    public readonly Callout forsakenFinalPast =
        Callout.Of("Forsaken: Final Past Nothing", $"{FinalBait} (Past)")
            .In(2, "Forsaken: Final Past Nothing");

    public static ForsakenMech? MechFor(uint markerId) => markerId switch
    {
        StackMarker => ForsakenMech.Stack,
        CircleMarker => ForsakenMech.Circle,
        ConeMarker => ForsakenMech.Cone,
        _ => null,
    };

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 120, e => e.Is(EventKind.CastStart, ForsakenCast),
            (start, run) => Run(start, run, world));

    private static bool IsMechMarker(GameEvent e) =>
        e.Kind == EventKind.HeadMarker &&
        e.Id is StackMarker or CircleMarker or ConeMarker;

    public const string LeftTower = "Left Tower";
    public const string RightTower = "Right Tower";

    private static int Htmr(Actor actor) => JobKinds.Kind(actor.Job) switch
    {
        JobKind.Healer => 0,
        JobKind.Tank => 1,
        JobKind.Melee => 2,
        _ => 3,
    };

    private static string? RoleOf(IWorld world) => world.You is not { } you
        ? null
        : JobKinds.Kind(you.Job) switch
        {
            JobKind.Tank => "Tank",
            JobKind.Healer => "Healer",
            JobKind.Melee => "Melee",
            JobKind.PhysRanged or JobKind.Caster => "Ranged",
            _ => null,
        };

    public readonly record struct Marked(Actor Who, ForsakenMech Mech);

    public static readonly int[] FirstGroupTowers = [1, 2, 3, 8];

    public static bool FirstGroupSoaks(int set) => Array.IndexOf(FirstGroupTowers, set) >= 0;

    private static string? Side(IReadOnlyList<Actor> sharing, IWorld world, Actor you)
    {
        if (sharing.Count != 2) return null;

        var seats = sharing.Select(world.SeatOf).ToList();

        var ordered = seats.All(s => s >= 0) && seats[0] != seats[1]
            ? sharing.OrderBy(a => Slots.PrioOf(world.SeatOf(a))).ToList()
            : sharing.OrderBy(Htmr).ThenBy(a => a.Name, StringComparer.Ordinal).ToList();

        var at = ordered.FindIndex(a => a.ObjectId == you.ObjectId);
        if (at < 0) return null;

        return at == 0 ? LeftTower : RightTower;
    }

    public static string? TowerFor(
        IReadOnlyList<GameEvent> markers, IWorld world, ForsakenMech mine)
    {
        if (mine == ForsakenMech.None || world.You is not { } you) return null;

        var sharing = markers
            .Where(m => (MechFor(m.Id) ?? ForsakenMech.None) == mine)
            .Select(m => m.Target)
            .OfType<Actor>()
            .Where(a => a.IsPlayer)
            .ToList();

        return Side(sharing, world, you);
    }

    public static string? TowerIn(
        IReadOnlyDictionary<uint, Marked> held, IReadOnlyCollection<uint> soakers,
        IWorld world, ForsakenMech mine)
    {
        if (mine == ForsakenMech.None || world.You is not { } you) return null;
        if (!soakers.Contains(you.ObjectId)) return null;

        var sharing = new List<Actor>();
        foreach (var id in soakers)
            if (held.TryGetValue(id, out var marked) && marked.Mech == mine)
                sharing.Add(marked.Who);

        return Side(sharing, world, you);
    }

    public static void Remember(Dictionary<uint, Marked> held, IReadOnlyList<GameEvent> wave)
    {
        foreach (var marker in wave)
        {
            if (marker.Target is not { IsPlayer: true } who) continue;
            held[who.ObjectId] = new Marked(who, MechFor(marker.Id) ?? ForsakenMech.None);
        }
    }

    public static HashSet<uint> SoakersAt(
        int set, IReadOnlyCollection<uint> first, IReadOnlyDictionary<uint, Marked> held)
    {
        if (FirstGroupSoaks(set)) return [.. first];

        var rest = new HashSet<uint>();
        foreach (var id in held.Keys)
            if (!first.Contains(id)) rest.Add(id);

        return rest;
    }

    public static bool? PartnerDiffers(
        IReadOnlyList<GameEvent> markers, IWorld world, ForsakenMech mine)
    {
        if (world.You is not { } you) return null;

        if (world.Partner() is { } buddy)
        {
            foreach (var marker in markers)
            {
                if (marker.Target?.ObjectId != buddy.ObjectId) continue;
                return (MechFor(marker.Id) ?? ForsakenMech.None) != mine;
            }

            return null;
        }

        return mine == ForsakenMech.Stack ? true : null;
    }

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(forsaken, start);
        ForgetStacks();

        var myMechs = new List<string>();
        run.SetParam("myMechs", myMechs);

        var markers = await run.WaitEventsQuickSuccession(8, IsMechMarker);

        var held = new Dictionary<uint, Marked>();
        Remember(held, markers);

        var mine = markers.FirstOrDefault(m => m.Target?.IsYou == true);
        var myMech = mine is null ? ForsakenMech.None : MechFor(mine.Id) ?? ForsakenMech.None;
        run.SetParam("myMech", myMech);

        var cones = markers.Where(m => m.Id == ConeMarker).ToList();
        var supportsWithCone = cones.Count(m => m.Target?.Support == true);
        var dpsWithCone = cones.Count - supportsWithCone;
        run.SetParam("supportsCone", supportsWithCone > dpsWithCone);

        run.SetParam("stackPlayers", Targets(markers, StackMarker));
        run.SetParam("conePlayers", Targets(markers, ConeMarker));
        run.SetParam("circledPlayers", Targets(markers, CircleMarker));
        run.SetParam("towerSet", 1);

        var split = (bool?)null;

        void Settle(bool? answer)
        {
            if (split is not null || answer is null) return;
            split = answer;
            run.SetParam("differentDebuffs", answer.Value);
        }

        Settle(PartnerDiffers(markers, world, myMech));

        run.SetParam("myTower", TowerFor(markers, world, myMech));
        run.SetParam("myRole", RoleOf(world));

        myMechs.Add(myMech.ToString().ToUpperInvariant());

        run.Call(myMech switch
        {
            ForsakenMech.Cone => forsakenFirstCone,
            ForsakenMech.Circle => forsakenFirstCircle,
            ForsakenMech.Stack => forsakenFirstStack,
            _ => forsakenFirstNothing,
        });

        var nextFuture = false;
        var castFuture = false;
        List<GameEvent> lastWave = [];
        HashSet<uint>? firstGroup = null;

        for (var set = FirstTowerSet; set <= LastTowerSet; set++)
        {
            await run.WaitEvent(EventKind.AbilityHit, PathOfLight);

            var wave = set < LastTowerSet
                ? await run.WaitEventsQuickSuccession(4, IsMechMarker)
                : [];

            if (wave.Count > 0)
            {
                firstGroup ??=
                [
                    .. wave.Select(m => m.Target).OfType<Actor>()
                        .Where(a => a.IsPlayer).Select(a => a.ObjectId),
                ];

                Remember(held, wave);
                lastWave = wave;
            }

            var round = wave.Count > 0 ? wave : lastWave;

            var roundMine = round.FirstOrDefault(m => m.Target?.IsYou == true);
            var roundMech = roundMine is null
                ? ForsakenMech.None
                : MechFor(roundMine.Id) ?? ForsakenMech.None;

            myMechs.Add(roundMech.ToString().ToUpperInvariant());

            run.SetParam("buddy", round
                .Where(m => m.Target?.IsYou != true)
                .Where(m => MechFor(m.Id) == roundMech)
                .Select(m => m.Target)
                .FirstOrDefault(t => t?.IsPlayer == true));

            run.SetParam("towerSet", set);
            run.SetParam(PlanStep.LastTowerParam, set == LastTowerSet);

            if (round.Count > 0)
            {
                run.SetParam("stackPlayers", Targets(round, StackMarker));
                run.SetParam("conePlayers", Targets(round, ConeMarker));
                run.SetParam("circledPlayers", Targets(round, CircleMarker));
            }

            if (firstGroup is { } opened && world.You is { } me)
                Settle(opened.Contains(me.ObjectId));

            var soakers = firstGroup is { } group ? SoakersAt(set, group, held) : null;

            var youSoak = soakers is not null && world.You is { } soaker
                && soakers.Contains(soaker.ObjectId);

            var myHeld = world.You is { } self && held.TryGetValue(self.ObjectId, out var marked)
                ? marked.Mech
                : roundMech;

            run.SetParam("myMech", myHeld);

            if (soakers is not null)
                run.SetParam("myTower", TowerIn(held, soakers, world, myHeld));
            else if (round.Count > 0)
                run.SetParam("myTower", TowerFor(round, world, roundMech));

            var carried = wave.Count > 0 && !youSoak
                && world.You is { } target
                && wave.Any(m => m.Target?.ObjectId == target.ObjectId);

            var hasPastFuture = set % 2 == 0;
            run.SetParam("hasPastFuture", hasPastFuture);

            if (hasPastFuture)
            {
                var pastFuture = await run.FindOrWaitForCast(
                    world, e => e.Id is FutureCast or PastCast);

                castFuture = pastFuture?.Id == FutureCast;
                nextFuture = castFuture && set != LastTowerSet;
                run.SetParam("future", nextFuture);

                run.Call(roundMech switch
                {
                    ForsakenMech.Cone => forsakenTowerCone,
                    ForsakenMech.Circle => forsakenTowerCircle,
                    ForsakenMech.Stack => forsakenTowerStack,
                    _ => forsakenTowerNothing,
                });
            }
            else
            {
                run.Call(nextFuture
                    ? roundMech switch
                    {
                        ForsakenMech.Cone => forsakenFollowupFutureCone,
                        ForsakenMech.Circle => forsakenFollowupFutureCircle,
                        ForsakenMech.Stack => forsakenFollowupFutureStack,
                        _ => forsakenFollowupFutureNothing,
                    }
                    : roundMech switch
                    {
                        ForsakenMech.Cone => forsakenFollowupPastCone,
                        ForsakenMech.Circle => forsakenFollowupPastCircle,
                        ForsakenMech.Stack => forsakenFollowupPastStack,
                        _ => forsakenFollowupPastNothing,
                    });

                await run.WaitEvent(EventKind.CastStart, AllThingsEndingA, AllThingsEndingB);

                run.Call(roundMech switch
                {
                    ForsakenMech.Cone => forsakenTowerNoPfCone,
                    ForsakenMech.Circle => forsakenTowerNoPfCircle,
                    ForsakenMech.Stack => forsakenTowerNoPfStack,
                    _ => forsakenTowerNoPfNothing,
                });
            }

            if (!carried) continue;

            run.SetParam("myNextMech", myHeld);
            run.Call(forsakenNewDebuff);
        }

        await run.WaitEvent(EventKind.AbilityHit, PathOfLight);

        run.Call(castFuture ? forsakenFinalFuture : forsakenFinalPast);
    }

    private static List<Actor> Targets(IEnumerable<GameEvent> markers, uint id) =>
        markers.Where(m => m.Id == id).Select(m => m.Target).OfType<Actor>().ToList();

    private int _shownStacks = NoStacks;

    public const int NoStacks = -1;

    public void ForgetStacks() => _shownStacks = NoStacks;

    public Sequence BuildExtras(IWorld world) =>
        Sequence.Indexed(Group + "Extras", 180,
            e => e.Is(EventKind.CastStart, UltimateEmbraceCast)
                 || (e.Kind == EventKind.StatusGain && e.Id == DebuffStacks && e.Target?.IsYou == true),
            (start, run, i) =>
            {
                if (start.Kind == EventKind.CastStart)
                {
                    run.Call(ultimateEmbrace, start);
                    return Task.CompletedTask;
                }

                if (start.Stacks == _shownStacks) return Task.CompletedTask;

                _shownStacks = start.Stacks;
                run.Call(forsakenDebuffReminder, start);
                return Task.CompletedTask;
            });
}
