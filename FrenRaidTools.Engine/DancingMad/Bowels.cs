namespace FrenRaidTools.Engine.DancingMad;

public sealed class Bowels
{
    public const string Group = "bowels";

    public const uint BowelsCast = 0xBAF2;

    public const uint Entropy = 0x640;
    public const uint Dynamic = 0x641;
    public const uint Headwind = 0x642;
    public const uint Tailwind = 0x643;

    public const uint EpicHero = 0x1060;
    public const uint FatedHero = 0x1062;

    public const uint DecisiveBattleChaos = 0xC2E2;
    public const uint DecisiveBattleExdeath = 0xC2E3;

    public const double SoonSeconds = 5.0;
    public const double OtherSoonSeconds = 7.0;

    public const string EntropyName = "Entropy";
    public const string DynamicName = "Dynamic";

    public readonly Callout epicHero = Callout.Of("Epic Hero", "Attack Chaos").AutoIcon();
    public readonly Callout fatedHero = Callout.Of("Fated Hero", "Attack Exdeath").AutoIcon();

    public readonly Callout decisiveBattle =
        Callout.Duration("The Decisive Battle", "Get your Debuffs");

    public readonly Callout bowelsInitial = Callout.Duration("Bowels of Agony", "Raidwide");

    public readonly Callout bowelsHeadwind =
        Callout.Duration("Bowels: Headwind Only", "Face away from Exdeath").Icon(Headwind);
    public readonly Callout bowelsTailwind =
        Callout.Duration("Bowels: Tailwind Only", "Face Exdeath").Icon(Tailwind);
    public readonly Callout bowelsHeadwindEntropy =
        Callout.Duration("Bowels: Headwind + Entropy", "Face away from Exdeath, Fire").Icon(Entropy, Headwind);
    public readonly Callout bowelsTailwindEntropy =
        Callout.Duration("Bowels: Tailwind + Entropy", "Face Exdeath, Fire").Icon(Entropy, Tailwind);
    public readonly Callout bowelsHeadwindDynamic =
        Callout.Duration("Bowels: Headwind + Dynamic Fluid", "Face away from Exdeath, Water").Icon(Dynamic, Headwind);
    public readonly Callout bowelsTailwindDynamic =
        Callout.Duration("Bowels: Tailwind + Dynamic Fluid", "Face Exdeath, Water").Icon(Dynamic, Tailwind);

    public readonly Callout bowelsMyEntropySoon =
        Callout.Duration("Bowels: My Entropy Soon", "Fire On You Soon").Icon(Entropy);
    public readonly Callout bowelsOtherEntropySoon =
        Callout.Duration("Bowels: Other Entropy Soon", "Fires Soon");
    public readonly Callout bowelsMyDynamicSoon =
        Callout.Duration("Bowels: My Dynamic Soon", "Water On You Soon").Icon(Dynamic);
    public readonly Callout bowelsOtherDynamicSoon =
        Callout.Duration("Bowels: Other Dynamic Soon", "Waters Soon");

    public const string BowelsOption = "bowels";
    public const string LimitBreakChoice = "lb";
    public const double LimitBreakLeadSeconds = 3.0;

    public readonly Callout bowelsTankLimitBreak =
        Callout.Duration("Bowels: Tank Limit Break", "Tank LB");

    public readonly Callout bowelsHeadwindAfter =
        Callout.Duration("Bowels: Knockback, Headwind", "{spotSpeech}, Face Away", "{spot}, Face Away").Icon(Headwind);
    public readonly Callout bowelsTailwindAfter =
        Callout.Duration("Bowels: Knockback, Tailwind", "{spotSpeech}, Face Exdeath", "{spot}, Face Exdeath").Icon(Tailwind);

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, BowelsCast),
            (start, run) => Run(start, run, world));

    private static bool IsWindDebuff(GameEvent e) =>
        e.Kind == EventKind.StatusGain &&
        e.Id is Entropy or Dynamic or Headwind or Tailwind;

    public static string? ShorterOf(IEnumerable<GameEvent> debuffs)
    {
        double? entropy = null;
        double? dynamic = null;

        foreach (var e in debuffs)
        {
            if (e.Id == Entropy && (entropy is null || e.Duration < entropy)) entropy = e.Duration;
            if (e.Id == Dynamic && (dynamic is null || e.Duration < dynamic)) dynamic = e.Duration;
        }

        if (entropy is null || dynamic is null) return null;
        if (Math.Abs(entropy.Value - dynamic.Value) < 0.5) return null;

        return entropy < dynamic ? EntropyName : DynamicName;
    }

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(bowelsInitial, start);

        var all = await run.WaitEventsQuickSuccession(12, IsWindDebuff);

        var myEntropy = Mine(all, Entropy);
        var myDynamic = Mine(all, Dynamic);
        var myHeadwind = Mine(all, Headwind);
        var myTailwind = Mine(all, Tailwind);

        run.SetParam(PlanStep.FluidParam, ShorterOf(all));

        if (myHeadwind is not null)
        {
            if (myEntropy is not null) run.Call(bowelsHeadwindEntropy, myEntropy);
            else if (myDynamic is not null) run.Call(bowelsHeadwindDynamic, myDynamic);
            else run.Call(bowelsHeadwind, myHeadwind);
        }
        else if (myTailwind is not null)
        {
            if (myEntropy is not null) run.Call(bowelsTailwindEntropy, myEntropy);
            else if (myDynamic is not null) run.Call(bowelsTailwindDynamic, myDynamic);
            else run.Call(bowelsTailwind, myTailwind);
        }

        foreach (var wave in Waves(all, myEntropy, myDynamic))
        {
            run.SetParam(PlanStep.FluidParam, wave.Entropy ? EntropyName : DynamicName);
            run.SetParam(ShortResolveParam, wave.First);

            if (wave.Mine is not null)
            {
                var popped = await run.WaitStatusRemovedUntil(
                    wave.Mine, wave.Mine.At + wave.Mine.Duration - SoonSeconds);
                if (popped is null)
                {
                    var soon = run.Call(wave.Entropy ? bowelsMyEntropySoon : bowelsMyDynamicSoon, wave.Mine);
                    await run.WaitStatusRemovedOrExpired(wave.Mine, 2.0);
                    soon.ForceExpire();
                }
            }
            else
            {
                var popped = await run.WaitStatusRemovedUntil(
                    wave.Any, wave.Any.At + wave.Any.Duration - OtherSoonSeconds);
                if (popped is null)
                {
                    var soon = run.Call(wave.Entropy ? bowelsOtherEntropySoon : bowelsOtherDynamicSoon, wave.Any);
                    await run.WaitStatusRemovedOrExpired(wave.Any, 2.0);
                    soon.ForceExpire();
                }
            }
        }

        var wind = myHeadwind ?? myTailwind;
        if (wind is null) return;

        var check = await run.WaitEventUntil(
            e => e.Is(EventKind.CastStart, Earthquake.VacuumWaveCast),
            wind.At + wind.Duration + 2.0);

        var face = run.Call(
            myHeadwind is not null ? bowelsHeadwindAfter : bowelsTailwindAfter,
            check ?? wind);

        if (check is not null && YoursToPress(world))
        {
            await run.WaitSeconds(check.Duration - LimitBreakLeadSeconds - run.Since(check));
            var press = run.Call(bowelsTankLimitBreak, check);
            await run.WaitSeconds(LimitBreakLeadSeconds);
            press.ForceExpire();
        }

        await run.WaitStatusRemovedOrExpired(wind, 2.0);
        face.ForceExpire();
    }

    private static bool YoursToPress(IWorld world) =>
        world.Chosen(BowelsOption) == LimitBreakChoice &&
        JobKinds.Kind(world.You?.Job ?? "") == JobKind.Tank;

    public const string ShortResolveParam = "shortResolve";

    public sealed record Wave(GameEvent Any, GameEvent? Mine, bool Entropy, bool First);

    public static List<Wave> Waves(
        IReadOnlyList<GameEvent> all, GameEvent? myEntropy, GameEvent? myDynamic)
    {
        var found = new List<Wave>();

        var anyEntropy = all.FirstOrDefault(e => e.Id == Entropy);
        var anyDynamic = all.FirstOrDefault(e => e.Id == Dynamic);

        if (anyEntropy is not null) found.Add(new Wave(anyEntropy, myEntropy, true, true));
        if (anyDynamic is not null) found.Add(new Wave(anyDynamic, myDynamic, false, true));

        found.Sort((a, b) => (a.Any.At + a.Any.Duration).CompareTo(b.Any.At + b.Any.Duration));

        for (var i = 1; i < found.Count; i++) found[i] = found[i] with { First = false };

        return found;
    }

    private static GameEvent? Mine(IEnumerable<GameEvent> all, uint id) =>
        all.FirstOrDefault(e => e.Id == id && e.Target?.IsYou == true);

    public Sequence BuildDecisive(IWorld world) =>
        Sequence.Repeat(Group + "Decisive", 30,
            e => e.Is(EventKind.CastStart, DecisiveBattleChaos, DecisiveBattleExdeath),
            async (start, run) =>
            {
                run.Call(decisiveBattle, start);
                await run.WaitCastFinished(start);
            });

    public Sequence BuildHeroes(IWorld world) =>
        Sequence.Indexed(Group + "Heroes", 180,
            e => e.Kind == EventKind.StatusGain && e.Target?.IsYou == true
                 && (e.Id == EpicHero || e.Id == FatedHero),
            (start, run, i) =>
            {
                run.Call(start.Id == EpicHero ? epicHero : fatedHero, start);
                return Task.CompletedTask;
            });
}
