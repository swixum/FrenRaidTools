namespace FrenRaidTools.Engine.DancingMad;

public sealed class TeleTrouncing
{
    public const string Group = "teleTrouncing";

    public const uint TeleTrouncingCast = 0xBAB9;
    public const uint Kefka = 19504;

    public const uint ArrowUpA = 0x130C;
    public const uint ArrowDownA = 0x130D;
    public const uint ArrowRightA = 0x130E;
    public const uint ArrowLeftA = 0x130F;
    public const uint ArrowUpB = 0x13D7;
    public const uint ArrowDownB = 0x13D8;
    public const uint ArrowRightB = 0x13D9;
    public const uint ArrowLeftB = 0x13DA;

    public const double ShortBuffSeconds = 8.5;

    public const uint Confetti = 0x13D6;
    public const uint StoneTether = 45;
    public const uint GazeControl = 0x19D;
    public const uint GazeArg1 = 0x40;
    public const uint GazeArg2 = 0x80;

    public const uint FireSpread = 127;
    public const uint FireStack = 128;
    public const uint FakeFire = 673;
    public const uint FakeThunder = 677;

    public const uint SleepIcon = 4894;
    public const uint ConfusionIcon = 1283;

    private enum Arrow { Up, Down, Right, Left }

    public readonly Callout lightOfJudgment = Callout.Duration("Light of Judgment", "Raidwide");

    public readonly Callout arrowsInitial = Callout.Duration("Tele-trouncing", "Arrows");

    public readonly Callout doubleNorth = Callout.Duration("TT: Double N", "Double North").Icon(ArrowUpA, ArrowUpA);
    public readonly Callout doubleSouth = Callout.Duration("TT: Double S", "Double South").Icon(ArrowDownA, ArrowDownA);
    public readonly Callout doubleEast = Callout.Duration("TT: Double E", "Double East").Icon(ArrowRightA, ArrowRightA);
    public readonly Callout doubleWest = Callout.Duration("TT: Double W", "Double West").Icon(ArrowLeftA, ArrowLeftA);

    public readonly Callout northToWest = Callout.Duration("TT: N -> W", "North West", "North -> West")
        .Icon(ArrowUpA, ArrowLeftA)
        .Note("With an arrow strat picked these read off the two spots you drop your arrows on. Only the big box has spots; on the other strats they call the arrows themselves, in the order they will expire, i.e. right-to-left on the HUD.");
    public readonly Callout northToEast = Callout.Duration("TT: N -> E", "North East", "North -> East").Icon(ArrowUpA, ArrowRightA);
    public readonly Callout southToWest = Callout.Duration("TT: S -> W", "South West", "South -> West").Icon(ArrowDownA, ArrowLeftA);
    public readonly Callout southToEast = Callout.Duration("TT: S -> E", "South East", "South -> East").Icon(ArrowDownA, ArrowRightA);
    public readonly Callout eastToNorth = Callout.Duration("TT: E -> N", "East North", "East -> North").Icon(ArrowRightA, ArrowUpA);
    public readonly Callout eastToSouth = Callout.Duration("TT: E -> S", "East South", "East -> South").Icon(ArrowRightA, ArrowDownA);
    public readonly Callout westToNorth = Callout.Duration("TT: W -> N", "West North", "West -> North").Icon(ArrowLeftA, ArrowUpA);
    public readonly Callout westToSouth = Callout.Duration("TT: W -> S", "West South", "West -> South").Icon(ArrowLeftA, ArrowDownA);
    public readonly Callout arrowError = Callout.Duration("TT: Error", "Error");

    public readonly Callout onlyNorth = Callout.Duration("TT: One Arrow Read (N)", "North").Icon(ArrowUpA);
    public readonly Callout onlySouth = Callout.Duration("TT: One Arrow Read (S)", "South").Icon(ArrowDownA);
    public readonly Callout onlyEast = Callout.Duration("TT: One Arrow Read (E)", "East").Icon(ArrowRightA);
    public readonly Callout onlyWest = Callout.Duration("TT: One Arrow Read (W)", "West").Icon(ArrowLeftA);

    public readonly Callout confettiOnYou =
        Callout.Duration("TT: Knockback on You", "Knockback on YOU").AutoIcon();
    public readonly Callout confettiNotOnYou =
        Callout.Duration("TT: Knockback not on You", "Knockback on {confettiPlayers}").AutoIcon();

    public readonly Callout sleepTetherInitial =
        Callout.Of("TT: Sleep Tether (Initial)", "Sleep Tether").Icon(SleepIcon).Quiet()
            .Note("Off by default at swix's ask: the sleep and confusion tethers are not worth a call.");
    public readonly Callout confusionTetherInitial =
        Callout.Of("TT: Confusion Tether (Initial)", "Confusion Tether").Icon(ConfusionIcon).Quiet()
            .Note("Off by default at swix's ask: the tether call after the knockback says what to do about it, so naming it twice adds nothing.");

    public readonly Callout sleepTether =
        Callout.Of("TT: Sleep Tether (After Knockback)", "Spread for Sleep").Icon(SleepIcon).Quiet()
            .Note("Off by default at swix's ask: the sleep and confusion tethers are not worth a call.");
    public readonly Callout confuseTether =
        Callout.Of("TT: Confusion Tether (After Knockback)", "Spread for Confusion").Icon(ConfusionIcon).Quiet()
            .Note("Off by default at swix's ask: the sleep and confusion tethers are not worth a call.");

    public readonly Callout earlyFakeGaze = Callout.Of("TT: Fake Gaze (Early Call)", "Look Towards Statue");
    public readonly Callout earlyRealGaze = Callout.Of("TT: Real Gaze (Early Call)", "Look Away");

    public readonly Callout elementMechanic = Callout.Of("TT: Element Mechanics",
        "{actualSpread ? 'Spread' : 'Stack'} {fakeThunder ? 'In Thunder' : 'In Safe'}, Look {fakeGaze ? 'Towards' : 'Away'}");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, TeleTrouncingCast),
            (start, run) => Run(start, run, world));

    private static bool IsArrow(GameEvent e) =>
        e.Id is ArrowUpA or ArrowDownA or ArrowRightA or ArrowLeftA
             or ArrowUpB or ArrowDownB or ArrowRightB or ArrowLeftB;

    private static Arrow? Direction(uint id) => id switch
    {
        ArrowUpA or ArrowUpB => Arrow.Up,
        ArrowDownA or ArrowDownB => Arrow.Down,
        ArrowRightA or ArrowRightB => Arrow.Right,
        ArrowLeftA or ArrowLeftB => Arrow.Left,
        _ => null,
    };

    private static char Letter(Arrow arrow) => arrow switch
    {
        Arrow.Up => 'N',
        Arrow.Down => 'S',
        Arrow.Right => 'E',
        _ => 'W',
    };

    private static Callout Spots(IWorld world, Callout call, Arrow? first, Arrow? second)
    {
        if (first is null || second is null) return call;

        var pair = $"{Letter(first.Value)}{Letter(second.Value)}";
        var chosen = world.Chosen(ArrowSpots.OptionKey);
        var text = ArrowSpots.Text(chosen, pair);
        if (text is null) return call;

        return call with
        {
            Text = text + Callout.CountdownToken,
            Speech = ArrowSpots.Speech(chosen, pair) ?? text,
        };
    }

    private Callout Single(Arrow arrow) => arrow switch
    {
        Arrow.Up => onlyNorth,
        Arrow.Down => onlySouth,
        Arrow.Right => onlyEast,
        _ => onlyWest,
    };

    private Callout? Pair(Arrow first, Arrow second) => first switch
    {
        Arrow.Up => second switch
        {
            Arrow.Up => doubleNorth,
            Arrow.Right => northToEast,
            Arrow.Left => northToWest,
            _ => null,
        },
        Arrow.Down => second switch
        {
            Arrow.Down => doubleSouth,
            Arrow.Right => southToEast,
            Arrow.Left => southToWest,
            _ => null,
        },
        Arrow.Right => second switch
        {
            Arrow.Right => doubleEast,
            Arrow.Up => eastToNorth,
            Arrow.Down => eastToSouth,
            _ => null,
        },
        Arrow.Left => second switch
        {
            Arrow.Left => doubleWest,
            Arrow.Up => westToNorth,
            Arrow.Down => westToSouth,
            _ => null,
        },
        _ => null,
    };

    public const double ArrowsAppearWithinSeconds = 12.0;
    public const double LongArrowRunsOutSeconds = 15.0;

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(arrowsInitial, start);

        var shortBuff = await run.FindOrWaitForStatusWithin(world,
            e => e.Target?.IsYou == true && IsArrow(e) && e.Duration < ShortBuffSeconds,
            ArrowsAppearWithinSeconds);
        var longBuff = await run.FindOrWaitForStatusWithin(world,
            e => e.Target?.IsYou == true && IsArrow(e) && e.Duration > ShortBuffSeconds,
            shortBuff is null ? 0.1 : ArrowsAppearWithinSeconds);

        var first = shortBuff is null ? null : Direction(shortBuff.Id);
        var second = longBuff is null ? null : Direction(longBuff.Id);

        var call = first is null || second is null ? null : Pair(first.Value, second.Value);
        if (call is not null) run.Call(Spots(world, call, first, second), shortBuff!);
        else if (first is not null || second is not null)
            run.Call(Single((first ?? second)!.Value), (shortBuff ?? longBuff)!);
        else run.Call(arrowError, start);

        if (longBuff is not null)
            await run.WaitStatusRemovedOrExpired(longBuff, graceSeconds: 2.0);
        else
            await run.WaitSeconds(LongArrowRunsOutSeconds);

        var confettis = world.ActiveStatuses().Where(s => s.Id == Confetti).ToList();
        var confettiPlayers = confettis.Select(c => c.Target).OfType<Actor>().ToList();
        run.SetParam("confettiPlayers", confettiPlayers);

        var mine = confettis.FirstOrDefault(c => c.Target?.IsYou == true);
        if (mine is not null) run.Call(confettiOnYou, mine);
        else if (confettis.Count > 0) run.Call(confettiNotOnYou, confettis[0]);

        var tethers = await run.WaitEventsQuickSuccession(8, e => e.Is(EventKind.Tether, StoneTether));
        await run.Settle();

        var myTether = tethers.FirstOrDefault(t => t.EitherEnd(a => a.IsYou));
        if (myTether is null) return;

        var anchor = myTether.OtherEnd(a => a.IsYou);
        if (anchor is null) return;

        var playerStone = (world.Latest(anchor)?.Pos.X ?? anchor.Pos.X) > 100.0f;
        run.SetParam("playerStone", playerStone);

        var tetherCall = run.Call(playerStone ? sleepTetherInitial : confusionTetherInitial);

        if (confettis.Count > 0) await run.WaitStatusRemoved(confettis[0]);
        tetherCall.ForceExpire();
        run.Call(playerStone ? sleepTether : confuseTether);

        var gaze = await run.WaitEvent(EventKind.ActorControl,
            e => e.Id == GazeControl && e.Arg1 == GazeArg1 && e.Arg2 == GazeArg2 && e.Arg3 == 0 && e.Arg4 == 0);
        await run.Settle();

        var gazeFrom = world.Latest(gaze.Target!) ?? gaze.Target!;
        var fakeGaze = gazeFrom.Pos.X < 100.0f;
        run.SetParam("fakeGaze", fakeGaze);

        var gazeCall = run.Call(fakeGaze ? earlyFakeGaze : earlyRealGaze, gaze);

        var kefkaMarkers = await run.WaitEvents(2, EventKind.HeadMarker, e => e.Target?.BaseId == Kefka);
        var playerMarker = await run.WaitEvent(EventKind.HeadMarker, FireSpread, FireStack);

        var fakeFire = kefkaMarkers.Any(m => m.Id == FakeFire);
        var fakeThunder = kefkaMarkers.Any(m => m.Id == FakeThunder);
        var presentSpread = playerMarker.Id == FireSpread;

        run.SetParam("fakeFire", fakeFire);
        run.SetParam("fakeThunder", fakeThunder);
        run.SetParam("actualSpread", presentSpread != fakeFire);

        gazeCall.ForceExpire();
        run.Call(elementMechanic);
    }

    public const uint LightOfJudgmentA = 0xC622;
    public const uint LightOfJudgmentB = 0xBABD;

    public Sequence BuildJudgment(IWorld world) =>
        Sequence.Indexed(Group + "Judgment", 30,
            e => e.Is(EventKind.CastStart, LightOfJudgmentA, LightOfJudgmentB),
            (start, run, i) =>
            {
                run.Call(lightOfJudgment, start);
                return Task.CompletedTask;
            });
}
