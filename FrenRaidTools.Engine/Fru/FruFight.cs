using System.Runtime.CompilerServices;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Engine.Fru;

public sealed class FruFight
{
    public const string Group = "fru";

    public const uint AbsoluteZero = 0x9D20;
    public const uint AkhMorn = 0x9D76;
    public const uint AkhRhai = 0x9D2D;
    public const uint BlackHalo = 0x9D62;
    public const uint BlackHaloTank = 0x9D62;
    public const uint Brightfire = 0x9CD8;
    public const uint BurnishedGlory = 0x9CEA;
    public const uint CrystallizeTime = 0x9D30;
    public const uint DarkestDanceTank = 0x9CF5;
    public const uint DiamondDust = 0x9D05;
    public const uint Explosion = 0x9CC3;
    public const uint FallOfFaith = 0x9CC9;
    public const uint FrigidNeedle = 0x9D08;
    public const uint FulgentBlade = 0x9D72;
    public const uint HallowedRay = 0x9D12;
    public const uint HellSJudgment = 0x9D49;
    public const uint HiemalStorm = 0x9D40;
    public const uint LightRampant = 0x9D14;
    public const uint Materialization = 0x9D36;
    public const uint MemorySEnd = 0x9D6C;
    public const uint MirrorMirror = 0x9CF3;
    public const uint PandoraSBox = 0x9D86;
    public const uint ParadiseLost = 0x9D87;
    public const uint PowderMarkTrail = 0x9CE8;
    public const uint PowderMarkTrailTank = 0x9CE8;
    public const uint QuadrupleSlap = 0x9CFF;
    public const uint QuadrupleSlapXTank = 0x9D00;
    public const uint ReflectedScytheKick = 0x9D0D;
    public const uint ShellCrusher = 0x9D5E;
    public const uint ShockwavePulsar = 0x9D5A;
    public const uint SinboundBlizzardIii = 0x9D42;
    public const uint SinboundHoly = 0x9D10;
    public const uint SomberDance = 0x9D5B;
    public const uint SomberDanceTank = 0x9D5B;
    public const uint SpiritTaker = 0x9D60;
    public const uint TheHouseOfLight = 0x9CFD;
    public const uint ThePathOfDarkness = 0x9CB6;

    public static readonly Callout absoluteZero = new()
    {
        Description = "Absolute Zero",
        Mechanic = "Absolute Zero",
        Phase = 2,
        Key = "absoluteZero",
        Speech = "Raidwide, knockback out",
        Text = "Raidwide, knockback out" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout akhMorn = new()
    {
        Description = "Akh Morn",
        Mechanic = "Akh Morn",
        Phase = 6,
        Key = "akhMorn",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "P5 Akh Morn, Light/Dark Stacks\nBefore the reference wording: Left, light stack\nMT: Left, light stack\nOT: Right, dark stack\nH1: Left, light stack\nH2: Right, dark stack\nM1: Left, light stack\nM2: Right, dark stack\nR1: Left, light stack\nR2: Right, dark stack",
    };
    public static readonly Callout akhRhai = new()
    {
        Description = "Akh Rhai",
        Mechanic = "Akh Rhai",
        Phase = 5,
        Key = "akhRhai",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "P4 Darklit Dragonsong, Akh Rhai\nBefore the reference wording: Stack mid, Wings, dodge SE\nMT: Wings, dodge NE\nOT: Wings, dodge NE\nH1: Wings, dodge NW\nH2: Wings, dodge NW\nM1: Wings, dodge SE\nM2: Wings, dodge SE\nR1: Wings, dodge SW\nR2: Wings, dodge SW",
    };
    public static readonly Callout blackHalo = new()
    {
        Description = "Black Halo",
        Mechanic = "Black Halo",
        Phase = 4,
        Key = "blackHalo",
        Speech = "Tank out",
        Text = "Tank out" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout blackHaloTank = new()
    {
        Description = "Black Halo",
        Mechanic = "Black Halo",
        Phase = 4,
        Key = "blackHaloTank",
        Speech = "Buster, swap",
        Text = "Buster, swap" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Tanks only.",
    };
    public static readonly Callout brightfire = new()
    {
        Description = "Brightfire",
        Mechanic = "Brightfire",
        Phase = 1,
        Key = "brightfire",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        CountdownFromStartSeconds = 11.9,
        Notes = "Fires on Brightfire, 11.9s before it lands, in 100% of pulls.",
    };
    public static readonly Callout burnishedGlory = new()
    {
        Description = "Burnished Glory",
        Mechanic = "Burnished Glory",
        Phase = 1,
        Key = "burnishedGlory",
        Speech = "Raidwide with a bleed",
        Text = "Raidwide with bleed" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Before the reference wording: Raidwide, bleed",
    };
    public static readonly Callout crystallizeTime = new()
    {
        Description = "Crystallize Time",
        Mechanic = "Crystallize Time",
        Phase = 5,
        Key = "crystallizeTime",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout darkestDanceTank = new()
    {
        Description = "Darkest Dance",
        Mechanic = "Darkest Dance",
        Phase = 5,
        Key = "darkestDanceTank",
        Speech = "Far bait",
        Text = "Far bait" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Tanks only.",
    };
    public static readonly Callout diamondDust = new()
    {
        Description = "Diamond Dust",
        Mechanic = "Diamond Dust",
        Phase = 2,
        Key = "diamondDust",
        Speech = "Raidwide, partner swap",
        Text = "Raidwide, partner swap" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Before the reference wording: Raidwide",
    };
    public static readonly Callout explosion = new()
    {
        Description = "Explosion",
        Mechanic = "Explosion",
        Phase = 6,
        Key = "explosion",
        Speech = "Stack",
        Text = "Stack" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout fallOfFaith = new()
    {
        Description = "Fall of Faith",
        Mechanic = "Fall of Faith",
        Phase = 1,
        Key = "fallOfFaith",
        Speech = "Lineup for tethers",
        Text = "Lineup for tethers" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "This one varies, so the call names every branch.\nIf Lightning: 3 cones\nIf Fire: 1 stack cone\nP1 Fall of Faith, Fall of Faith",
    };
    public static readonly Callout frigidNeedle = new()
    {
        Description = "Frigid Needle",
        Mechanic = "Frigid Needle",
        Phase = 2,
        Key = "frigidNeedle",
        Speech = "Rotate clockwise",
        Text = "Rotate CW" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "This one varies, so the call names every branch.\nIf Rotating toward Shiva: Slide across\nP2 Diamond Dust, Cursed Pattern",
    };
    public static readonly Callout fulgentBlade = new()
    {
        Description = "Fulgent Blade",
        Mechanic = "Fulgent Blade",
        Phase = 6,
        Key = "fulgentBlade",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "This one varies, so the call names every branch.\nIf First wave east: First east: WSW\nIf First wave west: First west: ENE\nP5 Fulgent Blade, Intersection Order\nP5 Fulgent Blade, Safe Spot\nBefore the reference wording: First wave side",
    };
    public static readonly Callout hallowedRay = new()
    {
        Description = "Hallowed Ray",
        Mechanic = "Hallowed Ray",
        Phase = 2,
        Key = "hallowedRay",
        Speech = "Line stack",
        Text = "Line stack" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout hellSJudgment = new()
    {
        Description = "Hell's Judgment",
        Mechanic = "Hell's Judgment",
        Phase = 4,
        Key = "hellSJudgment",
        Speech = "Everyone to one HP",
        Text = "1 HP" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Before the reference wording: Everyone to 1 HP",
    };
    public static readonly Callout hiemalStorm = new()
    {
        Description = "Hiemal Storm",
        Mechanic = "Hiemal Storm",
        Phase = 3,
        Key = "hiemalStorm",
        Speech = "Bait, then dodge",
        Text = "Bait" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "Intermission Intermission, Bait + Tether",
    };
    public static readonly Callout lightRampant = new()
    {
        Description = "Light Rampant",
        Mechanic = "Light Rampant",
        Phase = 2,
        Key = "lightRampant",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        CountdownFromStartSeconds = 12.9,
        Notes = "Fires on Light Rampant, 12.9s before it lands, in 100% of pulls.\nThis one varies, so the call names every branch.\nIf Three supports tethered: 3 and 3, no rotate\nP2 Light Rampant, 3 Support/3 DPS Tethers\nIf Two supports tethered: 2 and 4, R1 north\nP2 Light Rampant, 2 Support/4 DPS Tethers\nIf Four supports tethered: 4 and 2, T2 south\nP2 Light Rampant, 4 Support/2 DPS Tethers\nIf Tethers north and south: Swap N and S\nIf Tethers northwest and northeast: Swap NW and NE\nP2 Light Rampant, Tether Adjusts\nBefore the reference wording: Tethers, count supports",
    };
    public static readonly Callout materialization = new()
    {
        Description = "Materialization",
        Mechanic = "Materialization",
        Phase = 5,
        Key = "materialization",
        Speech = "Stack",
        Text = "Stack" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout memorySEnd = new()
    {
        Description = "Memory's End",
        Mechanic = "Memory's End",
        Phase = 4,
        Key = "memorySEnd",
        Speech = "Raidwide, heal through",
        Text = "Raidwide, heal through" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout mirrorMirror = new()
    {
        Description = "Mirror, Mirror",
        Mechanic = "Mirror, Mirror",
        Phase = 2,
        Key = "mirrorMirror",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "This one varies, so the call names every branch.\nIf Two red mirrors equally close: CW to red\nP2 Mirror Mirror, Mirror Mirror\nMT: Nearest red mirror, protean off H1\nOT: Nearest red mirror\nH1: Nearest red mirror, protean off MT\nH2: Nearest red mirror\nM1: Nearest red mirror, protean mid\nM2: Nearest red mirror\nR1: Nearest red mirror, protean mid\nR2: Nearest red mirror",
    };
    public static readonly Callout pandoraSBox = new()
    {
        Description = "Pandora's Box",
        Mechanic = "Pandora's Box",
        Phase = 6,
        Key = "pandoraSBox",
        Speech = "Tank limit break",
        Text = "Tank LB" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Before the reference wording: Tank limit break",
    };
    public static readonly Callout paradiseLost = new()
    {
        Description = "Paradise Lost",
        Mechanic = "Paradise Lost",
        Phase = 6,
        Key = "paradiseLost",
        Speech = "Melee limit break at the R",
        Text = "Melee LB3 at the R" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "P5 Loop + Enrage, Paradise Lost (Enrage)",
    };
    public static readonly Callout powderMarkTrail = new()
    {
        Description = "Powder Mark Trail",
        Mechanic = "Powder Mark Trail",
        Phase = 1,
        Key = "powderMarkTrail",
        Speech = "Buster on {event.target}",
        Text = "Buster on {event.target}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout powderMarkTrailTank = new()
    {
        Description = "Powder Mark Trail",
        Mechanic = "Powder Mark Trail",
        Phase = 1,
        Key = "powderMarkTrailTank",
        Speech = "Buster, swap after",
        Text = "Buster, swap after" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Tanks only.",
    };
    public static readonly Callout quadrupleSlap = new()
    {
        Description = "Quadruple Slap",
        Mechanic = "Quadruple Slap",
        Phase = 2,
        Key = "quadrupleSlap",
        Speech = "Buster on {event.target}",
        Text = "Buster on {event.target}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout quadrupleSlapXTank = new()
    {
        Description = "Quadruple Slap",
        Mechanic = "Quadruple Slap",
        Phase = 2,
        Key = "quadrupleSlapXTank",
        Speech = "Second buster, swap",
        Text = "Second buster, swap" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Tanks only.",
    };
    public static readonly Callout reflectedScytheKick = new()
    {
        Description = "Reflected Scythe Kick",
        Mechanic = "Reflected Scythe Kick",
        Phase = 2,
        Key = "reflectedScytheKick",
        Speech = "Red mirrors, in and proteans",
        Text = "Red mirrors, in and proteans" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout shellCrusher = new()
    {
        Description = "Shell Crusher",
        Mechanic = "Shell Crusher",
        Phase = 4,
        Key = "shellCrusher",
        Speech = "Stack",
        Text = "Stack" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "P3 Ultimate Relativity, Shell Crusher, Pulsar, Dark Halo\nBefore the reference wording: Stack mid",
    };
    public static readonly Callout shockwavePulsar = new()
    {
        Description = "Shockwave Pulsar",
        Mechanic = "Shockwave Pulsar",
        Phase = 4,
        Key = "shockwavePulsar",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout sinboundBlizzardIii = new()
    {
        Description = "Sinbound Blizzard III",
        Mechanic = "Sinbound Blizzard III",
        Phase = 3,
        Key = "sinboundBlizzardIii",
        Speech = "Kill light crystals only",
        Text = "Light crystals only" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        RepeatAfterSeconds = FruIceAge.CastSeconds,
        Notes = "This one varies, so the call names every branch.\nIf Ice Veil above 50 percent: Ice Veil 50, melee LB\nIntermission Intermission, Kill Order",
    };
    public static readonly Callout sinboundHoly = new()
    {
        Description = "Sinbound Holy",
        Mechanic = "Sinbound Holy",
        Phase = 2,
        Key = "sinboundHoly",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "P2 Diamond Dust, After Knockback\nMT: Stack G1, rotate away\nOT: Stack G2, rotate away\nH1: Stack G1, rotate away\nH2: Stack G2, rotate away\nM1: Stack G1, rotate away\nM2: Stack G2, rotate away\nR1: Stack G1, rotate away\nR2: Stack G2, rotate away",
    };
    public static readonly Callout somberDance = new()
    {
        Description = "Somber Dance",
        Mechanic = "Somber Dance",
        Phase = 5,
        Key = "somberDance",
        Speech = "Move with Gaia",
        Text = "Move with Gaia" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "P4 Darklit Dragonsong, Somber Dance",
    };
    public static readonly Callout somberDanceTank = new()
    {
        Description = "Somber Dance",
        Mechanic = "Somber Dance",
        Phase = 5,
        Key = "somberDanceTank",
        Speech = "Near bait, then far",
        Text = "Near bait, then far" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        Notes = "Tanks only.",
    };
    public static readonly Callout spiritTaker = new()
    {
        Description = "Spirit Taker",
        Mechanic = "Spirit Taker",
        Phase = 4,
        Key = "spiritTaker",
        Speech = "Spread from box",
        Text = "Spread from box" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "This one varies, so the call names every branch.\nP3 Sextuple Apoc, Spirit Taker\nIf Two DPS untethered: G1 DPS, other side\nP4 Darklit Dragonsong, Protean Adjusts\nIf Both waters on the same side: Swap vertical\nP4 Darklit Dragonsong, Water Swap",
    };
    public static readonly Callout theHouseOfLight = new()
    {
        Description = "The House of Light",
        Mechanic = "The House of Light",
        Phase = 2,
        Key = "theHouseOfLight",
        Speech = "Proteans",
        Text = "Proteans" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout thePathOfDarkness = new()
    {
        Description = "The Path of Darkness",
        Mechanic = "The Path of Darkness",
        Phase = 6,
        Key = "thePathOfDarkness",
        Speech = "Wait one wave",
        Text = "Wait 1, step in" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "P5 Fulgent Blade, Step In\nNext Wave: Next crossing\nP5 Fulgent Blade, Next Wave",
    };

    public static readonly Callout cyclonicBreakB0 = new()
    {
        Description = "Cyclonic Break",
        Mechanic = "Cyclonic Break",
        Phase = 1,
        Key = "cyclonicBreakB0",
        Speech = "Proteans, then partners",
        Text = "Proteans, then partners" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout cyclonicBreakB1 = new()
    {
        Description = "Cyclonic Break",
        Mechanic = "Cyclonic Break",
        Phase = 1,
        Key = "cyclonicBreakB1",
        Speech = "Proteans, then spread",
        Text = "Proteans, then spread" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout cyclonicBreakB3 = new()
    {
        Description = "Cyclonic Break",
        Mechanic = "Cyclonic Break",
        Phase = 1,
        Key = "cyclonicBreakB3",
        Speech = "Move, partners",
        Text = "Move, partners" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout cyclonicBreakB4 = new()
    {
        Description = "Cyclonic Break",
        Mechanic = "Cyclonic Break",
        Phase = 1,
        Key = "cyclonicBreakB4",
        Speech = "Move, spread",
        Text = "Move, spread" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout cyclonicBreakB6 = new()
    {
        Description = "Cyclonic Break",
        Mechanic = "Cyclonic Break",
        Phase = 1,
        Key = "cyclonicBreakB6",
        Speech = "Move",
        Text = "Move" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout cyclonicBreakB8 = new()
    {
        Description = "Cyclonic Break",
        Mechanic = "Cyclonic Break",
        Phase = 1,
        Key = "cyclonicBreakB8",
        Speech = "Move",
        Text = "Move" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout turnOfTheHeavensB0 = new()
    {
        Description = "Turn of the Heavens",
        Mechanic = "Turn of the Heavens",
        Phase = 1,
        Key = "turnOfTheHeavensB0",
        Speech = "Blue safe",
        Text = "Blue safe" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout turnOfTheHeavensB1 = new()
    {
        Description = "Turn of the Heavens",
        Mechanic = "Turn of the Heavens",
        Phase = 1,
        Key = "turnOfTheHeavensB1",
        Speech = "Red safe",
        Text = "Red safe" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout turnOfTheHeavensB3 = new()
    {
        Description = "Turn of the Heavens",
        Mechanic = "Turn of the Heavens",
        Phase = 1,
        Key = "turnOfTheHeavensB3",
        Speech = "Out, north or south",
        Text = "Out, north or south" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout turnOfTheHeavensB4 = new()
    {
        Description = "Turn of the Heavens",
        Mechanic = "Turn of the Heavens",
        Phase = 1,
        Key = "turnOfTheHeavensB4",
        Speech = "Knockback through the middle",
        Text = "KB through mid" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "This one varies, so the call names every branch.\nIf Tethers split: Supports north, DPS south\nIf Both tethers on one side: Two move sides",
    };
    public static readonly Callout turnOfTheHeavensB5 = new()
    {
        Description = "Turn of the Heavens",
        Mechanic = "Turn of the Heavens",
        Phase = 1,
        Key = "turnOfTheHeavensB5",
        Speech = "Move in",
        Text = "Move in" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout utopianSkyB0 = new()
    {
        Description = "Utopian Sky",
        Mechanic = "Utopian Sky",
        Phase = 1,
        Key = "utopianSkyB0",
        Speech = "Stack later",
        Text = "Stack later" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout utopianSkyB1 = new()
    {
        Description = "Utopian Sky",
        Mechanic = "Utopian Sky",
        Phase = 1,
        Key = "utopianSkyB1",
        Speech = "Spread later",
        Text = "Spread later" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout utopianSkyB3 = new()
    {
        Description = "Utopian Sky",
        Mechanic = "Utopian Sky",
        Phase = 1,
        Key = "utopianSkyB3",
        Speech = "Light parties",
        Text = "Light parties" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout utopianSkyB4 = new()
    {
        Description = "Utopian Sky",
        Mechanic = "Utopian Sky",
        Phase = 1,
        Key = "utopianSkyB4",
        Speech = "Spread, four notches out",
        Text = "Spread, 4 notches" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout mirrorMirrorB0X9D1C = new()
    {
        Description = "Mirror Mirror",
        Mechanic = "Mirror Mirror",
        Phase = 2,
        Key = "mirrorMirrorB0X9D1C",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Partners 1\nOT: Partners 2\nH1: Partners 4\nH2: Partners 3\nM1: Partners 4\nM2: Partners 3\nR1: Partners 1\nR2: Partners 2",
    };
    public static readonly Callout mirrorMirrorB0X9D1D = new()
    {
        Description = "Mirror Mirror",
        Mechanic = "Mirror Mirror",
        Phase = 2,
        Key = "mirrorMirrorB0X9D1D",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Spread A\nOT: Spread B\nH1: Spread D\nH2: Spread C\nM1: Spread 4\nM2: Spread 3\nR1: Spread 1\nR2: Spread 2",
    };
    public static readonly Callout lightRampantB0 = new()
    {
        Description = "Light Rampant",
        Mechanic = "Light Rampant",
        Phase = 2,
        Key = "lightRampantB0",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "Chained: Out to tower\nNo chain: Drop to mid, left on 3rd",
    };
    public static readonly Callout lightRampantB1X9D1C = new()
    {
        Description = "Light Rampant",
        Mechanic = "Light Rampant",
        Phase = 2,
        Key = "lightRampantB1X9D1C",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Partners 1, then clocks\nOT: Partners 2, then clocks\nH1: Partners 4, then clocks\nH2: Partners 3, then clocks\nM1: Partners 4, then clocks\nM2: Partners 3, then clocks\nR1: Partners 1, then clocks\nR2: Partners 2, then clocks",
    };
    public static readonly Callout lightRampantB1X9D1D = new()
    {
        Description = "Light Rampant",
        Mechanic = "Light Rampant",
        Phase = 2,
        Key = "lightRampantB1X9D1D",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Spread A, clocks for proteans\nOT: Spread B, clocks for proteans\nH1: Spread D, clocks for proteans\nH2: Spread C, clocks for proteans\nM1: Spread 4, clocks for proteans\nM2: Spread 3, clocks for proteans\nR1: Spread 1, clocks for proteans\nR2: Spread 2, clocks for proteans",
    };
    public static readonly Callout twinStillnessB0 = new()
    {
        Description = "Twin Stillness",
        Mechanic = "Twin Stillness",
        Phase = 2,
        Key = "twinStillnessB0",
        Speech = "Back to front",
        Text = "Back to front" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout twinStillnessB1 = new()
    {
        Description = "Twin Stillness",
        Mechanic = "Twin Stillness",
        Phase = 2,
        Key = "twinStillnessB1",
        Speech = "Front",
        Text = "Front" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout twinSilenceB0 = new()
    {
        Description = "Twin Silence",
        Mechanic = "Twin Silence",
        Phase = 2,
        Key = "twinSilenceB0",
        Speech = "Front to back",
        Text = "Front to back" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout twinSilenceB1 = new()
    {
        Description = "Twin Silence",
        Mechanic = "Twin Silence",
        Phase = 2,
        Key = "twinSilenceB1",
        Speech = "Back",
        Text = "Back" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout endlessIceAgeB0 = new()
    {
        Description = "Endless Ice Age",
        Mechanic = "Endless Ice Age",
        Phase = 3,
        Key = "endlessIceAgeB0",
        Speech = "Kill crystals, bait cardinal",
        Text = "Kill crystals, bait cardinal" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout endlessIceAgeB1 = new()
    {
        Description = "Endless Ice Age",
        Mechanic = "Endless Ice Age",
        Phase = 3,
        Key = "endlessIceAgeB1",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout ultimateRelativityB0 = new()
    {
        Description = "Ultimate Relativity",
        Mechanic = "Ultimate Relativity",
        Phase = 4,
        Key = "ultimateRelativityB0",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout ultimateRelativityB4 = new()
    {
        Description = "Ultimate Relativity",
        Mechanic = "Ultimate Relativity",
        Phase = 4,
        Key = "ultimateRelativityB4",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "Eruption: Explode out\nWater: Water stack mid",
    };
    public static readonly Callout akhMornB0 = new()
    {
        Description = "Akh Morn",
        Mechanic = "Akh Morn",
        Phase = 5,
        Key = "akhMornB0",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: West if baiting, else middle\nOT: West if baiting, else middle\nH1: Stack middle, seven\nH2: Stack middle, seven\nM1: Stack middle, seven\nM2: Stack middle, seven\nR1: Stack middle, seven\nR2: Stack middle, seven",
    };
    public static readonly Callout akhMornB1 = new()
    {
        Description = "Akh Morn",
        Mechanic = "Akh Morn",
        Phase = 5,
        Key = "akhMornB1",
        Speech = "Full party stack",
        Text = "Full stack" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout darklitDragonsongB0 = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = "Darklit Dragonsong",
        Phase = 5,
        Key = "darklitDragonsongB0",
        Speech = "Raidwide, bowtie",
        Text = "Raidwide, bowtie" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout darklitDragonsongB1 = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = "Darklit Dragonsong",
        Phase = 5,
        Key = "darklitDragonsongB1",
        Speech = "Spread into stacks",
        Text = "Spread into stacks" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "This one varies, so the call names every branch.\nIf Tethered: Intercard\nIf Untethered: In, off the crystal",
    };
    public static readonly Callout darklitDragonsongB2X9D23 = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = "Darklit Dragonsong",
        Phase = 5,
        Key = "darklitDragonsongB2X9D23",
        Speech = "Stacks, west safe",
        Text = "Stacks, west safe" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout darklitDragonsongB2X9D24 = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = "Darklit Dragonsong",
        Phase = 5,
        Key = "darklitDragonsongB2X9D24",
        Speech = "Stacks, east safe",
        Text = "Stacks, east safe" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout darklitDragonsongB3 = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = "Darklit Dragonsong",
        Phase = 5,
        Key = "darklitDragonsongB3",
        Speech = "Tank baits",
        Text = "Tank baits" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout darklitDragonsongB4 = new()
    {
        Description = "Darklit Dragonsong",
        Mechanic = "Darklit Dragonsong",
        Phase = 5,
        Key = "darklitDragonsongB4",
        Speech = "Raidwide",
        Text = "Raidwide" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
    };
    public static readonly Callout sextupleApocB0 = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = "Sextuple Apoc",
        Phase = 4,
        Key = "sextupleApocB0",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: West, box formation\nOT: West, box formation\nH1: West, box formation\nH2: West, box formation\nM1: East, box formation\nM2: East, box formation\nR1: East, box formation\nR2: East, box formation",
    };
    public static readonly Callout sextupleApocB1 = new()
    {
        Description = "Sextuple Apoc",
        Mechanic = "Sextuple Apoc",
        Phase = 4,
        Key = "sextupleApocB1",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "Short stack: Short\nMedium stack: Medium\nLong stack: Long\nNo water: No water",
    };
    public static readonly Callout paradiseRegainedB0 = new()
    {
        Description = "Paradise Regained",
        Mechanic = "Paradise Regained",
        Phase = 6,
        Key = "paradiseRegainedB0",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: First tower, hold\nOT: First tower, hold\nH1: First tower\nH2: First tower\nM1: Left tower, far side\nM2: Right tower, far side\nR1: Left tower, far side\nR2: Right tower, far side",
    };
    public static readonly Callout paradiseRegainedB1X9D29 = new()
    {
        Description = "Paradise Regained",
        Mechanic = "Paradise Regained",
        Phase = 6,
        Key = "paradiseRegainedB1X9D29",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Left tower\nOT: Middle, in\nH1: First tower, out\nH2: First tower, out\nM1: First tower, out\nM2: First tower, out\nR1: First tower, out\nR2: First tower, out",
    };
    public static readonly Callout paradiseRegainedB1X9D79 = new()
    {
        Description = "Paradise Regained",
        Mechanic = "Paradise Regained",
        Phase = 6,
        Key = "paradiseRegainedB1X9D79",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Right tower\nOT: Out, off the tower\nH1: First tower, in\nH2: First tower, in\nM1: First tower, in\nM2: First tower, in\nR1: First tower, in\nR2: First tower, in",
    };
    public static readonly Callout paradiseRegainedB2X9D29 = new()
    {
        Description = "Paradise Regained",
        Mechanic = "Paradise Regained",
        Phase = 6,
        Key = "paradiseRegainedB2X9D29",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Out, past the boss\nOT: In, off the left tower\nH1: Between the towers\nH2: Between the towers\nM1: Left tower, in\nM2: Right tower, in\nR1: Left tower, in\nR2: Right tower, in",
    };
    public static readonly Callout paradiseRegainedB2X9D79 = new()
    {
        Description = "Paradise Regained",
        Mechanic = "Paradise Regained",
        Phase = 6,
        Key = "paradiseRegainedB2X9D79",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Middle, out\nOT: In, off the right tower\nH1: Between the towers\nH2: Between the towers\nM1: Left tower, out\nM2: Right tower, out\nR1: Left tower, out\nR2: Right tower, out",
    };
    public static readonly Callout paradiseRegainedB3 = new()
    {
        Description = "Paradise Regained",
        Mechanic = "Paradise Regained",
        Phase = 6,
        Key = "paradiseRegainedB3",
        Speech = "Third tower",
        Text = "Third tower" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };
    public static readonly Callout polarizingStrikesB0 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB0",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Front\nOT: Front\nH1: Stack behind\nH2: Stack behind\nM1: Stack behind\nM2: Stack behind\nR1: Stack behind\nR2: Stack behind",
    };
    public static readonly Callout polarizingStrikesB1 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB1",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Middle, swap sides\nOT: Middle, swap sides\nH1: Middle\nH2: Middle\nM1: Middle\nM2: Middle\nR1: Middle\nR2: Middle",
    };
    public static readonly Callout polarizingStrikesB2 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB2",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Stack behind\nOT: Stack behind\nH1: Stack behind\nH2: Stack behind\nM1: Front\nM2: Front\nR1: Stack behind\nR2: Stack behind",
    };
    public static readonly Callout polarizingStrikesB3 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB3",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Middle\nOT: Middle\nH1: Middle\nH2: Middle\nM1: Middle, swap sides\nM2: Middle, swap sides\nR1: Middle\nR2: Middle",
    };
    public static readonly Callout polarizingStrikesB4 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB4",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Stack behind\nOT: Stack behind\nH1: Stack behind\nH2: Stack behind\nM1: Stack behind\nM2: Stack behind\nR1: Front\nR2: Front",
    };
    public static readonly Callout polarizingStrikesB5 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB5",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Middle\nOT: Middle\nH1: Middle\nH2: Middle\nM1: Middle\nM2: Middle\nR1: Middle, swap sides\nR2: Middle, swap sides",
    };
    public static readonly Callout polarizingStrikesB6 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB6",
        Speech = "{seatSpeech}",
        Text = "{seat}" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
        Notes = "MT: Stack behind\nOT: Stack behind\nH1: Front\nH2: Front\nM1: Stack behind\nM2: Stack behind\nR1: Stack behind\nR2: Stack behind",
    };
    public static readonly Callout polarizingStrikesB7 = new()
    {
        Description = "Polarizing Strikes",
        Mechanic = "Polarizing Strikes",
        Phase = 6,
        Key = "polarizingStrikesB7",
        Speech = "Middle",
        Text = "Middle" + Callout.CountdownToken,
        FromDuration = true,
        LingerSeconds = Callout.DurationLinger,
        FromPlan = true,
    };

    public static readonly Callout provoked = TankActions.Provoked;

    public static readonly Callout shirked = TankActions.Shirked;

    public const double Cooldown = 15.0;

    public static Sequence Build() =>
        CastCalls.Cooled(Group, Cooldown,
            (AbsoluteZero, absoluteZero),
            (Brightfire, brightfire),
            (BurnishedGlory, burnishedGlory),
            (CrystallizeTime, crystallizeTime),
            (DiamondDust, diamondDust),
            (Explosion, explosion),
            (FallOfFaith, fallOfFaith),
            (FrigidNeedle, frigidNeedle),
            (FulgentBlade, fulgentBlade),
            (HallowedRay, hallowedRay),
            (HellSJudgment, hellSJudgment),
            (HiemalStorm, hiemalStorm),
            (LightRampant, lightRampant),
            (Materialization, materialization),
            (MemorySEnd, memorySEnd),
            (PandoraSBox, pandoraSBox),
            (ParadiseLost, paradiseLost),
            (QuadrupleSlap, quadrupleSlap),
            (ReflectedScytheKick, reflectedScytheKick),
            (ShellCrusher, shellCrusher),
            (ShockwavePulsar, shockwavePulsar),
            (SinboundBlizzardIii, sinboundBlizzardIii),
            (SpiritTaker, spiritTaker),
            (TheHouseOfLight, theHouseOfLight),
            (ThePathOfDarkness, thePathOfDarkness));

    public static Sequence CyclonicBreakBeats(IWorld world) =>
        Sequence.Repeat(Group + ".cyclonicBreak", 17,
            e => e.Is(EventKind.CastStart, 0x9CD0, 0x9CD4, 0x9D89, 0x9D8A),
            async (start, run) =>
            {
                if (start.Id is 0x9CD0 or 0x9D89) run.Call(cyclonicBreakB0, start);
                if (start.Id is 0x9CD4 or 0x9D8A) run.Call(cyclonicBreakB1, start);
                var e2 = await run.WaitEvent(EventKind.AbilityHit, 0x9CD1);
                if (start.Id is 0x9CD0 or 0x9D89) run.Call(cyclonicBreakB3, e2);
                if (start.Id is 0x9CD4 or 0x9D8A) run.Call(cyclonicBreakB4, e2);
                await run.WaitMs(1000);
                var e6 = await run.WaitEvent(EventKind.AbilityHit, 0x9CD2);
                run.Call(cyclonicBreakB6, e6);
                if (start.Id is 0x9CD0 or 0x9CD4)
                {
                await run.WaitMs(1000);
                }
                if (start.Id is 0x9CD0 or 0x9CD4)
                {
                var e8 = await run.WaitEvent(EventKind.AbilityHit, 0x9CD2);
                run.Call(cyclonicBreakB8, e8);
                }
            });
    public static Sequence TurnOfTheHeavensBeats(IWorld world) =>
        Sequence.Repeat(Group + ".turnOfTheHeavens", 22,
            e => e.Is(EventKind.CastStart, 0x9CD6, 0x9CD7),
            async (start, run) =>
            {
                if (start.Id is 0x9CD6) run.Call(turnOfTheHeavensB0, start);
                if (start.Id is 0x9CD7) run.Call(turnOfTheHeavensB1, start);
                await run.WaitMs(3700);
                var e3 = await run.FindOrWaitForCast(world, e => e.Id is 0x9CE3);
                if (e3 is null) return;
                run.Call(turnOfTheHeavensB3, e3);
                var e4 = await run.FindOrWaitForCast(world, e => e.Id is 0x9CE1);
                if (e4 is null) return;
                run.Call(turnOfTheHeavensB4, e4);
                await run.WaitCastFinished(e4);
                run.Call(turnOfTheHeavensB5, e4);
            });
    public static Sequence UtopianSkyBeats(IWorld world) =>
        Sequence.Repeat(Group + ".utopianSky", 14,
            e => e.Is(EventKind.CastStart, 0x9CDA, 0x9CDB),
            async (start, run) =>
            {
                if (start.Id is 0x9CDA) run.Call(utopianSkyB0, start);
                if (start.Id is 0x9CDB) run.Call(utopianSkyB1, start);
                var e2 = await run.FindOrWaitForCast(world, e => e.Id is 0x9CDE);
                if (e2 is null) return;
                if (start.Id is 0x9CDA) run.Call(utopianSkyB3, e2);
                if (start.Id is 0x9CDB) run.Call(utopianSkyB4, e2);
            });
    public static Sequence MirrorMirrorBeats(IWorld world) =>
        Sequence.Repeat(Group + ".mirrorMirror", 45,
            e => e.Is(EventKind.CastStart, 0x9CF3),
            async (start, run) =>
            {
                var e0 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D1C or 0x9D1D);
                if (e0 is null) return;
                SeatCalls.Say(run, e0.Id == 0x9D1C ? mirrorMirrorB0X9D1C : mirrorMirrorB0X9D1D, e0, world,
                e0.Id == 0x9D1C ? new[] {"Partners 1", "Partners 2", "Partners 4", "Partners 3", "Partners 4", "Partners 3", "Partners 1", "Partners 2"} : new[] {"Spread A", "Spread B", "Spread D", "Spread C", "Spread 4", "Spread 3", "Spread 1", "Spread 2"},
                e0.Id == 0x9D1C ? new[] {"Partners, northwest at one", "Partners, northeast at two", "Partners, southwest at four", "Partners, southeast at three", "Partners, southwest at four", "Partners, southeast at three", "Partners, northwest at one", "Partners, northeast at two"} : new[] {"Spread north at A", "Spread east at B", "Spread west at D", "Spread south at C", "Spread southwest at four", "Spread southeast at three", "Spread northwest at one", "Spread northeast at two"});
            });
    public static Sequence LightRampantBeats(IWorld world) =>
        Sequence.Repeat(Group + ".lightRampant", 60,
            e => e.Is(EventKind.CastStart, 0x9D14),
            async (start, run) =>
            {
                var e0 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D1B);
                if (e0 is null) return;
                DebuffCalls.Say(run, lightRampantB0, e0, world,
                    [new DebuffCalls.Rule(0x103E, 0, "Out to tower", "Out to tower"),
                     new DebuffCalls.Rule(0x103E, 0, "Drop to mid, left on 3rd", "Drop to the middle, left on the third") { Absent = true }]);
                var e1 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D1C or 0x9D1D);
                if (e1 is null) return;
                SeatCalls.Say(run, e1.Id == 0x9D1C ? lightRampantB1X9D1C : lightRampantB1X9D1D, e1, world,
                e1.Id == 0x9D1C ? new[] {"Partners 1, then clocks", "Partners 2, then clocks", "Partners 4, then clocks", "Partners 3, then clocks", "Partners 4, then clocks", "Partners 3, then clocks", "Partners 1, then clocks", "Partners 2, then clocks"} : new[] {"Spread A, clocks for proteans", "Spread B, clocks for proteans", "Spread D, clocks for proteans", "Spread C, clocks for proteans", "Spread 4, clocks for proteans", "Spread 3, clocks for proteans", "Spread 1, clocks for proteans", "Spread 2, clocks for proteans"},
                e1.Id == 0x9D1C ? new[] {"Partners northwest at one, then clocks", "Partners northeast at two, then clocks", "Partners southwest at four, then clocks", "Partners southeast at three, then clocks", "Partners southwest at four, then clocks", "Partners southeast at three, then clocks", "Partners northwest at one, then clocks", "Partners northeast at two, then clocks"} : new[] {"Spread north at A, then clocks", "Spread east at B, then clocks", "Spread west at D, then clocks", "Spread south at C, then clocks", "Spread southwest at four, then clocks", "Spread southeast at three, then clocks", "Spread northwest at one, then clocks", "Spread northeast at two, then clocks"});
            });
    public static Sequence TwinStillnessBeats(IWorld world) =>
        Sequence.Repeat(Group + ".twinStillness", 6,
            e => e.Is(EventKind.CastStart, 0x9D01),
            async (start, run) =>
            {
                run.Call(twinStillnessB0, start);
                await run.WaitCastFinished(start);
                run.Call(twinStillnessB1, start);
            });
    public static Sequence TwinSilenceBeats(IWorld world) =>
        Sequence.Repeat(Group + ".twinSilence", 6,
            e => e.Is(EventKind.CastStart, 0x9D02),
            async (start, run) =>
            {
                run.Call(twinSilenceB0, start);
                await run.WaitCastFinished(start);
                run.Call(twinSilenceB1, start);
            });
    public static Sequence EndlessIceAgeBeats(IWorld world) =>
        Sequence.Repeat(Group + ".endlessIceAge", 11,
            e => e.Is(EventKind.CastStart, 0x9D43),
            async (start, run) =>
            {
                run.Call(endlessIceAgeB0, start);
                await run.WaitMs(7000);
                run.Call(endlessIceAgeB1, start);
            });
    public static Sequence UltimateRelativityBeats(IWorld world) =>
        Sequence.Repeat(Group + ".ultimateRelativity", 83,
            e => e.Is(EventKind.CastStart, 0x9D4A),
            async (start, run) =>
            {
                run.Call(ultimateRelativityB0, start);
                var e1 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D63);
                if (e1 is null) return;
                var e2 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D63);
                if (e2 is null) return;
                var e3 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D63);
                if (e3 is null) return;
                await run.WaitMs(11000);
                DebuffCalls.Say(run, ultimateRelativityB4, e3, world,
                    [new DebuffCalls.Rule(0x099C, 43, "Explode out", "Explode out"),
                     new DebuffCalls.Rule(0x099D, 43, "Water stack mid", "Water stack middle")]);
            });
    public static Sequence AkhMornBeats(IWorld world) =>
        Sequence.Repeat(Group + ".akhMorn", 13,
            e => e.Is(EventKind.CastStart, 0x9D37),
            async (start, run) =>
            {
                SeatCalls.Say(run, akhMornB0, start, world,
                    ["West if baiting, else middle", "West if baiting, else middle", "Stack middle, seven", "Stack middle, seven", "Stack middle, seven", "Stack middle, seven", "Stack middle, seven", "Stack middle, seven"],
                    ["West if baiting, otherwise stack middle", "West if baiting, otherwise stack middle", "Stack middle with the seven", "Stack middle with the seven", "Stack middle with the seven", "Stack middle with the seven", "Stack middle with the seven", "Stack middle with the seven"]);
                var e1 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D39);
                if (e1 is null) return;
                run.Call(akhMornB1, e1);
            });
    public static Sequence DarklitDragonsongBeats(IWorld world) =>
        Sequence.Repeat(Group + ".darklitDragonsong", 45,
            e => e.Is(EventKind.CastStart, 0x9D2F),
            async (start, run) =>
            {
                run.Call(darklitDragonsongB0, start);
                var e1 = await run.WaitEvent(EventKind.AbilityHit, 0x9CFE);
                run.Call(darklitDragonsongB1, e1);
                var e2 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D23 or 0x9D24);
                if (e2 is null) return;
                run.Call(e2.Id == 0x9D23 ? darklitDragonsongB2X9D23 : darklitDragonsongB2X9D24, e2);
                await run.WaitCastFinished(e2);
                run.Call(darklitDragonsongB3, e2);
                var e4 = await run.FindOrWaitForCast(world, e => e.Id is 0x9CEE);
                if (e4 is null) return;
                run.Call(darklitDragonsongB4, e4);
            });
    public static Sequence SextupleApocBeats(IWorld world) =>
        Sequence.Repeat(Group + ".sextupleApoc", 20,
            e => e.Is(EventKind.CastStart, 0x9D4E),
            async (start, run) =>
            {
                await run.WaitMs(5000);
                SeatCalls.Say(run, sextupleApocB0, start, world,
                    ["West, box formation", "West, box formation", "West, box formation", "West, box formation", "East, box formation", "East, box formation", "East, box formation", "East, box formation"],
                    ["West, box formation", "West, box formation", "West, box formation", "West, box formation", "East, box formation", "East, box formation", "East, box formation", "East, box formation"]);
                await run.WaitMs(2000);
                DebuffCalls.Say(run, sextupleApocB1, start, world,
                    [new DebuffCalls.Rule(0x099D, 10, "Short", "Short"),
                     new DebuffCalls.Rule(0x099D, 29, "Medium", "Medium"),
                     new DebuffCalls.Rule(0x099D, 38, "Long", "Long"),
                     new DebuffCalls.Rule(0x099D, 0, "No water", "No water") { Absent = true }]);
            });
    public static Sequence ParadiseRegainedBeats(IWorld world) =>
        Sequence.Repeat(Group + ".paradiseRegained", 25,
            e => e.Is(EventKind.CastStart, 0x9D7F),
            async (start, run) =>
            {
                SeatCalls.Say(run, paradiseRegainedB0, start, world,
                    ["First tower, hold", "First tower, hold", "First tower", "First tower", "Left tower, far side", "Right tower, far side", "Left tower, far side", "Right tower, far side"],
                    ["First tower, hold", "First tower, hold", "First tower", "First tower", "Left tower, far side", "Right tower, far side", "Left tower, far side", "Right tower, far side"]);
                var e1 = await run.FindOrWaitForCast(world, e => e.Id is 0x9D29 or 0x9D79);
                if (e1 is null) return;
                SeatCalls.Say(run, e1.Id == 0x9D29 ? paradiseRegainedB1X9D29 : paradiseRegainedB1X9D79, e1, world,
                e1.Id == 0x9D29 ? new[] {"Left tower", "Middle, in", "First tower, out", "First tower, out", "First tower, out", "First tower, out", "First tower, out", "First tower, out"} : new[] {"Right tower", "Out, off the tower", "First tower, in", "First tower, in", "First tower, in", "First tower, in", "First tower, in", "First tower, in"},
                e1.Id == 0x9D29 ? new[] {"Left tower", "Middle, stay in", "First tower, dodge out", "First tower, dodge out", "First tower, dodge out", "First tower, dodge out", "First tower, dodge out", "First tower, dodge out"} : new[] {"Right tower", "Out, off the tower", "First tower, dodge in", "First tower, dodge in", "First tower, dodge in", "First tower, dodge in", "First tower, dodge in", "First tower, dodge in"});
                await run.WaitCastFinished(e1);
                SeatCalls.Say(run, e1.Id == 0x9D29 ? paradiseRegainedB2X9D29 : paradiseRegainedB2X9D79, e1, world,
                e1.Id == 0x9D29 ? new[] {"Out, past the boss", "In, off the left tower", "Between the towers", "Between the towers", "Left tower, in", "Right tower, in", "Left tower, in", "Right tower, in"} : new[] {"Middle, out", "In, off the right tower", "Between the towers", "Between the towers", "Left tower, out", "Right tower, out", "Left tower, out", "Right tower, out"},
                e1.Id == 0x9D29 ? new[] {"Out, past the boss", "In, off the left tower", "Between the towers", "Between the towers", "Left tower, move in", "Right tower, move in", "Left tower, move in", "Right tower, move in"} : new[] {"Middle, dodge out", "In, off the right tower", "Between the towers", "Between the towers", "Left tower, move out", "Right tower, move out", "Left tower, move out", "Right tower, move out"});
                await run.WaitMs(2000);
                run.Call(paradiseRegainedB3, e1);
            });
    public static Sequence PolarizingStrikesBeats(IWorld world) =>
        Sequence.Repeat(Group + ".polarizingStrikes", 32,
            e => e.Is(EventKind.CastStart, 0x9D7C),
            async (start, run) =>
            {
                SeatCalls.Say(run, polarizingStrikesB0, start, world,
                    ["Front", "Front", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind"],
                    ["Front", "Front", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind"]);
                var e1 = await run.WaitEvent(EventKind.AbilityHit, 0x9D7D, 0x9D7E);
                SeatCalls.Say(run, polarizingStrikesB1, e1, world,
                    ["Middle, swap sides", "Middle, swap sides", "Middle", "Middle", "Middle", "Middle", "Middle", "Middle"],
                    ["Middle, swap sides", "Middle, swap sides", "Middle", "Middle", "Middle", "Middle", "Middle", "Middle"]);
                var e2 = await run.WaitEvent(EventKind.AbilityHit, 0x9CB7);
                SeatCalls.Say(run, polarizingStrikesB2, e2, world,
                    ["Stack behind", "Stack behind", "Stack behind", "Stack behind", "Front", "Front", "Stack behind", "Stack behind"],
                    ["Stack behind", "Stack behind", "Stack behind", "Stack behind", "Front", "Front", "Stack behind", "Stack behind"]);
                var e3 = await run.WaitEvent(EventKind.AbilityHit, 0x9D7D, 0x9D7E);
                SeatCalls.Say(run, polarizingStrikesB3, e3, world,
                    ["Middle", "Middle", "Middle", "Middle", "Middle, swap sides", "Middle, swap sides", "Middle", "Middle"],
                    ["Middle", "Middle", "Middle", "Middle", "Middle, swap sides", "Middle, swap sides", "Middle", "Middle"]);
                var e4 = await run.WaitEvent(EventKind.AbilityHit, 0x9CB7);
                SeatCalls.Say(run, polarizingStrikesB4, e4, world,
                    ["Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Front", "Front"],
                    ["Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Stack behind", "Front", "Front"]);
                var e5 = await run.WaitEvent(EventKind.AbilityHit, 0x9D7D, 0x9D7E);
                SeatCalls.Say(run, polarizingStrikesB5, e5, world,
                    ["Middle", "Middle", "Middle", "Middle", "Middle", "Middle", "Middle, swap sides", "Middle, swap sides"],
                    ["Middle", "Middle", "Middle", "Middle", "Middle", "Middle", "Middle, swap sides", "Middle, swap sides"]);
                var e6 = await run.WaitEvent(EventKind.AbilityHit, 0x9CB7);
                SeatCalls.Say(run, polarizingStrikesB6, e6, world,
                    ["Stack behind", "Stack behind", "Front", "Front", "Stack behind", "Stack behind", "Stack behind", "Stack behind"],
                    ["Stack behind", "Stack behind", "Front", "Front", "Stack behind", "Stack behind", "Stack behind", "Stack behind"]);
                var e7 = await run.WaitEvent(EventKind.AbilityHit, 0x9D7D, 0x9D7E);
                run.Call(polarizingStrikesB7, e7);
            });

    public static IEnumerable<Sequence> Tanks(IWorld world)
    {
        yield return TankActions.Build(Group, world);

        yield return SeatCalls.Cooled(Group + "Seat", Cooldown, world,
            new SeatCalls.Seated([AkhMorn], akhMorn,
                ["Left, light stack", "Right, dark stack", "Left, light stack", "Right, dark stack", "Left, light stack", "Right, dark stack", "Left, light stack", "Right, dark stack"],
                ["Left, the light stack", "Right, the dark stack", "Left, the light stack", "Right, the dark stack", "Left, the light stack", "Right, the dark stack", "Left, the light stack", "Right, the dark stack"]),
            new SeatCalls.Seated([AkhRhai], akhRhai,
                ["Wings, dodge NE", "Wings, dodge NE", "Wings, dodge NW", "Wings, dodge NW", "Wings, dodge SE", "Wings, dodge SE", "Wings, dodge SW", "Wings, dodge SW"],
                ["Wings, dodge northeast", "Wings, dodge northeast", "Wings, dodge northwest", "Wings, dodge northwest", "Wings, dodge southeast", "Wings, dodge southeast", "Wings, dodge southwest", "Wings, dodge southwest"]),
            new SeatCalls.Seated([MirrorMirror], mirrorMirror,
                ["Nearest red mirror, protean off H1", "Nearest red mirror", "Nearest red mirror, protean off MT", "Nearest red mirror", "Nearest red mirror, protean mid", "Nearest red mirror", "Nearest red mirror, protean mid", "Nearest red mirror"],
                ["Nearest red mirror", "Nearest red mirror", "Nearest red mirror", "Nearest red mirror", "Nearest red mirror", "Nearest red mirror", "Nearest red mirror", "Nearest red mirror"]),
            new SeatCalls.Seated([SinboundHoly], sinboundHoly,
                ["Stack G1, rotate away", "Stack G2, rotate away", "Stack G1, rotate away", "Stack G2, rotate away", "Stack G1, rotate away", "Stack G2, rotate away", "Stack G1, rotate away", "Stack G2, rotate away"],
                ["Stack group one, rotate away", "Stack group two, rotate away", "Stack group one, rotate away", "Stack group two, rotate away", "Stack group one, rotate away", "Stack group two, rotate away", "Stack group one, rotate away", "Stack group two, rotate away"]));





        yield return CyclonicBreakBeats(world);
        yield return TurnOfTheHeavensBeats(world);
        yield return UtopianSkyBeats(world);
        yield return MirrorMirrorBeats(world);
        yield return LightRampantBeats(world);
        yield return TwinStillnessBeats(world);
        yield return TwinSilenceBeats(world);
        yield return EndlessIceAgeBeats(world);
        yield return UltimateRelativityBeats(world);
        yield return AkhMornBeats(world);
        yield return DarklitDragonsongBeats(world);
        yield return SextupleApocBeats(world);
        yield return ParadiseRegainedBeats(world);
        yield return PolarizingStrikesBeats(world);

        yield return CastCalls.CooledWhen(Group + "Tank", Cooldown,
            () => JobKinds.Tanking(world),
            (BlackHaloTank, blackHaloTank),
            (DarkestDanceTank, darkestDanceTank),
            (PowderMarkTrailTank, powderMarkTrailTank),
            (QuadrupleSlapXTank, quadrupleSlapXTank),
            (SomberDanceTank, somberDanceTank));

        yield return CastCalls.CooledWhen(Group + "NotTank", Cooldown,
            () => !JobKinds.Tanking(world),
            (BlackHalo, blackHalo),
            (PowderMarkTrail, powderMarkTrail),
            (SomberDance, somberDance));
    }

    [ModuleInitializer]
    internal static void Register()
    {
        FightPlans.Register(new PlannedFight(
            "fru", "Futures Rewritten", "Dawntrail Ultimate", 1238, true));
        LocalFights.Register(new LocalFight(
            "fru", Group, "Futures Rewritten", 0, new FruFight(), Build)
        {
            PhaseNames = ["P1 Fatebreaker", "P2 Usurper of Frost", "Intermission Crystals", "P3 Oracle of Darkness", "P4 Usurper and Oracle", "P5 Pandora"],
            Extra = Tanks,
        });
    }
}
