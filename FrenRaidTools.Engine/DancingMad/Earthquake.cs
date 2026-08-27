namespace FrenRaidTools.Engine.DancingMad;

public sealed class Earthquake
{
    public const string Group = "earthquake";

    public const int PhaseNumber = 3;

    public const string MechanicName = "Earthquake";

    public const uint EarthquakeCastA = 0xC571;
    public const uint EarthquakeCastB = 0xC572;

    public const uint SlapHappyStackCast = 0xBAE6;
    public const uint SlapHappyRolesCast = 0xBAE7;
    public const uint DamningEdictCast = 0xBB01;
    public const uint DespairCast = 0xBAEE;
    public const uint LongitudinalCast = 0xBAFD;
    public const uint LatitudinalCast = 0xBAFE;
    public const uint LatLongHit = 0xBAFF;

    public const double SecondSlapDelayMs = 1_500;

    public const uint Crust = 0x154E;
    public const uint Accretion = 0x644;
    public const uint Nothingness = 0xBAFC;
    public const uint LatLongMoveHit = 0xBAFF;
    public const double IntendedCleanseSeconds = 2.0;

    public CleanseCalls CleanseCall { get; set; } = CleanseCalls.PriorSet;

    public const uint BlackHoleNpc = 19512;
    public const uint BlackHoleHit = 0xBAFB;
    public const uint WhiteHoleHit = 0xBD66;
    public const int TetherSpots = 3;

    public static readonly ArenaPos TetherAp = new(100, 100, 16.9, 16.9);

    public const uint ACCRETION = 0x644;
    public const uint LINE_1 = 0xBBC;
    public const uint LINE_2 = 0xBBD;
    public const uint LINE_3 = 0xBBE;

    public readonly Callout longitudinalCast =
        Callout.Duration("Longitudinal Implosion: Cast", "Sides Then Front/Back");

    public readonly Callout latitudinalCast =
        Callout.Duration("Latitudinal Implosion: Cast", "Front/Back then Sides");

    public readonly Callout longitudinalMove =
        Callout.Of("Longitudinal Implosion: Move", "Front/Back").Quiet()
            .Note("Off by default at swix's ask. It can only fire on the first shockwave, which leaves under two seconds to the second one and often lands in the same instant as another call. The cast call above already says both halves in order.");

    public readonly Callout latitudinalMove =
        Callout.Of("Latitudinal Implosion: Move", "Sides");

    public const uint VacuumWaveCast = 0xBB13;
    public const uint ThunderIIICast = 0xBB12;
    public const uint ThunderIIIProximityCast = 0xBB09;
    public const uint ThunderIIIProximityHit = 0xBB0C;

    public readonly Callout vacuumWave =
        Callout.Duration("Vacuum Wave", "Knockback from {event.source}").Quiet()
            .Note("Off by default: the Bowels knockback line carries the position and facing in one call.");

    public readonly Callout thunderIII =
        Callout.Duration("Thunder III (Exdeath AoE)", "Away from {event.source}");

    public readonly Callout thunderIIItb =
        Callout.Duration("Thunder III (Exdeath Proximity)", "Proximity Buster");

    public readonly Callout thunderSwapAway =
        Callout.Of("Thunder III: You Took the Buster", "Away from {event.source}, Swap")
            .Note("Tanks only. Fires on the first proximity hit of each set: whoever ate it moves out so the other tank is nearest for the second hit.");

    public readonly Callout thunderSwapNear =
        Callout.Of("Thunder III: Buster Swap", "Be Near {event.source}, Swap");

    public readonly Callout earthquake =
        Callout.Duration("Earthquake Initial", "Earthquake - 1 HP");

    public readonly Callout earthquake1supp =
        Callout.Duration("Earthquake: First in Line (Support, No Accretion)", "First").Icon(LINE_1).Note("The variable {accretions} will contain the players who have accretion. Consider adding that to your callout if you are a healer or have a single-target healing buff.");

    public readonly Callout earthquake2supp =
        Callout.Duration("Earthquake: Second in Line (Support, No Accretion)", "Second").Icon(LINE_2);

    public readonly Callout earthquake3supp =
        Callout.Duration("Earthquake: Third in Line (Support, No Accretion)", "Third").Icon(LINE_3);

    public readonly Callout earthquake1dps =
        Callout.Duration("Earthquake: First in Line (DPS, No Accretion)", "First").Icon(LINE_1);

    public readonly Callout earthquake2dps =
        Callout.Duration("Earthquake: Second in Line (DPS, No Accretion)", "Second").Icon(LINE_2);

    public readonly Callout earthquake3dps =
        Callout.Duration("Earthquake: Third in Line (DPS, No Accretion)", "Third").Icon(LINE_3);

    public readonly Callout earthquake1acc =
        Callout.Duration("Earthquake: First in Line (Accretion)", "First + Accretion").Icon(LINE_1, ACCRETION);

    public readonly Callout earthquake2acc =
        Callout.Duration("Earthquake: Second in Line (Accretion)", "Second + Accretion").Icon(LINE_2, ACCRETION);

    public readonly Callout earthquakeInvalid =
        Callout.Of("Earthquake: Invalid", "Error");

    public readonly Callout slapHappyRoles =
        Callout.DurationPlus("Slap Happy: Roles", "Roles {safe}", 3.7);

    public readonly Callout slapHappyStack =
        Callout.DurationPlus("Slap Happy: Stack", "Stack {safe}", 3.7);

    public readonly Callout damningEdict =
        Callout.Duration("Damning Edict", "{safe} Behind Chaos").Quiet()
            .Note("Off by default at swix's ask. This was the call that read Chaos's facing; the slap call 1.5s later carries a fixed direction plus the reminder to watch his frontal yourself.");

    public readonly Callout earthquakeBodySlamDamningEdict =
        Callout.Duration("Earthquake: Body Slam + Damning Edict", "Out of middle, Watch Chaos Frontal");

    public readonly Callout earthquakeSlapHappyRolesLat =
        Callout.Duration("Earthquake: Slap Happy + Lat: Front/Back First then Roles", "{firstSafe} to {secondSafe}", "{firstSafe} to {secondSafe}, Roles {finalSafe}").Note("Please note that Chaos is not necessarily going to perfectly face a cardinal or intercard for this, so these directions are best-effort. You may still need to use eyes. `{firstSafe}` is one or more directions that is/are safe for the initial hit of lat/long and are adjacent to the final safe spot. `{secondSafe}` is one or more directions that is/are safe for the second hit of lat/long and are adjacent to the final safe spot. `{finalSafe}` is the slap happy safe direction. Note that it is technically possible to use the {secondSafe} spot for a party stack, but this is not a standard strategy.");

    public readonly Callout earthquakeSlapHappyRolesLong =
        Callout.Duration("Earthquake: Slap Happy + Long: Sides First then Roles", "{firstSafe} to {secondSafe}", "{firstSafe} to {secondSafe}, Roles {finalSafe}");

    public readonly Callout earthquakeSlapHappyStackLat =
        Callout.Duration("Earthquake: Slap Happy + Lat: Front/Back First then Stack", "{firstSafe} to {secondSafe}", "{firstSafe} to {secondSafe}, Stack {finalSafe}");

    public readonly Callout earthquakeSlapHappyStackLong =
        Callout.Duration("Earthquake: Slap Happy + Long: Sides First then Stack", "{firstSafe} to {secondSafe}", "{firstSafe} to {secondSafe}, Stack {finalSafe}");

    public readonly Callout earthquakeSlapHappyFinalRolesLat =
        Callout.DurationPlus("Earthquake: Slap Happy + Lat: Sides + Stack", "Roles {finalSafe}", 3.7);

    public readonly Callout earthquakeSlapHappyFinalRolesLong =
        Callout.DurationPlus("Earthquake: Slap Happy + Long: Front/Back + Stack", "Roles {finalSafe}", 3.7);

    public readonly Callout earthquakeSlapHappyFinalStackLat =
        Callout.DurationPlus("Earthquake: Slap Happy + Lat: Sides + Stack", "Stack {finalSafe}", 3.7);

    public readonly Callout earthquakeSlapHappyFinalStackLong =
        Callout.DurationPlus("Earthquake: Slap Happy + Long: Front/Back + Stack", "Stack {finalSafe}", 3.7);

    public readonly Callout earthquakeDespairOnly =
        Callout.Duration("Earthquake: Despair Only", "Out of middle");

    public readonly Callout earthquakePersistentTracker =
        Callout.Of("Earthquake: Persistent Text", "", "{onesRemaining} #1, {twosRemaining} #2, {threesRemaining} #3").Quiet().Note("This is a text-only callout that provides a persistent view of how many debuffs are still present in each role. You can use {onesRemaining}, {twosRemaining}, {threesRemaining} or {totalRemaining}.");

    public readonly Callout earthquakeCleansed =
        Callout.Of("Earthquake: Debuff Cleansed", "{event.target} Cleansed").Note("You can control when this callout fires on the settings tab above. By default, it works on a same-role, prior-set basis - i.e. #1 accretion cleansing will trigger this if you are #2 accretion. This does NOT call your own debuff being removed - use the self cleanse call below for that.");

    public readonly Callout earthquakeSelfCleanse =
        Callout.Of("Earthquake: Self Cleansed", "Cleansed");

    public readonly Callout earthquakeTetherSet1 =
        Callout.Of("Earthquake: Tether Set 1 (One then Two)", "{firstTethers} then {secondTethers}").Note("For tether sets that are 1 + 2 or 2 + 1 staggered spawns, {firstTethers} and {secondTethers} tell you the first vs second locations. You can use {allTethers} for all locations.");

    public readonly Callout earthquakeTetherSet2 =
        Callout.Of("Earthquake: Tether Set 2 (Three Tethers)", "{allTethers}");

    public readonly Callout earthquakeTetherSet3 =
        Callout.Of("Earthquake: Tether Set 3 (Three Tethers)", "{allTethers}");

    public readonly Callout earthquakeTetherSet4 =
        Callout.Of("Earthquake: Tether Set 4 (Two then One)", "{firstTethers} then {secondTethers}");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 240, e => e.Is(EventKind.CastStart, EarthquakeCastA, EarthquakeCastB),
            (start, run) => Run(start, run, world));

    private static ArenaSector Facing(GameEvent? cast) =>
        cast?.Source is null ? ArenaSector.Unknown : ArenaPos.Facing(cast.Source.Heading);

    public const double LandingLeadSeconds = 1.0;

    private static ArenaSector FacingNow(IWorld world, GameEvent cast) =>
        cast.Source is null
            ? ArenaSector.Unknown
            : ArenaPos.Facing((world.Latest(cast.Source) ?? cast.Source).Heading);

    private static async Task<ArenaSector> FacingNearLanding(
        SequenceRun run, IWorld world, GameEvent cast)
    {
        var left = cast.Duration - run.Since(cast) - LandingLeadSeconds;
        if (left > 0) await run.WaitSeconds(left);
        return FacingNow(world, cast);
    }

    public static List<ArenaSector> EdictDespairSafe(ArenaSector edict, ArenaSector despair)
    {
        if (!edict.IsPoint() || !despair.IsPoint()) return [];

        if (edict == despair || edict.Opposite() == despair)
            return [despair.PlusEighths(3), despair.PlusEighths(-3)];

        if (edict.PlusQuads(1) == despair || edict.PlusQuads(-1) == despair)
            return [edict.Opposite()];

        var safe = new List<ArenaSector> { despair.PlusQuads(1), despair.PlusQuads(-1) };
        safe.Remove(edict);
        safe.Remove(edict.PlusEighths(1));
        safe.Remove(edict.PlusEighths(-1));
        return safe;
    }

    public const string FirstSlapSpot = "Southwest";
    public const string SecondSlapSpot = "West, Watch Chaos Frontal";
    public const string StackSlapSpot = "East";
    public const string SecondStackSpot = "East, Watch Chaos Frontal";

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(earthquake, start);

        await SlapHappy(run, world, FirstSlapSpot, StackSlapSpot);

        var edict = await run.FindOrWaitForCast(world, e => e.Id == DamningEdictCast);
        var chaosFacing = Facing(edict);
        run.SetParam("chaosFacing", chaosFacing.Told());
        run.SetParam("safe", chaosFacing.Opposite().Told());
        if (edict is not null) run.Call(damningEdict, edict);

        await run.WaitMs(SecondSlapDelayMs);
        await SlapHappy(run, world, SecondSlapSpot, SecondStackSpot, untilItLands: true);

        var despair = await run.FindOrWaitForCast(world, e => e.Id == DespairCast);
        if (despair is not null) run.Call(earthquakeBodySlamDamningEdict, despair);

        await SlapHappyWithLatLong(run, world);

        var finalDespair = await run.FindOrWaitForCast(world, e => e.Id == DespairCast);
        if (finalDespair is not null) run.Call(earthquakeDespairOnly, finalDespair);
    }

    private async Task SlapHappy(
        SequenceRun run, IWorld world, string rolesSpot, string stackSpot,
        bool untilItLands = false)
    {
        var slap = await run.FindOrWaitForCast(
            world, e => e.Id is SlapHappyStackCast or SlapHappyRolesCast);
        if (slap is null) return;

        var roles = slap.Id == SlapHappyRolesCast;

        run.SetParam("safe", roles ? rolesSpot : stackSpot);
        run.Call(roles ? slapHappyRoles : slapHappyStack, slap);

        if (untilItLands) await run.WaitCastFinished(slap);
    }

    private async Task SlapHappyWithLatLong(SequenceRun run, IWorld world)
    {
        var slap = await run.FindOrWaitForCast(
            world, e => e.Id is SlapHappyStackCast or SlapHappyRolesCast);
        var latLong = await run.FindOrWaitForCast(
            world, e => e.Id is LongitudinalCast or LatitudinalCast);
        if (slap is null || latLong is null) return;

        var longi = latLong.Id == LongitudinalCast;
        var roles = slap.Id == SlapHappyRolesCast;

        var bossFacing = Facing(slap);
        var latLongFacing = Facing(latLong);
        AimLatLong(run, bossFacing, latLongFacing, roles, longi);

        var call = longi
            ? roles ? earthquakeSlapHappyRolesLong : earthquakeSlapHappyStackLong
            : roles ? earthquakeSlapHappyRolesLat : earthquakeSlapHappyStackLat;
        var next = longi
            ? roles ? earthquakeSlapHappyFinalRolesLong : earthquakeSlapHappyFinalStackLong
            : roles ? earthquakeSlapHappyFinalRolesLat : earthquakeSlapHappyFinalStackLat;

        run.Call(call, latLong);

        var latLanding = await FacingNearLanding(run, world, latLong);
        var slapNow = FacingNow(world, slap);
        if (slapNow != bossFacing || latLanding != latLongFacing)
        {
            AimLatLong(run, slapNow, latLanding, roles, longi);
            run.Call(call, latLong);
        }

        await run.WaitEvent(EventKind.AbilityHit, LatLongHit);
        run.Call(next, slap);
    }

    private static void AimLatLong(
        SequenceRun run, ArenaSector bossFacing, ArenaSector latLongFacing, bool roles, bool longi)
    {
        run.SetParam("bossFacing", bossFacing.Told());
        run.SetParam("latLongFacing", latLongFacing.Told());

        var cleavingTowards = bossFacing.PlusQuads(roles ? -1 : 1);
        var cleaving = new[]
        {
            cleavingTowards.PlusEighths(-1), cleavingTowards, cleavingTowards.PlusEighths(1),
        };

        var sidesSafe = new List<ArenaSector> { latLongFacing.PlusQuads(-1), latLongFacing.PlusQuads(1) }
            .Where(x => !cleaving.Contains(x)).ToList();
        var frontBackSafe = new List<ArenaSector> { latLongFacing, latLongFacing.Opposite() }
            .Where(x => !cleaving.Contains(x)).ToList();

        run.SetParam("firstSafe", (longi ? sidesSafe : frontBackSafe).Told());
        run.SetParam("secondSafe", (longi ? frontBackSafe : sidesSafe).Told());
        run.SetParam("finalSafe", roles ? FirstSlapSpot : StackSlapSpot);
    }

    public Sequence BuildLatLong(IWorld world) =>
        Sequence.Repeat(Group + "LatLong", 30,
            e => e.Is(EventKind.CastStart, LongitudinalCast, LatitudinalCast),
            async (start, run) =>
            {
                var longi = start.Id == LongitudinalCast;
                run.Call(longi ? longitudinalCast : latitudinalCast, start);
                await run.WaitEvent(EventKind.AbilityHit, LatLongMoveHit);
                run.Call(longi ? longitudinalMove : latitudinalMove);
            });

    public Sequence BuildCleanses(IWorld world) =>
        Sequence.Repeat(Group + "Cleanses", 180,
            e => e.Kind == EventKind.StatusGain && e.Id == Crust && e.Target?.IsYou == true,
            (start, run) => Cleanses(start, run, world));

    public static AccretionRole? RoleFor(double crustSeconds, bool accretion, bool support) =>
        crustSeconds < 90
            ? accretion ? AccretionRole.FirstAccretion
              : support ? AccretionRole.FirstSupport : AccretionRole.FirstDps
            : crustSeconds < 120
                ? accretion ? AccretionRole.SecondAccretion
                  : support ? AccretionRole.SecondSupport : AccretionRole.SecondDps
                : accretion ? null
                  : support ? AccretionRole.ThirdSupport : AccretionRole.ThirdDps;

    private async Task Cleanses(GameEvent start, SequenceRun run, IWorld world)
    {
        var gains = await run.WaitEventsQuickSuccession(
            MaxOpeningDebuffs,
            e => e.Kind == EventKind.StatusGain && e.Id is Crust or Accretion);
        if (!gains.Contains(start)) gains.Insert(0, start);

        var crusts = gains.Where(e => e.Id == Crust).ToList();

        var carriers = new Dictionary<uint, Actor>();
        foreach (var gain in gains)
        {
            if (gain.Id != Accretion || gain.Target is not { } target) continue;
            carriers[target.ObjectId] = target;
        }

        await run.WaitMs(100);

        foreach (var status in world.ActiveStatuses())
        {
            if (status.Id != Accretion || status.Target is not { } target) continue;
            carriers[target.ObjectId] = target;
        }

        var accretionOn = carriers.Keys.ToHashSet();

        run.SetParam("accretions", carriers.Values.ToList());

        var roles = new Dictionary<uint, AccretionRole>();
        AccretionRole? myRole = null;

        foreach (var crust in crusts)
        {
            var target = crust.Target;
            if (target is null) continue;

            var role = RoleFor(crust.Duration, accretionOn.Contains(target.ObjectId), target.Support);
            if (role is null) continue;

            roles[target.ObjectId] = role.Value;
            _lineNames[PlanTether.Name(PlaceOf(role.Value), GroupOf(role.Value))] = target.Called;
            if (target.IsYou) myRole = role;
        }

        Assignment = myRole;

        if (myRole is null)
        {
            run.Call(earthquakeInvalid);
            return;
        }

        run.Call(myRole switch
        {
            AccretionRole.FirstDps => earthquake1dps,
            AccretionRole.FirstSupport => earthquake1supp,
            AccretionRole.FirstAccretion => earthquake1acc,
            AccretionRole.SecondDps => earthquake2dps,
            AccretionRole.SecondSupport => earthquake2supp,
            AccretionRole.SecondAccretion => earthquake2acc,
            AccretionRole.ThirdDps => earthquake3dps,
            _ => earthquake3supp,
        }, start);

        var left = new HashSet<uint>(roles.Keys);
        Track(run, roles, left);
        run.Call(earthquakePersistentTracker);

        var remaining = roles.Count;
        var lastNothingness = new Dictionary<uint, double>();

        while (remaining > 0)
        {
            var e = await run.WaitEvent(x =>
                (x.Kind == EventKind.StatusLose && x.Id == Crust) ||
                (x.Is(EventKind.AbilityHit, Nothingness) && x.FirstTarget));

            if (e.Kind == EventKind.AbilityHit)
            {
                if (e.Target is not null) lastNothingness[e.Target.ObjectId] = e.At;
                continue;
            }

            var target = e.Target;
            if (target is null) continue;
            if (!roles.TryGetValue(target.ObjectId, out var targetRole)) continue;

            remaining--;
            left.Remove(target.ObjectId);
            Track(run, roles, left);
            if (remaining > 0) run.Call(earthquakePersistentTracker);

            if (target.IsYou)
            {
                run.Call(earthquakeSelfCleanse);
                continue;
            }

            var hitAt = lastNothingness.GetValueOrDefault(target.ObjectId, double.NegativeInfinity);
            var intended = e.At - hitAt < IntendedCleanseSeconds;

            var speak = CleanseCall switch
            {
                CleanseCalls.All => intended,
                CleanseCalls.Matched => Previous(myRole.Value) == targetRole,
                _ => intended && SetOf(targetRole) == SetOf(myRole.Value) - 1,
            };

            if (speak) run.Call(earthquakeCleansed, e);
        }
    }

    public static void Track(
        SequenceRun run, IReadOnlyDictionary<uint, AccretionRole> roles, IReadOnlySet<uint> left)
    {
        var counts = new int[PlanTether.Places.Length];

        foreach (var (who, role) in roles)
            if (left.Contains(who)) counts[SetOf(role) - 1]++;

        run.SetParam("onesRemaining", counts[0]);
        run.SetParam("twosRemaining", counts[1]);
        run.SetParam("threesRemaining", counts[2]);
        run.SetParam("totalRemaining", counts[0] + counts[1] + counts[2]);
    }

    public static int SetOf(AccretionRole role) => role switch
    {
        AccretionRole.FirstDps or AccretionRole.FirstSupport or AccretionRole.FirstAccretion => 1,
        AccretionRole.SecondDps or AccretionRole.SecondSupport or AccretionRole.SecondAccretion => 2,
        _ => 3,
    };

    public const int MaxOpeningDebuffs = 16;

    public const double AnchorRadius = 5.0;

    public static readonly ArenaPos Bearing = new(100, 100, 0, 0);

    public static string PlaceOf(AccretionRole role) =>
        PlanTether.Places[Math.Clamp(SetOf(role), 1, PlanTether.Places.Length) - 1];

    public static string GroupOf(AccretionRole role) => role switch
    {
        AccretionRole.FirstAccretion or AccretionRole.SecondAccretion => PlanTether.Accretion,
        AccretionRole.FirstSupport or AccretionRole.SecondSupport
            or AccretionRole.ThirdSupport => PlanTether.Support,
        _ => PlanTether.Dps,
    };

    public AccretionRole? Assignment { get; private set; }

    private readonly Dictionary<string, string> _lineNames = new(StringComparer.Ordinal);

    public void ForgetAssignment()
    {
        Assignment = null;
        _lineNames.Clear();
    }

    public static AccretionRole? Remembered(AccretionRole? latched, IWorld world) =>
        latched ?? Mine(world);

    public static AccretionRole? Mine(IWorld world)
    {
        var you = world.You;
        if (you is null) return null;

        var statuses = world.ActiveStatuses();

        var accretion = false;
        foreach (var status in statuses)
            if (status.Id == Accretion && status.Target?.ObjectId == you.ObjectId) accretion = true;

        foreach (var status in statuses)
        {
            if (status.Id != Crust) continue;
            if (status.Target?.ObjectId != you.ObjectId) continue;
            return RoleFor(status.Duration, accretion, you.Support);
        }

        return null;
    }

    public static ArenaSector Anchor(IWorld world)
    {
        foreach (var npc in world.NpcsById(GravenImage.Kefka))
        {
            var pos = npc.Pos;
            if (!pos.Known) continue;

            var dx = pos.X - Bearing.CenterX;
            var dy = pos.Y - Bearing.CenterY;
            if (Math.Sqrt(dx * dx + dy * dy) < AnchorRadius) continue;

            var sector = Bearing.For(pos);
            if (sector.IsPoint()) return sector;
        }

        return ArenaSector.Unknown;
    }

    public static List<ArenaSector> ClockwiseFrom(
        ArenaSector anchor, IEnumerable<ArenaSector> holes)
    {
        var points = holes.Where(h => h.IsPoint()).Distinct().ToList();

        return anchor.IsPoint()
            ? [.. points.OrderBy(hole => anchor.EighthsTo(hole))]
            : [.. points.Order()];
    }

    public const int PairSize = 2;

    public static IReadOnlyList<ArenaSector>? PairOf(StaggeredTethers? waves) =>
        waves is null ? null
        : waves.First.Count == PairSize ? waves.First
        : waves.Second.Count == PairSize ? waves.Second
        : null;

    private void Duty(
        SequenceRun run, IWorld world, int set,
        IEnumerable<ArenaSector> holes, StaggeredTethers? waves = null,
        AccretionRole? latched = null)
    {
        var mine = Remembered(latched, world);
        var anchor = Anchor(world);
        var pair = PairOf(waves);

        run.SetParam(PlanCalls.PlaceParam, mine is null ? null : PlaceOf(mine.Value));
        run.SetParam(PlanCalls.GroupParam, mine is null ? null : GroupOf(mine.Value));
        run.SetParam(PlanAnchors.TetherStep, set);
        run.SetParam(PlanCalls.SpotsParam, Names(ClockwiseFrom(anchor, holes)));
        run.SetParam(PlanCalls.PairParam,
            pair is null ? null : Names(ClockwiseFrom(anchor, pair)));
        run.SetParam(PlanCalls.HoldersParam,
            _lineNames.Count == 0 ? null : new Dictionary<string, string>(_lineNames, StringComparer.Ordinal));
    }

    public Sequence BuildCasts(IWorld world) =>
        CastCalls.For(Group + "Casts",
            (VacuumWaveCast, vacuumWave),
            (ThunderIIICast, thunderIII),
            (ThunderIIIProximityCast, thunderIIItb));

    public Sequence BuildTankSwap(IWorld world) =>
        Sequence.Repeat(Group + "TankSwap", 30,
            e => e.Is(EventKind.CastStart, ThunderIIIProximityCast),
            async (start, run) =>
            {
                var hit = await run.WaitEvent(e =>
                    e.Is(EventKind.AbilityHit, ThunderIIIProximityHit) && e.FirstTarget);
                if (!JobKinds.Tanking(world)) return;
                run.Call(hit.Target?.IsYou == true ? thunderSwapAway : thunderSwapNear, hit);
            });

    public Sequence BuildTethers(IWorld world) =>
        Sequence.Repeat(Group + "Tethers", 180,
            e => e.Is(EventKind.AbilityHit, BlackHoleHit),
            (start, run) => Tethers(start, run, world));

    private async Task Tethers(GameEvent start, SequenceRun run, IWorld world)
    {
        var mine = Assignment ?? Mine(world);

        var set1 = await Staggered(run, world, 1);
        mine ??= Assignment ?? Mine(world);
        run.SetParam("firstTethers", Names(set1.First));
        run.SetParam("secondTethers", Names(set1.Second));
        run.SetParam("allTethers", Names(set1.Second));
        Duty(run, world, 1, set1.Combined, set1, mine);
        run.Call(earthquakeTetherSet1);

        await run.WaitEvent(EventKind.AbilityHit, DamningEdictCast);

        var set2 = await Simple(run, world);
        run.ClearParams();
        run.SetParam("allTethers", Names(set2));
        Duty(run, world, 2, set2, latched: mine);
        run.Call(earthquakeTetherSet2);

        await run.WaitEvent(EventKind.AbilityHit, DamningEdictCast);

        var set3 = await Simple(run, world);
        run.ClearParams();
        run.SetParam("allTethers", Names(set3));
        Duty(run, world, 3, set3, latched: mine);
        run.Call(earthquakeTetherSet3);

        await run.WaitEvent(EventKind.AbilityHit, WhiteHoleHit);

        var set4 = await Staggered(run, world, 2);
        run.SetParam("firstTethers", Names(set4.First));
        run.SetParam("secondTethers", Names(set4.Second));
        run.SetParam("allTethers", Names(set4.Second));
        Duty(run, world, 4, set4.Combined, set4, mine);
        run.Call(earthquakeTetherSet4);
    }

    private static List<string> Names(IEnumerable<ArenaSector> spots) =>
        spots.Select(s => s.Name()).ToList();

    public static List<ArenaSector> CardinalSpots(IEnumerable<Actor> npcs) =>
        npcs.Select(n => TetherAp.For(n.Pos))
            .Where(s => s.IsCardinal())
            .Distinct()
            .Order()
            .ToList();

    private static async Task<List<ArenaSector>> Simple(SequenceRun run, IWorld world)
    {
        while (true)
        {
            await run.WaitMs(50);
            var spots = CardinalSpots(world.NpcsById(BlackHoleNpc));
            if (spots.Count >= TetherSpots) return spots;
        }
    }

    private static async Task<StaggeredTethers> Staggered(
        SequenceRun run, IWorld world, int firstExpected)
    {
        var seen = new HashSet<uint>();
        var first = new List<Actor>();

        while (first.Count < firstExpected)
        {
            var tether = await run.WaitEvent(
                e => e.Kind == EventKind.Tether && e.EitherEnd(a => a.BaseId == BlackHoleNpc));

            var hole = tether.Source?.BaseId == BlackHoleNpc ? tether.Source : tether.Target;
            if (hole is null) continue;
            if (seen.Add(hole.ObjectId)) first.Add(hole);
        }

        var remaining = TetherSpots - firstExpected;
        List<ArenaSector> second;

        while (true)
        {
            await run.WaitMs(50);
            second = CardinalSpots(world.NpcsById(BlackHoleNpc).Where(n => !seen.Contains(n.ObjectId)));
            if (second.Count >= remaining) break;
        }

        var firstSpots = CardinalSpots(first.Select(a => world.Latest(a) ?? a));
        var combined = new List<ArenaSector>(firstSpots);
        combined.AddRange(second);
        return new StaggeredTethers(firstSpots, second, combined);
    }

    public static AccretionRole? Previous(AccretionRole role) => role switch
    {
        AccretionRole.SecondDps => AccretionRole.FirstDps,
        AccretionRole.SecondSupport => AccretionRole.FirstSupport,
        AccretionRole.SecondAccretion => AccretionRole.FirstAccretion,
        AccretionRole.ThirdDps => AccretionRole.SecondDps,
        AccretionRole.ThirdSupport => AccretionRole.SecondSupport,
        _ => null,
    };
}

public sealed record StaggeredTethers(
    IReadOnlyList<ArenaSector> First,
    IReadOnlyList<ArenaSector> Second,
    IReadOnlyList<ArenaSector> Combined);

public enum AccretionRole
{
    FirstDps, FirstSupport, FirstAccretion,
    SecondDps, SecondSupport, SecondAccretion,
    ThirdDps, ThirdSupport,
}

public enum CleanseCalls
{
    All,
    Matched,
    PriorSet,
}
