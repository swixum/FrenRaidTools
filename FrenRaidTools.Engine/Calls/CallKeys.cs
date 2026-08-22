namespace FrenRaidTools.Engine;

public static class CallKeys
{
    private static readonly Dictionary<string, string> Renamed =
        new(StringComparer.Ordinal)
    {
        ["ks2FakeAccelLong"] = "secondFakeAccelLong",
        ["ks2FakeAccelLongShriek"] = "secondFakeAccelLongShriek",
        ["ks2FakeAccelShort"] = "secondFakeAccelShort",
        ["ks2FakeAccelShortShriek"] = "secondFakeAccelShortShriek",
        ["ks2FakeLightning"] = "secondFakeLightning",
        ["ks2FakeWater"] = "secondFakeWater",
        ["ks2RealAccelLong"] = "secondRealAccelLong",
        ["ks2RealAccelLongShriek"] = "secondRealAccelLongShriek",
        ["ks2RealAccelShort"] = "secondRealAccelShort",
        ["ks2RealAccelShortShriek"] = "secondRealAccelShortShriek",
        ["ks2RealLightning"] = "secondRealLightning",
        ["ks2RealWater"] = "secondRealWater",
        ["ks3FakeBWAF"] = "fakeBlackAllag",
        ["ks3FakeBWBD"] = "fakeBlackDeath",
        ["ks3FakeWWAF"] = "fakeWhiteAllag",
        ["ks3FakeWWBD"] = "fakeWhiteDeath",
        ["ks3RealBWAF"] = "realBlackAllag",
        ["ks3RealBWBD"] = "realBlackDeath",
        ["ks3RealWWAF"] = "realWhiteAllag",
        ["ks3RealWWBD"] = "realWhiteDeath",
        ["ks3error"] = "kefkaSaysError",
        ["ks3standInBlack"] = "standInBlack",
        ["ks3standInWhite"] = "standInWhite",
        ["ksFakeAccelLong"] = "fakeAccelLong",
        ["ksFakeAccelLongShriek"] = "fakeAccelLongShriek",
        ["ksFakeAccelShort"] = "fakeAccelShort",
        ["ksFakeAccelShortShriek"] = "fakeAccelShortShriek",
        ["ksFakeDyn"] = "fakeDynamicFluid",
        ["ksFakeDyn2"] = "secondFakeDynamicFluid",
        ["ksFakeEnt"] = "fakeEntropy",
        ["ksFakeEnt2"] = "secondFakeEntropy",
        ["ksFakeIceFakeThunder"] = "fakeIceFakeThunder",
        ["ksFakeIceRealThunder"] = "fakeIceRealThunder",
        ["ksFakeLightning"] = "fakeLightning",
        ["ksFakeWater"] = "fakeWater",
        ["ksFirstBombSetAccelNothing"] = "firstSetAccelNothing",
        ["ksFirstBombSetAccelSpread"] = "firstSetAccelSpread",
        ["ksFirstBombSetAccelStack"] = "firstSetAccelStack",
        ["ksFirstBombSetNothing"] = "firstSetNothing",
        ["ksFirstBombSetSpread"] = "firstSetSpread",
        ["ksFirstBombSetStack"] = "firstSetStack",
        ["ksFirstEntropyDynamic"] = "firstEntropyDynamic",
        ["ksFirstEntropyDynamicMove"] = "firstEntropyDynamicMove",
        ["ksFirstEntropyDynamicStay"] = "firstEntropyDynamicStay",
        ["ksRealAccelLong"] = "realAccelLong",
        ["ksRealAccelLongShriek"] = "realAccelLongShriek",
        ["ksRealAccelShort"] = "realAccelShort",
        ["ksRealAccelShortShriek"] = "realAccelShortShriek",
        ["ksRealDyn"] = "realDynamicFluid",
        ["ksRealDyn2"] = "secondRealDynamicFluid",
        ["ksRealEnt"] = "realEntropy",
        ["ksRealEnt2"] = "secondRealEntropy",
        ["ksRealIceFakeThunder"] = "realIceFakeThunder",
        ["ksRealIceRealThunder"] = "realIceRealThunder",
        ["ksRealLightning"] = "realLightning",
        ["ksRealWater"] = "realWater",
        ["ksSecondBombSetAccelNothing"] = "secondSetAccelNothing",
        ["ksSecondBombSetAccelSpread"] = "secondSetAccelSpread",
        ["ksSecondBombSetAccelStack"] = "secondSetAccelStack",
        ["ksSecondBombSetNothing"] = "secondSetNothing",
        ["ksSecondBombSetSpread"] = "secondSetSpread",
        ["ksSecondBombSetStack"] = "secondSetStack",
        ["ksSecondEntropyDynamicBothFake"] = "secondEntropyDynamicBothFake",
        ["ksSecondEntropyDynamicBothReal"] = "secondEntropyDynamicBothReal",
        ["ksSecondEntropyDynamicFakeIce"] = "secondEntropyDynamicFakeIce",
        ["ksSecondEntropyDynamicFakeThunder"] = "secondEntropyDynamicFakeThunder",
        ["ksSecondEntropyDynamicMoveBothFake"] = "secondEntropyDynamicMoveBothFake",
        ["ksSecondEntropyDynamicMoveBothReal"] = "secondEntropyDynamicMoveBothReal",
        ["ksSecondEntropyDynamicMoveFakeIce"] = "secondEntropyDynamicMoveFakeIce",
        ["ksSecondEntropyDynamicMoveFakeThunder"] = "secondEntropyDynamicMoveFakeThunder",
        ["ksSecondEntropyDynamicStayBothFake"] = "secondEntropyDynamicStayBothFake",
        ["ksSecondEntropyDynamicStayBothReal"] = "secondEntropyDynamicStayBothReal",
        ["ksSecondEntropyDynamicStayFakeIce"] = "secondEntropyDynamicStayFakeIce",
        ["ksSecondEntropyDynamicStayFakeThunder"] = "secondEntropyDynamicStayFakeThunder",
        ["ksSecondShriek"] = "secondShriek",
        ["ksSecondShriekOnYou"] = "secondShriekOnYou",
        ["ksThunderShriek"] = "thunderShriek",
        ["ksThunderShriekOnYou"] = "thunderShriekOnYou",
        ["lc1"] = "limitCutNumber1",
        ["lc2"] = "limitCutNumber2",
        ["lc3"] = "limitCutNumber3",
        ["lc4"] = "limitCutNumber4",
        ["lc5"] = "limitCutNumber5",
        ["lc6"] = "limitCutNumber6",
        ["lc7"] = "limitCutNumber7",
        ["lc8"] = "limitCutNumber8",
        ["lcInitial"] = "limitCutInitial",
        ["lcUnknown"] = "unknown",
        ["ttConfettiNotOnYou"] = "confettiNotOnYou",
        ["ttConfettiOnYou"] = "confettiOnYou",
        ["ttConfuseTether"] = "confuseTether",
        ["ttConfusionTetherInitial"] = "confusionTetherInitial",
        ["ttEE"] = "doubleEast",
        ["ttEN"] = "eastToNorth",
        ["ttES"] = "eastToSouth",
        ["ttEarlyFakeGaze"] = "earlyFakeGaze",
        ["ttEarlyRealGaze"] = "earlyRealGaze",
        ["ttElementMechanic"] = "elementMechanic",
        ["ttError"] = "arrowError",
        ["ttInitial"] = "arrowsInitial",
        ["ttNE"] = "northToEast",
        ["ttNN"] = "doubleNorth",
        ["ttNW"] = "northToWest",
        ["ttOnlyE"] = "onlyEast",
        ["ttOnlyN"] = "onlyNorth",
        ["ttOnlyS"] = "onlySouth",
        ["ttOnlyW"] = "onlyWest",
        ["ttSE"] = "southToEast",
        ["ttSS"] = "doubleSouth",
        ["ttSW"] = "southToWest",
        ["ttSleepTether"] = "sleepTether",
        ["ttSleepTetherInitial"] = "sleepTetherInitial",
        ["ttWN"] = "westToNorth",
        ["ttWS"] = "westToSouth",
        ["ttWW"] = "doubleWest",
    };

    public static int RenameCount => Renamed.Count;

    public static string Current(string key) =>
        Renamed.TryGetValue(key, out var now) ? now : key;

    public static bool Moved(string key) => Renamed.ContainsKey(key);

    public static IEnumerable<string> Old => Renamed.Keys;

    public static int Carry(ISet<string> keys)
    {
        var moved = keys.Where(Moved).ToList();

        foreach (var old in moved)
        {
            keys.Remove(old);
            keys.Add(Current(old));
        }

        return moved.Count;
    }

    public static int Carry<T>(IDictionary<string, T> byKey)
    {
        var moved = byKey.Keys.Where(Moved).ToList();

        foreach (var old in moved)
        {
            var kept = byKey[old];
            byKey.Remove(old);
            byKey[Current(old)] = kept;
        }

        return moved.Count;
    }
}
