namespace FrenRaidTools.Engine.DancingMad;

public sealed class KefkaSays
{
    public const string Group = "kefkaSays";

    public const int PhaseNumber = 4;

    public const string MechanicName = "Kefka Says";

    public const uint KefkaSaysCast = 0xC2DC;
    public const uint NpcKefka = 18475;

    public const uint FakeIce = 675;
    public const uint FakeThunder = 677;

    public const int MysteryMagicSets = 3;

    public const uint NpcNeoExdeath = 19510;
    public const uint NpcChaos = 19507;
    public const uint RealNeoExdeath = 1122;
    public const uint RealChaos = 1120;

    public const double ShortAccelSeconds = 60;
    public const double SecondAccelSeconds = 45;
    public const int DebuffBurst = 10;
    public const int WoundBurst = 16;

    public const double BombSetSeconds = 20;
    public const double ShriekSeconds = 15;
    public const double FirstSetAfterBeamSeconds = 7.77;

    public const uint BlackCastReal = 0xC395;
    public const uint BlackCastFake = 0xC394;
    public const double BlackCleaveDistance = 20;

    public static string? OrbSide(ArenaSector orbs, ArenaSector half)
    {
        if (!orbs.IsPoint() || !half.IsPoint()) return null;

        return orbs.EighthsTo(half) switch
        {
            0 or 1 or 7 => "at orbs",
            3 or 4 or 5 => "away from orbs",
            2 => "right of orbs",
            _ => "left of orbs",
        };
    }

    public static readonly ArenaPos TightAp = new(100, 100, 5, 5);

    public static ArenaSector Relative(ArenaSector spot, ArenaSector towardBoss) =>
        !spot.IsPoint() || !towardBoss.IsPoint()
            ? spot
            : spot.PlusEighths(towardBoss.EighthsTo(ArenaSector.North));

    public const uint ChaosEntropy = 0x15AB;
    public const uint ChaosDynamic = 0x15AC;
    public const uint ThunderCharged = 0x5CD;
    public const uint ManaReleaseCast = 0xBAA5;

    public static readonly uint[] DonutCasts = [0xBB22, 0xBB23, 0xBB24, 0xBB25];

    public const double FirstLongSeconds = 75;
    public const double SecondLongSeconds = 60;
    public const double RecentSeconds = 5;
    public const double ChaosFirstDelayMs = 1500;
    public const double ChaosSecondDelayMs = 2000;
    public const double ChaosVfxGapMs = 5000;
    public const double ThunderChargedDelayMs = 6300;
    public const double DynEntSpeechDelaySeconds = 2.0;

    public VfxTracker Vfx { get; set; } = new();

    public const uint ACCEL = 0x15AA;
    public const uint ALLAG_FIELD = 0x1C6;
    public const uint BEYOND_DEATH = 0x566;
    public const uint BEYOND_DEATH_FAKE = 0x1558;
    public const uint BLACK_WOUND = 0x15A6;
    public const uint BLACK_WOUND_FAKE = 0x1318;
    public const uint FORK = 0x15A8;
    public const uint SHRIEK = 0x15A7;
    public const uint WATER = 0x15A9;
    public const uint WHITE_WOUND = 0x15A5;
    public const uint WHITE_WOUND_FAKE = 0x1317;

    public const uint DYNAMIC = 0x641;
    public const uint ENTROPY = 0x640;

    public readonly Callout kefkaSays =
        Callout.Duration("Kefka Says", "Kefka Says");

    public readonly Callout realIceRealThunder =
        Callout.Of("Kefka Says: Real Ice, Real Thunder (All Sets)", "Avoid Both");

    public readonly Callout realIceFakeThunder =
        Callout.Of("Kefka Says: Real Ice, Fake Thunder (All Sets)", "In Thunder, Avoid Ice");

    public readonly Callout fakeIceRealThunder =
        Callout.Of("Kefka Says: Fake Ice, Real Thunder (All Sets)", "In Ice, Avoid Thunder");

    public readonly Callout fakeIceFakeThunder =
        Callout.Of("Kefka Says: Fake Ice, Fake Thunder (All Sets)", "Stand in Both");

    public readonly Callout realAccelShort =
        Callout.Duration("Kefka Says: Real Accel, Short (First Set Applied)", "Real Short Accel").Icon(ACCEL);

    public readonly Callout realAccelShortShriek =
        Callout.Duration("Kefka Says: Real Accel, Short, with Shriek (First Set Applied)", "Real Short + Shriek").Icon(SHRIEK, ACCEL);

    public readonly Callout realAccelLong =
        Callout.Duration("Kefka Says: Real Accel, Long (First Set Applied)", "Real Long Accel").Icon(ACCEL);

    public readonly Callout realAccelLongShriek =
        Callout.Duration("Kefka Says: Real Accel, Long, with Shriek (First Set Applied)", "Real Long + Shriek").Icon(SHRIEK, ACCEL);

    public readonly Callout realWater =
        Callout.Duration("Kefka Says: Real Water (First Set Applied)", "Real Water").Icon(WATER);

    public readonly Callout realLightning =
        Callout.Duration("Kefka Says: Real Lightning (First Set Applied)", "Real Lightning").Icon(FORK);

    public readonly Callout fakeAccelShort =
        Callout.Duration("Kefka Says: Fake Accel, Short (First Set Applied)", "Fake Short Accel").Icon(ACCEL);

    public readonly Callout fakeAccelShortShriek =
        Callout.Duration("Kefka Says: Fake Accel, Short, with Shriek (First Set Applied)", "Fake Short Accel").Icon(SHRIEK, ACCEL);

    public readonly Callout fakeAccelLong =
        Callout.Duration("Kefka Says: Fake Accel, Long (First Set Applied)", "Fake Long Accel").Icon(ACCEL);

    public readonly Callout fakeAccelLongShriek =
        Callout.Duration("Kefka Says: Fake Accel, Long, with Shriek (First Set Applied)", "Fake Long + Shriek").Icon(SHRIEK, ACCEL);

    public readonly Callout fakeWater =
        Callout.Duration("Kefka Says: Fake Water (First Set Applied)", "Fake Water").Icon(WATER);

    public readonly Callout fakeLightning =
        Callout.Duration("Kefka Says: Fake Lightning (First Set Applied)", "Fake Lightning").Icon(FORK);

    public readonly Callout realDynamicFluid =
        Callout.Duration("Kefka Says: Real Dynamic Fluid (First Set Applied)", "Real Water").Icon(DYNAMIC).Quiet().SpeakAfter(DynEntSpeechDelaySeconds).Note("Note that the first/second set refer to the order in which they will RESOLVE, not apply. These calls are disabled by default. I would recommend keeping it this way as there is a lot going on at that point, and there is another call later to remind you of what to do with your debuff anyway.");

    public readonly Callout realEntropy =
        Callout.Duration("Kefka Says: Real Entropy (First Set Applied)", "Real Fire").Icon(ENTROPY).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout fakeDynamicFluid =
        Callout.Duration("Kefka Says: Fake Dynamic Fluid (First Set Applied)", "Fake Water").Icon(DYNAMIC).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout fakeEntropy =
        Callout.Duration("Kefka Says: Fake Entropy (First Set Applied)", "Fake Fire").Icon(ENTROPY).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout secondRealDynamicFluid =
        Callout.Duration("Kefka Says: Real Dynamic Fluid (Second Set Applied)", "Real Water").Icon(DYNAMIC).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout secondRealEntropy =
        Callout.Duration("Kefka Says: Real Entropy (Second Set Applied)", "Real Fire").Icon(ENTROPY).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout secondFakeDynamicFluid =
        Callout.Duration("Kefka Says: Fake Dynamic Fluid (Second Set Applied)", "Fake Water").Icon(DYNAMIC).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout secondFakeEntropy =
        Callout.Duration("Kefka Says: Fake Entropy (Second Set Applied)", "Fake Fire").Icon(ENTROPY).Quiet().SpeakAfter(DynEntSpeechDelaySeconds);

    public readonly Callout secondRealAccelShort =
        Callout.Duration("Kefka Says: Real Accel, Short (Second Set Applied)", "Real Short Accel").Icon(ACCEL);

    public readonly Callout secondRealAccelShortShriek =
        Callout.Duration("Kefka Says: Real Accel, Short, with Shriek (Second Set Applied)", "Real Short + Shriek").Icon(SHRIEK, ACCEL);

    public readonly Callout secondRealAccelLong =
        Callout.Duration("Kefka Says: Real Accel, Long (Second Set Applied)", "Real Long Accel").Icon(ACCEL);

    public readonly Callout secondRealAccelLongShriek =
        Callout.Duration("Kefka Says: Real Accel, Long, with Shriek (Second Set Applied)", "Real Long + Shriek").Icon(SHRIEK, ACCEL);

    public readonly Callout secondRealWater =
        Callout.Duration("Kefka Says: Real Water (Second Set Applied)", "Real Water").Icon(WATER);

    public readonly Callout secondRealLightning =
        Callout.Duration("Kefka Says: Real Lightning (Second Set Applied)", "Real Lightning").Icon(FORK);

    public readonly Callout secondFakeAccelShort =
        Callout.Duration("Kefka Says: Fake Accel, Short (Second Set Applied)", "Fake Short Accel").Icon(ACCEL);

    public readonly Callout secondFakeAccelShortShriek =
        Callout.Duration("Kefka Says: Fake Accel, Short, with Shriek (Second Set Applied)", "Fake Short Accel").Icon(SHRIEK, ACCEL);

    public readonly Callout secondFakeAccelLong =
        Callout.Duration("Kefka Says: Fake Accel, Long (Second Set Applied)", "Fake Long Accel").Icon(ACCEL);

    public readonly Callout secondFakeAccelLongShriek =
        Callout.Duration("Kefka Says: Fake Accel, Long, with Shriek (Second Set Applied)", "Fake Long + Shriek").Icon(SHRIEK, ACCEL);

    public readonly Callout secondFakeWater =
        Callout.Duration("Kefka Says: Fake Water (Second Set Applied)", "Fake Water").Icon(WATER);

    public readonly Callout secondFakeLightning =
        Callout.Duration("Kefka Says: Fake Lightning (Second Set Applied)", "Fake Lightning").Icon(FORK);

    public readonly Callout realWhiteDeath =
        Callout.Duration("Kefka Says: Real WW + BD", "Real White + Death").Icon(WHITE_WOUND, BEYOND_DEATH);

    public readonly Callout fakeWhiteDeath =
        Callout.Duration("Kefka Says: Fake WW + BD", "Fake White + Death").Icon(WHITE_WOUND, BEYOND_DEATH);

    public readonly Callout realBlackDeath =
        Callout.Duration("Kefka Says: Real BW + BD", "Real Black + Death").Icon(BLACK_WOUND, BEYOND_DEATH);

    public readonly Callout fakeBlackDeath =
        Callout.Duration("Kefka Says: Fake BW + BD (Applied)", "Fake Black + Death").Icon(BLACK_WOUND, BEYOND_DEATH);

    public readonly Callout realWhiteAllag =
        Callout.Duration("Kefka Says: Real WW + AF", "Real White + Allag").Icon(WHITE_WOUND, ALLAG_FIELD);

    public readonly Callout fakeWhiteAllag =
        Callout.Duration("Kefka Says: Fake WW + AF", "Fake White + Allag").Icon(WHITE_WOUND, ALLAG_FIELD);

    public readonly Callout realBlackAllag =
        Callout.Duration("Kefka Says: Real BW + AF", "Real Black + Allag").Icon(BLACK_WOUND, ALLAG_FIELD);

    public readonly Callout fakeBlackAllag =
        Callout.Duration("Kefka Says: Fake BW + AF", "Fake Black + Allag").Icon(BLACK_WOUND, ALLAG_FIELD);

    public readonly Callout kefkaSaysError =
        Callout.Of("Kefka Says: Missing/Invalid Debuffs", "Error");

    public readonly Callout standInWhite =
        Callout.Duration("Kefka Says: Stand in White", "Stand in Purple ({whiteCompass})");

    public readonly Callout standInBlack =
        Callout.Duration("Kefka Says: Stand in Black", "Stand in Blue ({blackCompass})");

    public const string MarkerParam = "myMarker";
    public const string NextSpotParam = "next";

    public const string SpreadSupport = "D";
    public const string SpreadDps = "B";
    public const string StackSupport = "A";
    public const string StackDps = "C";

    public static string? Waymark(string job, bool spread) => JobKinds.Kind(job) switch
    {
        JobKind.Tank or JobKind.Healer => spread ? SpreadSupport : StackSupport,
        JobKind.Melee or JobKind.PhysRanged or JobKind.Caster => spread ? SpreadDps : StackDps,
        _ => null,
    };

    public static string? Waymark(IWorld world, bool spread) =>
        world.You is { } you ? Waymark(you.Job, spread) : null;

    private (bool Spreading, string? Marker)? _secondSpot;

    public void NoteSecondSpot(bool spreading, string? marker) => _secondSpot = (spreading, marker);

    public void ForgetSecondSpot() => _secondSpot = null;

    public string? SecondSpotSuffix() =>
        _secondSpot is { Marker: { } marker } spot
            ? $" and {(spot.Spreading ? "Spread" : "Stack")} {marker}"
            : null;

    public readonly Callout firstSetStack =
        Callout.Duration("Kefka Says: First Debuff Set Resolving: Stack", "Stack {myMarker}").AutoIcon();

    public readonly Callout firstSetSpread =
        Callout.Duration("Kefka Says: First Debuff Set Resolving: Spread", "Spread {myMarker}").AutoIcon();

    public readonly Callout firstSetNothing =
        Callout.Duration("Kefka Says: First Debuff Set Resolving: Nothing", "Stack {myMarker}").AutoIcon();

    public readonly Callout firstSetAccelStack =
        Callout.Duration("Kefka Says: First Debuff Set Resolving: Accel + Stack", "{stillness ? 'Stillness' : 'Motion'} and Stack {myMarker}").AutoIcon();

    public readonly Callout firstSetAccelSpread =
        Callout.Duration("Kefka Says: First Debuff Set Resolving: Accel + Spread", "{stillness ? 'Stillness' : 'Motion'} and Spread {myMarker}").AutoIcon();

    public readonly Callout firstSetAccelNothing =
        Callout.Duration("Kefka Says: First Debuff Set Resolving: Accel + Nothing", "{stillness ? 'Stillness' : 'Motion'} and Stack {myMarker}").AutoIcon();

    public readonly Callout thunderShriek =
        Callout.Duration("Kefka Says: Thunder and First Shrieks Resolving", "{fakeThunder ? 'In' : 'Avoid'} Thunder, Look {fakeShriek ? 'In' : 'Away'}").Icon(SHRIEK);

    public readonly Callout thunderShriekOnYou =
        Callout.Duration("Kefka Says: Thunder and First Shrieks Resolving (Shriek on You)", "{fakeThunder ? 'In' : 'Avoid'} Thunder, Look {fakeShriek ? 'In' : 'Away'}, on YOU").Icon(SHRIEK);

    public readonly Callout firstEntropyDynamic =
        Callout.Duration("Kefka Says: First Entropy/Dynamic Resolving", "{isDonut ? 'Stack for Donut' : 'Stack then Move'}").AutoIcon();

    public readonly Callout firstEntropyDynamicMove =
        Callout.Duration("Kefka Says: First Entropy/Dynamic: Move (Circle Aoe)", "Move{next}").AutoIcon()
            .Note("When your second stack or spread is known, {next} becomes the spot it sends you to, so this reads like Move and Spread B. Supports take D on a spread and A on a stack, dps take B and C. Holding neither a fork nor a water for that set leaves it as plain Move.");

    public readonly Callout firstEntropyDynamicStay =
        Callout.Duration("Kefka Says: First Entropy/Dynamic: Stay (Donut AoE)", "Stay{next}").AutoIcon()
            .Note("When your second stack or spread is known, {next} becomes the spot it sends you to, so this reads like Stay and Spread B. Holding neither a fork nor a water for that set leaves it as plain Stay.");

    public readonly Callout secondSetStack =
        Callout.Duration("Kefka Says: Second Debuff Set Resolving: Stack", "Stack {myMarker} {fakeIce ? 'In Ice' : 'Out of Ice'}").AutoIcon();

    public readonly Callout secondSetSpread =
        Callout.Duration("Kefka Says: Second Debuff Set Resolving: Spread", "Spread {myMarker} {fakeIce ? 'In Ice' : 'Out of Ice'}").AutoIcon();

    public readonly Callout secondSetNothing =
        Callout.Duration("Kefka Says: Second Debuff Set Resolving: Nothing", "Stack {myMarker} {fakeIce ? 'In Ice' : 'Out of Ice'}").AutoIcon();

    public readonly Callout secondSetAccelStack =
        Callout.Duration("Kefka Says: Second Debuff Set Resolving: Accel + Stack", "{stillness ? 'Stillness' : 'Motion'} and Stack {myMarker} {fakeIce ? 'In Ice' : 'Out of Ice'}").AutoIcon();

    public readonly Callout secondSetAccelSpread =
        Callout.Duration("Kefka Says: Second Debuff Set Resolving: Accel + Spread", "{stillness ? 'Stillness' : 'Motion'} and Spread {myMarker} {fakeIce ? 'In Ice' : 'Out of Ice'}").AutoIcon();

    public readonly Callout secondSetAccelNothing =
        Callout.Duration("Kefka Says: Second Debuff Set Resolving: Accel + Nothing", "{stillness ? 'Stillness' : 'Motion'} and Stack {myMarker} {fakeIce ? 'In Ice' : 'Out of Ice'}").AutoIcon();

    public readonly Callout secondShriek =
        Callout.Duration("Kefka Says: Second Shrieks Resolving", "Look {fakeShriek ? 'In' : 'Away'}").Icon(SHRIEK);

    public readonly Callout secondShriekOnYou =
        Callout.Duration("Kefka Says: Second Shrieks Resolving (On You)", "Look {fakeShriek ? 'In' : 'Away'}, on YOU").Icon(SHRIEK);

    public readonly Callout secondEntropyDynamicBothReal =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic Resolving, Both Thunder/Ice Real", "{isDonut ? 'Donut' : 'Stack'}, Avoid Both").AutoIcon();

    public readonly Callout secondEntropyDynamicMoveBothReal =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Move, Both Thunder/Ice Real", "Move, Avoid Both").AutoIcon();

    public readonly Callout secondEntropyDynamicStayBothReal =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Stay, Both Thunder/Ice Real", "Stay out of Both").AutoIcon();

    public readonly Callout secondEntropyDynamicBothFake =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic Resolving, Both Thunder/Ice Fake", "{isDonut ? 'Donut' : 'Stack'} in Both").AutoIcon();

    public readonly Callout secondEntropyDynamicMoveBothFake =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Move, Both Thunder/Ice Fake", "Move, Into Both").AutoIcon();

    public readonly Callout secondEntropyDynamicStayBothFake =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Stay, Both Thunder/Ice Fake", "Stay in Both").AutoIcon();

    public readonly Callout secondEntropyDynamicFakeIce =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic Resolving, Real Thunder, Fake Ice", "{isDonut ? 'Donut' : 'Stack'} in Ice").AutoIcon();

    public readonly Callout secondEntropyDynamicMoveFakeIce =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Move, Real Thunder, Fake Ice", "Move Into Ice").AutoIcon();

    public readonly Callout secondEntropyDynamicStayFakeIce =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Stay, Real Thunder, Fake Ice", "Stay In Ice").AutoIcon();

    public readonly Callout secondEntropyDynamicFakeThunder =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic Resolving, Fake Thunder, Real Ice", "{isDonut ? 'Donut' : 'Stack'} in Thunder").AutoIcon();

    public readonly Callout secondEntropyDynamicMoveFakeThunder =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Move, Fake Thunder, Real Ice", "Move Into Thunder").AutoIcon();

    public readonly Callout secondEntropyDynamicStayFakeThunder =
        Callout.Duration("Kefka Says: Second Entropy/Dynamic: Stay, Fake Thunder, Real Ice", "Stay in Thunder").AutoIcon();

    public sealed record ManaCharge(bool ThunderFake, bool IceFake);

    public ManaCharge? LastManaCharge { get; private set; }

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, KefkaSaysCast),
            (start, run) => Run(start, run, world));

    private static bool OnKefka(GameEvent e) => e.Target?.BaseId == NpcKefka;

    private static bool NearKefka(GameEvent e) => e.EitherEnd(a => a.BaseId == NpcKefka);

    private static bool Yours(GameEvent e) => e.Target?.IsYou == true;

    private static GameEvent? ById(IEnumerable<GameEvent> debuffs, params uint[] ids) =>
        debuffs.FirstOrDefault(d => Array.IndexOf(ids, d.Id) >= 0);

    private static void MarkFake(HashSet<(uint Status, uint Target)> fake, IEnumerable<GameEvent> debuffs, bool real)
    {
        if (real) return;
        foreach (var debuff in debuffs)
            if (debuff.Target is { } target)
                fake.Add((debuff.Id, target.ObjectId));
    }

    private static bool IsFake(HashSet<(uint Status, uint Target)> fake, GameEvent? debuff) =>
        debuff?.Target is { } target && fake.Contains((debuff.Id, target.ObjectId));

    private static CallTicket On(SequenceRun run, Callout callout, GameEvent? on) =>
        on is null ? run.Call(callout) : run.Call(callout, on);

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(kefkaSays, start);

        for (var set = 0; set < MysteryMagicSets; set++)
            await MysteryMagic(run);

        await ManaChargeDetail(run);
    }

    private async Task MysteryMagic(SequenceRun run)
    {
        var markers = await run.WaitEvents(2, EventKind.HeadMarker, OnKefka);

        var fakeThunder = markers.Any(m => m.Id == FakeThunder);
        var fakeIce = markers.Any(m => m.Id == FakeIce);

        run.SetParam("fakeThunder", fakeThunder);
        run.SetParam("fakeIce", fakeIce);

        var first = markers[0];
        if (fakeThunder)
            run.Call(fakeIce ? fakeIceFakeThunder : realIceFakeThunder, first);
        else
            run.Call(fakeIce ? fakeIceRealThunder : realIceRealThunder, first);
    }

    private async Task ManaChargeDetail(SequenceRun run)
    {
        var pre1 = await run.WaitEvent(EventKind.HeadMarker, NearKefka);
        var thunderPre = pre1.Id == FakeThunder;

        var pre2 = await run.WaitEvent(EventKind.HeadMarker, NearKefka);
        var icePre = pre2.Id == FakeIce;

        var post = await run.WaitEvents(2, EventKind.HeadMarker, OnKefka);
        var thunderPost = post.Any(m => m.Id == FakeThunder);
        var icePost = post.Any(m => m.Id == FakeIce);

        LastManaCharge = new ManaCharge(thunderPre ^ thunderPost, icePre ^ icePost);

        run.SetParam("fakeThunder", LastManaCharge.ThunderFake);
        run.SetParam("fakeIce", LastManaCharge.IceFake);

        run.Raise(new GameEvent
        {
            Kind = EventKind.Synthetic,
            Id = Synthetic.ManaChargeDetail,
            At = post[^1].At,
            Arg1 = LastManaCharge.ThunderFake ? 1u : 0u,
            Arg2 = LastManaCharge.IceFake ? 1u : 0u,
        });
    }

    public Sequence BuildExdeath(IWorld world) =>
        Sequence.Repeat(Group + "Exdeath", 180, e => e.Is(EventKind.CastStart, KefkaSaysCast),
            (start, run) => Exdeath(start, run, world));

    private static bool IsElementDebuff(GameEvent e) =>
        e.Kind == EventKind.StatusGain && e.Id is FORK or WATER or SHRIEK or ACCEL;

    private static bool IsWoundDebuff(GameEvent e) =>
        e.Kind == EventKind.StatusGain &&
        e.Id is WHITE_WOUND or BLACK_WOUND or ALLAG_FIELD or BEYOND_DEATH
             or BEYOND_DEATH_FAKE or WHITE_WOUND_FAKE or BLACK_WOUND_FAKE;

    private async Task<bool> NeoExdeathReal(SequenceRun run)
    {
        var vfx = await run.WaitEvent(
            e => e.Kind == EventKind.StatusLoopVfx && e.Target?.BaseId == NpcNeoExdeath);
        return vfx.Id == RealNeoExdeath;
    }

    private CallTicket? ElementCall(
        SequenceRun run, bool real, double shortBelow,
        GameEvent? accel, GameEvent? shriek, GameEvent? water, GameEvent? fork,
        Callout realShort, Callout realShortShriek, Callout realLong, Callout realLongShriek,
        Callout fakeShort, Callout fakeShortShriek, Callout fakeLong, Callout fakeLongShriek,
        Callout realWater, Callout fakeWater, Callout realLightning, Callout fakeLightning)
    {
        if (accel is not null)
        {
            var isShort = accel.Duration < shortBelow;
            var call = isShort
                ? shriek is not null
                    ? real ? realShortShriek : fakeShortShriek
                    : real ? realShort : fakeShort
                : shriek is not null
                    ? real ? realLongShriek : fakeLongShriek
                    : real ? realLong : fakeLong;
            return run.Call(call, accel);
        }

        if (water is not null) return run.Call(real ? realWater : fakeWater, water);
        if (fork is not null) return run.Call(real ? realLightning : fakeLightning, fork);
        return null;
    }

    private async Task Exdeath(GameEvent start, SequenceRun run, IWorld world)
    {
        var fake = new HashSet<(uint Status, uint Target)>();

        await run.WaitEvents(2, EventKind.HeadMarker, OnKefka);

        var real1 = await NeoExdeathReal(run);
        var burst1 = await run.WaitEventsQuickSuccession(DebuffBurst, IsElementDebuff);
        MarkFake(fake, burst1, real1);
        var mine1 = burst1.Where(Yours).ToList();

        await run.WaitMs(100);

        var accel1 = ById(mine1, ACCEL);
        var shriek1 = ById(mine1, SHRIEK);
        var water1 = ById(mine1, WATER);
        var fork1 = ById(mine1, FORK);

        var ticket = ElementCall(run, real1, ShortAccelSeconds, accel1, shriek1, water1, fork1,
            realAccelShort, realAccelShortShriek, realAccelLong, realAccelLongShriek,
            fakeAccelShort, fakeAccelShortShriek, fakeAccelLong, fakeAccelLongShriek,
            realWater, fakeWater, realLightning, fakeLightning);

        await run.WaitEvents(2, EventKind.HeadMarker, OnKefka);
        ticket?.ForceExpire();

        var real2 = await NeoExdeathReal(run);
        var burst2 = await run.WaitEventsQuickSuccession(DebuffBurst, IsElementDebuff);
        MarkFake(fake, burst2, real2);
        var mine2 = burst2.Where(Yours).ToList();

        var accel2 = ById(mine2, ACCEL);
        var shriek2 = ById(mine2, SHRIEK);
        var water2 = ById(mine2, WATER);
        var fork2 = ById(mine2, FORK);

        ticket = ElementCall(run, real2, SecondAccelSeconds, accel2, shriek2, water2, fork2,
            secondRealAccelShort, secondRealAccelShortShriek, secondRealAccelLong, secondRealAccelLongShriek,
            secondFakeAccelShort, secondFakeAccelShortShriek, secondFakeAccelLong, secondFakeAccelLongShriek,
            secondRealWater, secondFakeWater, secondRealLightning, secondFakeLightning);

        await run.WaitEvents(2, EventKind.HeadMarker, OnKefka);
        ticket?.ForceExpire();

        var wounds = (await run.WaitEventsQuickSuccession(WoundBurst, IsWoundDebuff))
            .Where(Yours).ToList();

        var real3 = Vfx.NeoExdeathReal ?? real2;

        var myBD = ById(wounds, BEYOND_DEATH, BEYOND_DEATH_FAKE);
        var myAF = ById(wounds, ALLAG_FIELD);
        var myWW = ById(wounds, WHITE_WOUND, WHITE_WOUND_FAKE);
        var myBW = ById(wounds, BLACK_WOUND, BLACK_WOUND_FAKE);

        run.SetParam("myBD", myBD);
        run.SetParam("myAF", myAF);
        run.SetParam("myWW", myWW);
        run.SetParam("myBW", myBW);

        var shouldGetHit = (myBD is not null) ^ !real3;
        var whiteIsLethal = (myWW is not null) ^ !real3;
        var standInRealWhite = shouldGetHit == whiteIsLethal;

        CallTicket? wound = null;
        if (myBD is not null)
        {
            if (myWW is not null) wound = run.Call(real3 ? realWhiteDeath : fakeWhiteDeath, myBD);
            else if (myBW is not null) wound = run.Call(real3 ? realBlackDeath : fakeBlackDeath, myBD);
        }
        else if (myAF is not null)
        {
            if (myWW is not null) wound = run.Call(real3 ? realWhiteAllag : fakeWhiteAllag, myAF);
            else if (myBW is not null) wound = run.Call(real3 ? realBlackAllag : fakeBlackAllag, myAF);
        }

        wound ??= run.Call(kefkaSaysError);

        var real4 = await NeoExdeathReal(run);
        var whiteIsSafe = standInRealWhite == real4;

        var blackCast = await run.FindOrWaitForCast(world,
            e => e.Id == (real4 ? BlackCastReal : BlackCastFake));

        if (blackCast is not null)
        {
            var caster = blackCast.Source is null ? null : world.Latest(blackCast.Source) ?? blackCast.Source;
            if (caster is not null)
            {
                var cleaving = caster.Pos.Forward(caster.Heading, BlackCleaveDistance);
                var blackPos = TightAp.For(cleaving);
                var orbs = TightAp.For(caster.Pos);

                var body = world.NpcsById(GrandCross.NpcNeoExdeath)
                    .FirstOrDefault(n => n.Pos.Known);
                var toNeo = body is null ? ArenaSector.Unknown : TightAp.For(body.Pos);

                run.SetParam("blackCompass", Relative(blackPos, toNeo).Told());
                run.SetParam("whiteCompass", Relative(blackPos.Opposite(), toNeo).Told());
                run.SetParam("blackPos", OrbSide(orbs, blackPos) ?? blackPos.Told());
                run.SetParam("whitePos", OrbSide(orbs, blackPos.Opposite()) ?? blackPos.Opposite().Told());
            }

            run.Call(whiteIsSafe ? standInWhite : standInBlack, blackCast);
        }

        if (blackCast is not null) await run.WaitCastFinished(blackCast);

        BombSet(run, world, fake, first: true, fork1, fork2, accel1, accel2, water1, water2);
        NoteSecondSet(run, world, fake, fork1, fork2, water1, water2);

        var hm1 = await run.WaitEvent(EventKind.HeadMarker, NearKefka);
        run.SetParam("fakeThunder", hm1.Id == FakeThunder);

        var shortShriek = ShriekEnding(world, run, false);
        var shortShriekOnYou = ShriekEnding(world, run, true);
        run.SetParam("fakeShriek", IsFake(fake, shortShriek));
        On(run, shortShriekOnYou is null ? thunderShriek : thunderShriekOnYou, shortShriek);

        var hm2 = await run.WaitEvent(EventKind.HeadMarker, NearKefka);
        run.SetParam("fakeIce", hm2.Id == FakeIce);

        var held = BombSet(run, world, fake, first: false, fork1, fork2, accel1, accel2, water1, water2);
        await run.WaitStatusRemovedIfAny(held);

        var longShriek = ShriekEnding(world, run, false);
        var longShriekOnYou = ShriekEnding(world, run, true);
        run.SetParam("fakeShriek", IsFake(fake, longShriek));
        var lastShriek = On(run, longShriekOnYou is null ? secondShriek : secondShriekOnYou, longShriek);

        if (longShriek is not null)
        {
            await run.WaitStatusRemovedOrExpired(longShriek, 1.0);
            lastShriek.ForceExpire();
        }
    }

    private static GameEvent? ShriekEnding(IWorld world, SequenceRun run, bool onlyYou) =>
        world.ActiveStatuses().FirstOrDefault(s =>
            s.Id == SHRIEK && run.Remaining(s) < ShriekSeconds && (!onlyYou || Yours(s)));

    private void NoteSecondSet(
        SequenceRun run, IWorld world, HashSet<(uint Status, uint Target)> fake,
        GameEvent? fork1, GameEvent? fork2, GameEvent? water1, GameEvent? water2)
    {
        var fork = Longest(run, fork1, fork2);
        var water = Longest(run, water1, water2);

        bool spreading;
        if (fork is not null && (water is null || run.Remaining(fork) > run.Remaining(water)))
            spreading = !IsFake(fake, fork);
        else if (water is not null)
            spreading = IsFake(fake, water);
        else return;

        NoteSecondSpot(spreading, Waymark(world, spreading));
    }

    private static GameEvent? Longest(SequenceRun run, params GameEvent?[] statuses)
    {
        GameEvent? best = null;
        foreach (var status in statuses)
            if (status is not null && run.Remaining(status) > 0 &&
                (best is null || run.Remaining(status) > run.Remaining(best)))
                best = status;
        return best;
    }

    private GameEvent? BombSet(
        SequenceRun run, IWorld world, HashSet<(uint Status, uint Target)> fake, bool first,
        GameEvent? fork1, GameEvent? fork2, GameEvent? accel1, GameEvent? accel2,
        GameEvent? water1, GameEvent? water2)
    {
        var myFork = run.DurationBelow(BombSetSeconds, fork1, fork2);
        var myAccel = run.DurationBelow(BombSetSeconds, accel1, accel2);
        var myWater = run.DurationBelow(BombSetSeconds, water1, water2);

        var stack = first ? firstSetStack : secondSetStack;
        var spread = first ? firstSetSpread : secondSetSpread;
        var nothing = first ? firstSetNothing : secondSetNothing;
        var accelStack = first ? firstSetAccelStack : secondSetAccelStack;
        var accelSpread = first ? firstSetAccelSpread : secondSetAccelSpread;
        var accelNothing = first ? firstSetAccelNothing : secondSetAccelNothing;

        if (myAccel is not null) run.SetParam("stillness", !IsFake(fake, myAccel));

        GameEvent? Timed(GameEvent? status) =>
            !first || status is null
                ? status
                : status with { Kind = EventKind.CastStart, At = run.Now, Duration = FirstSetAfterBeamSeconds };

        if (myFork is not null)
        {
            var spreading = !IsFake(fake, myFork);
            run.SetParam(MarkerParam, Waymark(world, spreading));
            if (myAccel is not null) run.Call(spreading ? accelSpread : accelStack, Timed(myFork));
            else run.Call(spreading ? spread : stack, Timed(myFork));
            return myWater;
        }

        if (myWater is not null)
        {
            var spreading = IsFake(fake, myWater);
            run.SetParam(MarkerParam, Waymark(world, spreading));
            if (myAccel is not null) run.Call(spreading ? accelSpread : accelStack, Timed(myWater));
            else run.Call(spreading ? spread : stack, Timed(myWater));
            return myWater;
        }

        var stacks = world.ActiveStatuses()
            .Where(s => s.Id == (IsFake(fake, s) ? FORK : WATER))
            .Where(s => run.Remaining(s) > 0 && run.Remaining(s) < BombSetSeconds)
            .ToList();

        run.SetParam("stacks", stacks.Select(s => s.Target).OfType<Actor>().ToList());
        run.SetParam(MarkerParam, Waymark(world, spread: false));

        var stackBuff = stacks.FirstOrDefault();
        On(run, myAccel is not null ? accelNothing : nothing, Timed(stackBuff));
        return stackBuff;
    }

    public Sequence BuildChaos(IWorld world) =>
        Sequence.Repeat(Group + "Chaos", 180, e => e.Is(EventKind.CastStart, KefkaSaysCast),
            (start, run) => Chaos(start, run, world));

    private static bool IsChaosDebuff(GameEvent e) => e.Id is ChaosDynamic or ChaosEntropy;

    public static (GameEvent Early, GameEvent Late) EndsFirst(GameEvent a, GameEvent b) =>
        b.At + b.Duration < a.At + a.Duration ? (b, a) : (a, b);

    private Callout DynEnt(bool isDynamic, bool isLong, bool isReal) =>
        isDynamic
            ? isLong ? isReal ? secondRealDynamicFluid : secondFakeDynamicFluid : isReal ? realDynamicFluid : fakeDynamicFluid
            : isLong ? isReal ? secondRealEntropy : secondFakeEntropy : isReal ? realEntropy : fakeEntropy;

    private async Task<bool> ChaosReal(SequenceRun run)
    {
        var vfx = await run.WaitEvent(
            e => e.Kind == EventKind.StatusLoopVfx && e.Target?.BaseId == NpcChaos);
        return vfx.Id == RealChaos;
    }

    private async Task Chaos(GameEvent start, SequenceRun run, IWorld world)
    {
        var fake = new HashSet<(uint Status, uint Target)>();

        var real1 = await ChaosReal(run);

        await run.FindOrWaitForStatusWhere(world, IsChaosDebuff);
        await run.WaitMs(ChaosFirstDelayMs);

        var set1 = world.ActiveStatuses().Where(IsChaosDebuff).ToList();
        MarkFake(fake, set1, real1);

        var first = set1.FirstOrDefault();
        if (first is null) return;

        var ticket = run.Call(
            DynEnt(first.Id == ChaosDynamic, first.Duration > FirstLongSeconds, real1), first);

        var real2 = await ChaosReal(run);
        ticket.ForceExpire();
        await run.WaitMs(ChaosVfxGapMs);

        await run.FindOrWaitForStatusWhere(world,
            e => IsChaosDebuff(e) && run.Since(e) < RecentSeconds);
        await run.WaitMs(ChaosSecondDelayMs);

        var set2 = world.ActiveStatuses()
            .Where(s => IsChaosDebuff(s) && run.Since(s) < RecentSeconds).ToList();
        MarkFake(fake, set2, real2);

        var second = set2.FirstOrDefault();
        if (second is null) return;

        run.Call(DynEnt(second.Id == ChaosDynamic, second.Duration > SecondLongSeconds, real2), second);

        await run.WaitEvent(EventKind.StatusGain, ThunderCharged);
        await run.WaitMs(ThunderChargedDelayMs);

        var (early, late) = EndsFirst(first, second);

        var isDonut = Donut(fake, early);
        run.Call(firstEntropyDynamic, early);

        var cast = await run.WaitEvent(EventKind.CastStart, DonutCasts);
        run.SetParam(NextSpotParam, SecondSpotSuffix() ?? "");
        run.Call(isDonut ? firstEntropyDynamicStay : firstEntropyDynamicMove, cast);

        var detail = await run.WaitEvent(EventKind.Synthetic, Synthetic.ManaChargeDetail);
        await run.WaitEvent(EventKind.CastStart, ManaReleaseCast);

        var lateDonut = Donut(fake, late);
        var iceFake = detail.Arg2 == 1;
        var thunderFake = detail.Arg1 == 1;

        var (onDebuff, onCast) = (iceFake, thunderFake) switch
        {
            (true, true) => (secondEntropyDynamicBothFake,
                lateDonut ? secondEntropyDynamicStayBothFake : secondEntropyDynamicMoveBothFake),
            (true, false) => (secondEntropyDynamicFakeIce,
                lateDonut ? secondEntropyDynamicStayFakeIce : secondEntropyDynamicMoveFakeIce),
            (false, true) => (secondEntropyDynamicFakeThunder,
                lateDonut ? secondEntropyDynamicStayFakeThunder : secondEntropyDynamicMoveFakeThunder),
            _ => (secondEntropyDynamicBothReal,
                lateDonut ? secondEntropyDynamicStayBothReal : secondEntropyDynamicMoveBothReal),
        };

        run.Call(onDebuff, late);

        var lateCast = await run.WaitEvent(EventKind.CastStart, DonutCasts);
        run.Call(onCast, lateCast);

        bool Donut(HashSet<(uint Status, uint Target)> marked, GameEvent debuff)
        {
            var isFake = IsFake(marked, debuff);
            var donut = (debuff.Id == ChaosDynamic) ^ isFake;
            run.SetParam("fake", isFake);
            run.SetParam("isDonut", donut);
            return donut;
        }
    }
}
