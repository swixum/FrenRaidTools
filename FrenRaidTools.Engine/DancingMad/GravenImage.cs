namespace FrenRaidTools.Engine.DancingMad;

public sealed class GravenImage
{
    public const uint GravenImageCast = 0xBCF2;
    public const uint Kefka = 19504;

    public const uint FireSpread = 127;
    public const uint FireStack = 128;
    public const uint FakeFire = 673;
    public const uint RealFire = 674;
    public const uint FakeIce = 675;
    public const uint RealIce = 676;
    public const uint FakeThunder = 677;
    public const uint RealThunder = 678;

    public const uint Laser = 0xBAA8;
    public const uint TowerCast = 0xBAAA;
    public const uint Confetti = 0x13D6;
    public const uint ConfettiKnockback = 0xBAA7;

    public const uint StoneTether = 45;
    public const uint FirstStoneDrop = 0xBAAC;
    public const uint HandActivate = 0xBAB0;
    public const uint GlowingHandControl = 0x19D;
    public const uint GlowingHandArg1 = 0x40;
    public const uint GlowingHandArg2 = 0x80;

    public readonly Callout gravenImage = Callout.Duration("Graven Image");

    public readonly Callout graven1Tether = Callout.Of("Graven Image 1: Tether", "Knockback");
    public readonly Callout graven1NoTether = Callout.Of("Graven Image 1: No Tether", "No Tether");

    public readonly Callout gravenRealIceSpread =
        Callout.Of("Graven Image 1: Real Ice, Spread", "Spread out of Cones");
    public readonly Callout gravenRealIceStack =
        Callout.Of("Graven Image 1: Real Ice, Stack", "Stacks out of Cones");
    public readonly Callout gravenFakeIceSpread =
        Callout.Of("Graven Image 1: Fake Ice, Spread", "Spread in Cones");
    public readonly Callout gravenFakeIceStack =
        Callout.Of("Graven Image 1: Fake Ice, Stack", "Stacks in Cones");

    public readonly Callout gravenSpreadForLaser =
        Callout.Of("Graven Image 1: Spread For Laser", "Line Spread");
    public readonly Callout gravenAvoidTower =
        Callout.Of("Graven Image 1: Got Hit by Laser", "Avoid Tower");
    public readonly Callout gravenTakeTower =
        Callout.Duration("Graven Image 1: Take Tower", "Take Tower");

    public readonly Callout gravenConfetti =
        Callout.Duration("Graven Image 1: Knockback on You", "Knockback").AutoIcon();
    public readonly Callout gravenNoConfetti =
        Callout.Duration("Graven Image 1: Knockback on You", "Knockback on {confettiPlayers}")
            .In(1, "Graven Image 1: Knockback on Others");

    public readonly Callout gravenRealIceRealThunder =
        Callout.Of("Graven Image 1: Real Ice, Real Thunder", "Avoid Both");
    public readonly Callout gravenRealIceFakeThunder =
        Callout.Of("Graven Image 1: Real Ice, Fake Thunder", "Out of Cones, In Lines");
    public readonly Callout gravenFakeIceRealThunder =
        Callout.Of("Graven Image 1: Fake Ice, Real Thunder", "In Cones, Out of Lines");
    public readonly Callout gravenFakeIceFakeThunder =
        Callout.Of("Graven Image 1: Fake Ice, Fake Thunder", "Stand in Both");

    public readonly Callout graven2realIceStone =
        Callout.Of("Graven Image 2: Real Ice, Stone", "Avoid Ice, Stone");
    public readonly Callout graven2fakeIceStone =
        Callout.Of("Graven Image 2: Fake Ice, Stone", "Fake Ice, Stone");
    public readonly Callout graven2realIceDark =
        Callout.Of("Graven Image 2: Real Ice, Dark", "Avoid Ice, Dark");
    public readonly Callout graven2fakeIceDark =
        Callout.Of("Graven Image 2: Fake Ice, Dark", "Fake Ice, Dark");

    public readonly Callout graven2dropFirstStone =
        Callout.Of("Graven Image 2: Drop First Stone", "Drop Stone {spotSpeech}", "Drop Stone {spot}");
    public readonly Callout graven2avoidFirstStone =
        Callout.Of("Graven Image 2: Drop First Stone", "Run to middle")
            .In(1, "Graven Image 2: Avoid First Stone");

    public readonly Callout graven2westSafe1 = Callout.Of("Graven Image 2: West Safe", "West Safe");
    public readonly Callout graven2eastSafe1 = Callout.Of("Graven Image 2: East Safe", "East Safe");

    public readonly Callout graven2stone2 = Callout.Of("Graven Image 2: Second Stone", "Stone");
    public readonly Callout graven2dark2 = Callout.Of("Graven Image 2: Second Dark", "Dark");

    public readonly Callout graven2dropSecondStone =
        Callout.Of("Graven Image 2: Drop Second Stone", "Drop Stone {spotSpeech}", "Drop Stone {spot}");
    public readonly Callout graven2avoidSecondStone =
        Callout.Of("Graven Image 2: Drop Second Stone", "Run to middle")
            .In(1, "Graven Image 2: Avoid Second Stone");

    public readonly Callout gravenConfetti2 =
        Callout.Duration("Graven Image 2: Knockback on You", "{safeSpot2} Safe, Knockback")
            .AutoIcon().In(1, "Graven Image 2: Knockback on You");
    public readonly Callout gravenNoConfetti2 =
        Callout.Duration("Graven Image 2: Knockback on You", "{safeSpot2} Safe, Knockback on {confettiPlayers}")
            .In(1, "Graven Image 2: Knockback on Others");

    public readonly Callout gravenFinalSoaks =
        Callout.Of("Graven Image 2: Final Soaks", "Final Soaks");

    public const string Group = "gravenImage";

    public Sequence Build(IWorld world) =>
        Sequence.Multi(Group, 120,
            e => e.Is(EventKind.CastStart, GravenImageCast),
            (start, run) => First(start, run, world),
            (start, run) => Second(start, run, world));

    private async Task First(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(gravenImage, start);

        var initialTethers = await run.WaitEventsQuickSuccession(4, e => e.Kind == EventKind.Tether);
        var myTether = initialTethers.FirstOrDefault(t => t.EitherEnd(a => a.IsYou));
        if (myTether is not null) run.Call(graven1Tether, myTether);
        else run.Call(graven1NoTether);

        var kefkaMarkers = await run.WaitEvents(2, EventKind.HeadMarker, e => e.Target?.BaseId == Kefka);
        var playerMarker = await run.WaitEvent(EventKind.HeadMarker, FireSpread, FireStack);

        var fakeFire = kefkaMarkers.Any(m => m.Id == FakeFire);
        var fakeIce = kefkaMarkers.Any(m => m.Id == FakeIce);
        var presentSpread = playerMarker.Id == FireSpread;
        var actuallySpread = presentSpread != fakeFire;

        run.SetParam("fakeFire", fakeFire);
        run.SetParam("fakeIce", fakeIce);

        var first = kefkaMarkers[0];
        if (actuallySpread) run.Call(fakeIce ? gravenFakeIceSpread : gravenRealIceSpread, first);
        else run.Call(fakeIce ? gravenFakeIceStack : gravenRealIceStack, first);

        await run.WaitMs(6_000);
        run.Call(gravenSpreadForLaser);

        var laserTargets = await run.WaitEventsQuickSuccession(
            4, e => e.Is(EventKind.AbilityHit, Laser) && e.FirstTarget);

        var myLaser = laserTargets.FirstOrDefault(t => t.Target?.IsYou == true);
        if (myLaser is not null)
        {
            run.Call(gravenAvoidTower, myLaser);
        }
        else
        {
            var towerCast = await run.FindOrWaitForCast(world, e => e.Id == TowerCast);
            if (towerCast is not null) run.Call(gravenTakeTower, towerCast);
        }

        var confettis = await run.WaitEventsQuickSuccession(
            2, e => e.Is(EventKind.StatusGain, Confetti));
        var confettiPlayers = confettis.Select(c => c.Target).OfType<Actor>().ToList();
        run.SetParam("confettiPlayers", confettiPlayers);

        var mine = confettis.FirstOrDefault(c => c.Target?.IsYou == true);
        if (mine is not null) run.Call(gravenConfetti, mine);
        else if (confettis.Count > 0) run.Call(gravenNoConfetti, confettis[0]);

        var secondMarkers = await run.WaitEvents(2, EventKind.HeadMarker, e => e.Target?.BaseId == Kefka);
        var fakeThunder = secondMarkers.Any(m => m.Id == FakeThunder);
        var fakeIce2 = secondMarkers.Any(m => m.Id == FakeIce);

        run.SetParam("fakeThunder", fakeThunder);
        run.SetParam("fakeIce", fakeIce2);

        var second = secondMarkers[0];
        if (fakeThunder)
            run.Call(fakeIce2 ? gravenFakeIceFakeThunder : gravenRealIceFakeThunder, second);
        else
            run.Call(fakeIce2 ? gravenFakeIceRealThunder : gravenRealIceRealThunder, second);
    }

    private async Task Second(GameEvent start, SequenceRun run, IWorld world)
    {
        var confettis = world.ActiveStatuses().Where(s => s.Id == Confetti).ToList();
        var confettiPlayers = confettis.Select(c => c.Target).OfType<Actor>().ToList();
        run.SetParam("confettiPlayers", confettiPlayers);

        var tethers = await run.WaitEventsQuickSuccession(
            8, e => e.Is(EventKind.Tether, StoneTether));
        await run.Settle();

        var myTether = tethers.FirstOrDefault(t => t.EitherEnd(a => a.IsYou));
        if (myTether is null) return;

        var anchor = myTether.OtherEnd(a => a.IsYou);
        if (anchor is null) return;

        var playerStone = (world.Latest(anchor)?.Pos.X ?? anchor.Pos.X) > 120.0f;
        run.SetParam("playerStone", playerStone);

        var bossMarker = await run.WaitEvent(EventKind.HeadMarker, e => e.Target?.BaseId == Kefka);
        if (bossMarker.Id == FakeIce)
            run.Call(playerStone ? graven2fakeIceStone : graven2fakeIceDark);
        else
            run.Call(playerStone ? graven2realIceStone : graven2realIceDark);

        await run.WaitEvent(EventKind.AbilityHit, FirstStoneDrop);
        run.Call(playerStone ? graven2dropFirstStone : graven2avoidFirstStone);

        await run.WaitEvent(EventKind.AbilityHit, HandActivate);
        var firstHand = await run.WaitEvent(
            EventKind.ActorControl,
            e => e.Id == GlowingHandControl && e.Arg1 == GlowingHandArg1 && e.Arg2 == GlowingHandArg2 && e.Arg3 == 0 && e.Arg4 == 0);
        await run.Settle();

        var westSafeFirst = (world.Latest(firstHand.Target!)?.Pos.X ?? firstHand.Target!.Pos.X) > 100.0f;
        run.Call(westSafeFirst ? graven2westSafe1 : graven2eastSafe1);

        var secondTethers = await run.WaitEventsQuickSuccession(
            8, e => e.Is(EventKind.Tether, StoneTether));
        await run.Settle();

        var mySecond = secondTethers.FirstOrDefault(t => t.EitherEnd(a => a.IsYou));
        if (mySecond is null) return;

        var secondAnchor = mySecond.OtherEnd(a => a.IsYou);
        if (secondAnchor is null) return;

        playerStone = (world.Latest(secondAnchor)?.Pos.X ?? secondAnchor.Pos.X) > 120.0f;
        run.SetParam("playerStone", playerStone);
        run.Call(playerStone ? graven2stone2 : graven2dark2);

        await run.WaitEvent(EventKind.AbilityHit, FirstStoneDrop);
        run.Call(playerStone ? graven2dropSecondStone : graven2avoidSecondStone);

        var secondHand = await run.WaitEvent(
            EventKind.ActorControl,
            e => e.Id == GlowingHandControl && e.Arg1 == GlowingHandArg1 && e.Arg2 == GlowingHandArg2 && e.Arg3 == 0 && e.Arg4 == 0);
        await run.Settle(200);

        var westSafeSecond = (world.Latest(secondHand.Target!)?.Pos.X ?? secondHand.Target!.Pos.X) > 100.0f;
        run.SetParam("safeSpot2", westSafeSecond ? "West" : "East");

        var myConfetti = confettis.FirstOrDefault(c => c.Target?.IsYou == true);
        var knock = myConfetti ?? (confettis.Count > 0 ? confettis[0] : null);
        CallTicket? knockCall = null;
        if (knock is not null)
            knockCall = run.Call(myConfetti is not null ? gravenConfetti2 : gravenNoConfetti2, knock);

        await run.WaitEvent(EventKind.AbilityHit, ConfettiKnockback);
        knockCall?.ForceExpire();
        await run.WaitMs(1_000);
        run.Call(gravenFinalSoaks);
    }
}
