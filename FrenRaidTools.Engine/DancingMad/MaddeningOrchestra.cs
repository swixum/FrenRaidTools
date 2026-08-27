namespace FrenRaidTools.Engine.DancingMad;

public sealed class MaddeningOrchestra
{
    public const string Group = "maddeningOrchestra";

    public const int PhaseNumber = 5;

    public const string MechanicName = "Maddening Orchestra";

    public const uint OrchestraCast = 0xBB50;
    public const uint HolyHit = 0xBB54;
    public const uint FlareDebuff = 0x14E6;
    public const uint HolyDebuff = 0x14E7;

    public readonly Callout maddeningOrchestra =
        Callout.Duration("Maddening Orchestra", "Spread to spots");

    public readonly Callout maddeningOrchestraFlare =
        Callout.Duration("Maddening Orchestra: Tank Flare", "Surprise Flare").AutoIcon();

    public readonly Callout maddeningOrchestraHoly =
        Callout.Duration("Maddening Orchestra: Tank Holy", "Surprise Holy").AutoIcon();

    public readonly Callout maddeningHoly =
        Callout.Of("Maddening Orchestra: Hit by Holy", "Out");

    public readonly Callout maddeningNoHoly =
        Callout.Of("Maddening Orchestra: Not Hit by Holy", "In");

    public readonly Callout maddeningFinalFlare =
        Callout.Duration("Maddening Orchestra: Flare Tank Move Out", "Move Out").AutoIcon();

    public readonly Callout maddeningFinal =
        Callout.Duration("Maddening Orchestra: Avoid Flare Tank", "Away from {flareTank}");

    public Sequence Build(IWorld world) =>
        Sequence.Repeat(Group, 180, e => e.Is(EventKind.CastStart, OrchestraCast),
            (start, run) => Run(start, run, world));

    public static bool IsHolyHit(GameEvent e) =>
        e.Is(EventKind.AbilityHit, HolyHit) && e.FirstTarget;

    private async Task Run(GameEvent start, SequenceRun run, IWorld world)
    {
        run.Call(maddeningOrchestra, start);

        var holyHits = await run.WaitEventsQuickSuccession(3, IsHolyHit);

        var flare = await run.FindOrWaitForStatusWhere(world, e => e.Id == FlareDebuff);
        var holy = await run.FindOrWaitForStatusWhere(world, e => e.Id == HolyDebuff);
        if (flare is null || holy is null) return;

        run.SetParam("flareTank", flare.Target);
        run.SetParam("holyTank", holy.Target);

        if (flare.Target?.IsYou == true) run.Call(maddeningOrchestraFlare, flare);
        else if (holy.Target?.IsYou == true) run.Call(maddeningOrchestraHoly, holy);
        else if (holyHits.Any(h => h.Target?.IsYou == true)) run.Call(maddeningHoly);
        else if (world.You is not null) run.Call(maddeningNoHoly);

        await run.WaitEventsQuickSuccession(3, IsHolyHit);

        if (flare.Target?.IsYou == true) run.Call(maddeningFinalFlare, flare);
        else run.Call(maddeningFinal, flare);
    }
}
