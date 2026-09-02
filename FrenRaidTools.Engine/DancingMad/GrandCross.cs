namespace FrenRaidTools.Engine.DancingMad;

public sealed class GrandCross
{
    public const string Group = "grandCross";

    public const int PhaseNumber = 4;

    public const string MechanicName = "Grand Cross";

    public const uint GrandCrossCast = 0xBB14;
    public const uint InfernoCast = 0xBB20;
    public const uint TsunamiCast = 0xBB21;

    public const uint NpcChaos = 19507;
    public const uint NpcNeoExdeath = 19510;

    public const uint RealNeoExdeath = 1122;
    public const uint FakeNeoExdeath = 1121;
    public const uint RealChaos = 1120;
    public const uint FakeChaos = 1119;

    public const double CrossSpeechDelaySeconds = 2.0;
    public const double LastCrossSpeechDelaySeconds = 4.0;

    public readonly Callout grandCrossRaidwide =
        Callout.Duration("Grand Cross: Raidwide", "Raidwide")
            .Note("Every Grand Cross deals raidwide damage, real or fake. Real and fake describe the cross pattern, not the hit. Measured on the 8 August clear: three casts, three hits landing on all eight players, one of them real. The countdown runs on the 8.7 second cast, so zero is the damage.")
            .At("Raidwides");

    public readonly Callout infernoRaidwide =
        Callout.Duration("Inferno: Raidwide", "Raidwide")
            .Note("Inferno hits all eight whether Chaos is telling the truth or not. Doubles up with the Inferno call above; turn one of them off if you would rather hear only one.")
            .At("Raidwides");

    public readonly Callout tsunamiRaidwide =
        Callout.Duration("Tsunami: Raidwide", "Raidwide")
            .Note("Tsunami hits all eight whether Chaos is telling the truth or not. Doubles up with the Tsunami call above; turn one of them off if you would rather hear only one.")
            .At("Raidwides");

    public readonly Callout grandCross1 =
        Callout.Duration("Grand Cross 1", "{real ? 'Real' : 'Fake'} Cross").SpeakAfter(CrossSpeechDelaySeconds).Note("The spoken line is held back a couple of seconds so it does not talk over the other calls landing at the same moment. The text on screen still shows straight away.")
            .At("Crosses");

    public readonly Callout grandCross2 =
        Callout.Duration("Grand Cross 2", "{real ? 'Real' : 'Fake'} Cross").SpeakAfter(CrossSpeechDelaySeconds)
            .At("Crosses");

    public readonly Callout grandCross3 =
        Callout.Duration("Grand Cross 3", "{real ? 'Real' : 'Fake'} Cross").SpeakAfter(LastCrossSpeechDelaySeconds)
            .At("Crosses");

    public readonly Callout inferno1 =
        Callout.Duration("Inferno 1", "{real ? 'Real' : 'Fake'} Inferno")
            .At("Inferno");

    public readonly Callout inferno2 =
        Callout.Duration("Inferno 2", "{real ? 'Real' : 'Fake'} Inferno")
            .At("Inferno");

    public readonly Callout tsunami1 =
        Callout.Duration("Tsunami 1", "{real ? 'Real' : 'Fake'} Tsunami")
            .At("Tsunami");

    public readonly Callout tsunami2 =
        Callout.Duration("Tsunami 2", "{real ? 'Real' : 'Fake'} Tsunami")
            .At("Tsunami");

    public VfxTracker Vfx { get; set; } = new();

    private double _raidwideClearAt;

    public void ForgetRaidwideQueue() => _raidwideClearAt = 0.0;

    private async Task Raidwide(Callout call, GameEvent start, SequenceRun run)
    {
        var lands = start.At + start.Duration;
        var wait = Math.Min(_raidwideClearAt, lands) - start.At;

        _raidwideClearAt = Math.Max(_raidwideClearAt, lands);

        if (wait > 0) await run.WaitSeconds(wait);

        run.Call(call, start);
    }

    public Sequence Build(IWorld world) =>
        Sequence.Indexed(Group, 10, e => e.Is(EventKind.CastStart, GrandCrossCast),
            async (start, run, invocation) =>
            {
                run.SetParam("real", Vfx.NeoExdeathReal);

                if (Vfx.NeoExdeathReal == true)
                    run.Call(invocation switch
                    {
                        0 => grandCross1,
                        1 => grandCross2,
                        _ => grandCross3,
                    }, start);

                await Raidwide(grandCrossRaidwide, start, run);
            });

    public Sequence BuildInfernoTsunami(IWorld world) =>
        Sequence.Indexed(Group + "InfernoTsunami", 10,
            e => e.Is(EventKind.CastStart, InfernoCast, TsunamiCast),
            async (start, run, invocation) =>
            {
                run.SetParam("real", Vfx.ChaosReal);
                var isInferno = start.Id == InfernoCast;

                run.Call(invocation == 0
                    ? isInferno ? inferno1 : tsunami1
                    : isInferno ? inferno2 : tsunami2, start);

                await Raidwide(isInferno ? infernoRaidwide : tsunamiRaidwide, start, run);
            });
}

public sealed class VfxTracker
{
    public bool? NeoExdeathReal { get; private set; }

    public bool? ChaosReal { get; private set; }

    public void Take(GameEvent e)
    {
        if (e.Kind != EventKind.StatusLoopVfx) return;

        var on = e.Target?.BaseId ?? 0;

        if (on == GrandCross.NpcNeoExdeath)
        {
            if (e.Id == GrandCross.RealNeoExdeath) NeoExdeathReal = true;
            else if (e.Id == GrandCross.FakeNeoExdeath) NeoExdeathReal = false;
        }
        else if (on == GrandCross.NpcChaos)
        {
            if (e.Id == GrandCross.RealChaos) ChaosReal = true;
            else if (e.Id == GrandCross.FakeChaos) ChaosReal = false;
        }
    }

    public void Reset()
    {
        NeoExdeathReal = null;
        ChaosReal = null;
    }
}
