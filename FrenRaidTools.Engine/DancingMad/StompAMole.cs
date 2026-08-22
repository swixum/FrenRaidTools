namespace FrenRaidTools.Engine.DancingMad;

public sealed class StompAMole
{
    public const string Group = "stompAMole";

    public const int PhaseNumber = 3;

    public const string MechanicName = "Stomp-a-Mole";

    public const uint BlizzardThreeCast = 0xBB0F;
    public const uint BlizzardCast = 0xBB0D;
    public const uint StackMarker = 0xA1;
    public const uint KnockDownHit = 0xBB03;
    public const uint StompHit = 0xBAF0;
    public const uint BigBangCast = 0xBB11;

    public const uint MeteorFail = 0xC258;
    public const uint BowelsFail = 0xC259;
    public const uint MeteorEnrage = 0xC61E;
    public const uint BowelsEnrage = 0xC61F;

    public readonly Callout stompAMole =
        Callout.Duration("Stomp-a-Mole: Initial Cast", "Bait Blizzards then Stacks");

    public readonly Callout stompAMoleMove1 =
        Callout.Duration("Stomp-a-Mole: Move 1", "Move");

    public readonly Callout stompAMoleMiddleStack =
        Callout.Of("Stomp-a-Mole: Middle Stack (Marker on Your Role)", "Middle Stack").Note("{stackOn} is the player who took the marker, so {stackOn.name} and {stackOn.support} are both available if you would rather hear who has it.");

    public readonly Callout stompAMoleTakeTower =
        Callout.Of("Stomp-a-Mole: Take Tower (Marker on Other Role)", "Take Tower").Note("With a plan loaded this line is the plan's own tower side for your seat.");

    public readonly Callout stompAMoleStackMarker1 =
        Callout.Of("Stomp-a-Mole: Stack Marker (Your Role Unknown)", "Stack on {stackOn.support ? 'Support' : 'DPS'}");

    public readonly Callout stompAMoleMove2 =
        Callout.Duration("Stomp-a-Mole: Move 2", "Move");

    public readonly Callout stompAMoleSwitch =
        Callout.Of("Stomp-a-Mole: Swap", "Swap").Quiet()
            .Note("Said once, the first time a stomp or knockdown lands on you. The marker calls name the new job after that.");

    public readonly Callout bigBangAndB3 =
        Callout.Duration("Stomp-a-Mole: Blizzard + Big Bang", "Out of middle, Keep Moving");

    public readonly Callout p3normalEnrage =
        Callout.Duration("P3 Enrage (Normal)", "Enrage");

    public readonly Callout p3bowelsEnrage =
        Callout.Duration("P3 Enrage (Failed)", "Failed");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 90, e => e.Is(EventKind.CastStart, BlizzardThreeCast),
            (start, run) => Run(start, run, world));

    private void MarkerCall(SequenceRun run, IWorld world, GameEvent marker)
    {
        run.SetParam("stackOn", marker.Target);

        var yourJob = world.You?.Job;
        var markedJob = marker.Target?.Job;
        if (string.IsNullOrEmpty(yourJob) || string.IsNullOrEmpty(markedJob))
        {
            run.Call(stompAMoleStackMarker1, marker);
            return;
        }

        var yours = JobKinds.Support(markedJob) == JobKinds.Support(yourJob);
        run.Call(yours ? stompAMoleMiddleStack : stompAMoleTakeTower, marker);
    }

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(stompAMole, start);

        var first = await run.WaitEvent(EventKind.CastStart, BlizzardCast);
        run.Call(stompAMoleMove1, first);

        var stackMarker = await run.WaitEvent(EventKind.HeadMarker, StackMarker);
        MarkerCall(run, world, stackMarker);

        var second = await run.WaitEvent(EventKind.CastStart, BlizzardCast);
        run.Call(stompAMoleMove2, second);

        var swapped = false;

        while (true)
        {
            var e = await run.WaitEvent(x =>
                x.Is(EventKind.HeadMarker, StackMarker)
                || x.Is(EventKind.CastStart, BigBangCast)
                || ((x.Is(EventKind.AbilityHit, KnockDownHit) || x.Is(EventKind.AbilityHit, StompHit))
                    && x.Target?.IsYou == true));

            if (e.Is(EventKind.CastStart, BigBangCast))
            {
                var bigBang = run.Call(bigBangAndB3, e);
                await run.WaitCastFinished(e);
                bigBang.ForceExpire();
                break;
            }

            if (e.Kind == EventKind.HeadMarker)
            {
                MarkerCall(run, world, e);
                continue;
            }

            if (swapped) continue;

            swapped = true;
            run.Call(stompAMoleSwitch, e);
        }

        var enrages = await run.WaitEventsQuickSuccession(
            2, e => e.Is(EventKind.CastStart, MeteorEnrage, BowelsEnrage, MeteorFail, BowelsFail));

        var bad = enrages.FirstOrDefault(e => e.Id is MeteorFail or BowelsFail);
        if (bad is not null)
        {
            run.Call(p3bowelsEnrage, bad);
        }
        else if (enrages.Count > 0)
        {
            var ticket = run.Call(p3normalEnrage, enrages[0]);
            await run.WaitCastFinished(enrages[0]);
            ticket.ForceExpire();
        }
    }
}
