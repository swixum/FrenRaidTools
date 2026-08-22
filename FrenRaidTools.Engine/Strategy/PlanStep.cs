using System.Text;

namespace FrenRaidTools.Engine;

public static class PlanStep
{
    public const string BaitToken = "BAIT FUTURE/PAST";
    public const string BaitUnknown = "Bait Past or Future";
    public const string BaitFuture = "Bait Future";
    public const string BaitPast = "Bait Past";
    public const string Owned = "Forsaken ";
    public const char Arrow = '→';

    public const string Remember = "REMEMBER DEBUFF";
    public const string RememberNew = "REMEMBER NEW DEBUFF";
    public const string YourDebuff = "Yours is";
    public const string NewDebuff = "New debuff";
    public const string NoDebuff = "None";

    public static readonly string[] Guesses = ["If ", "Whoever ", "Whichever "];

    public const string FluidParam = "myFluid";

    public static bool Conditional(string line)
    {
        foreach (var opener in Guesses)
            if (line.StartsWith(opener, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    public static string Trim(string line)
    {
        var built = new System.Text.StringBuilder(line.Length);
        var depth = 0;

        foreach (var c in line)
        {
            if (c == '(') { depth++; continue; }
            if (c == ')') { if (depth > 0) depth--; continue; }
            if (depth == 0) built.Append(c);
        }

        return built.ToString().Replace("  ", " ").Replace(" ,", ",").Trim();
    }

    public const string Cue = "Wait for ";
    public const string Payoff = ", then ";

    public static string Now(string line)
    {
        if (!line.StartsWith(Cue, StringComparison.OrdinalIgnoreCase)) return line;

        var at = line.IndexOf(Payoff, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return line;

        var rest = line[(at + Payoff.Length)..].Trim();
        return rest.Length == 0 ? line : char.ToUpperInvariant(rest[0]) + rest[1..];
    }

    public const string Middle = "Middle";
    public const int CongaHalf = 4;

    public static readonly string[] CongaEnds = ["East", "West"];

    public static string FromMiddle(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return line;
        if (!parts[1].Equals("from", StringComparison.OrdinalIgnoreCase)) return line;
        if (!CongaEnds.Contains(parts[2], StringComparer.OrdinalIgnoreCase)) return line;

        for (var n = 1; n <= CongaHalf; n++)
            if (parts[0].Equals(PlanTether.Ordinal(n), StringComparison.OrdinalIgnoreCase))
                return $"{PlanTether.Ordinal(CongaHalf + 1 - n)} from {Middle}";

        return line;
    }

    public const string OnHitbox = "on hitbox";

    public static string Bare(string line)
    {
        if (!line.EndsWith(OnHitbox, StringComparison.OrdinalIgnoreCase)) return line;

        var kept = line[..^OnHitbox.Length].TrimEnd();
        if (kept.EndsWith(',')) kept = kept[..^1].TrimEnd();

        return kept.Length == 0 ? line : kept;
    }

    public static string? Settled(string line, string? mine)
    {
        line = FromMiddle(Now(line));

        if (!Conditional(line)) return Bare(Trim(line));
        if (string.IsNullOrWhiteSpace(mine)) return null;
        if (!line.Contains(mine, StringComparison.OrdinalIgnoreCase)) return null;

        var at = line.IndexOf(',');
        if (at < 0) return null;

        var rest = Trim(line[(at + 1)..]);
        return rest.Length == 0 ? null : Bare(char.ToUpperInvariant(rest[0]) + rest[1..]);
    }

    public static bool IsDebuffNote(string line) =>
        line.StartsWith(YourDebuff, StringComparison.Ordinal) ||
        line.StartsWith(NewDebuff, StringComparison.Ordinal);

    public static bool Known(string? mine) =>
        !string.IsNullOrWhiteSpace(mine) &&
        !string.Equals(mine, NoDebuff, StringComparison.OrdinalIgnoreCase);

    public const int WordLength = 4;

    public static bool Repeats(string label, IEnumerable<string> lines)
    {
        var said = string.Join(' ', lines);

        foreach (var word in label.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.Length < WordLength) continue;
            if (said.Contains(word, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    public const string BaitClones = "Bait clone, North outer";
    public const string BaitCones = "Bait cone on marker";

    public static string? NonTower(string? role) => role switch
    {
        "Tank" or "Melee" => BaitClones,
        "Healer" or "Ranged" => BaitCones,
        _ => null,
    };

    public const string LastNorthOuter = "North, outer hitbox";
    public const string LastBackLeftOfRight = "Back left of Right Tower";
    public const string LastWestOfLeft = "West of Left Tower, bait cleave";
    public const string LastEastOfRight = "East of Right Tower, bait cleave";

    public static string? LastTower(string? role) => role switch
    {
        "Tank" => LastNorthOuter,
        "Melee" => LastBackLeftOfRight,
        "Healer" => LastWestOfLeft,
        "Ranged" => LastEastOfRight,
        _ => null,
    };

    public const string CloseFarToken = "Close/Far";

    public static string CloseOrFar(string seat) =>
        seat is "R1" or "R2" or "H2" ? "Far" : "Close";

    public static string SettleDepth(string line, string seat) =>
        line.Contains(CloseFarToken, StringComparison.OrdinalIgnoreCase)
            ? line.Replace(CloseFarToken, CloseOrFar(seat), StringComparison.OrdinalIgnoreCase)
            : line;

    public const string PrioTower = "HTMR Tower Prio";
    public const string PrioTowerShort = "HTMR Tower";

    public static string Resolve(string line, string? tower)
    {
        if (string.IsNullOrEmpty(tower)) return line;

        return line
            .Replace(PrioTower, tower, StringComparison.OrdinalIgnoreCase)
            .Replace(PrioTowerShort, tower, StringComparison.OrdinalIgnoreCase);
    }

    public const string LastTowerParam = "lastTower";

    public static string BaitLine(bool? future) =>
        future is null ? BaitUnknown : future.Value ? BaitFuture : BaitPast;

    public static bool HasBait(StrategyBlock block)
    {
        foreach (var line in block.Lines)
            if (line.Trim().StartsWith(BaitToken, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    public static IReadOnlyList<string> BaitOnly(StrategyBlock block, bool? future) =>
        HasBait(block) ? [BaitLine(future)] : [];

    public const string BaitStart = "Bait ";
    public const string Clone = "clone";
    public const string Cone = "cone";

    public static string? Baits(string? role) => role switch
    {
        "Tank" or "Melee" => Clone,
        "Healer" or "Ranged" => Cone,
        _ => null,
    };

    public static string? OwnBait(StrategyText? seat, string? role)
    {
        if (seat is null || Baits(role) is not { } bait) return null;

        foreach (var block in seat.Blocks)
            foreach (var line in block.Lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith(BaitToken, StringComparison.OrdinalIgnoreCase)) continue;
                if (!trimmed.StartsWith(BaitStart, StringComparison.OrdinalIgnoreCase)) continue;
                if (!trimmed.Contains(bait, StringComparison.OrdinalIgnoreCase)) continue;

                var settled = Settled(trimmed, null);
                if (!string.IsNullOrEmpty(settled)) return settled;
            }

        return null;
    }

    public static IReadOnlyList<string> Lines(
        StrategyBlock block, string? mine, bool? future,
        string? tower = null, string? role = null, string? fluid = null,
        bool baitsClones = true, bool baits = true, string? ownBait = null,
        string? next = null, bool partner = false)
    {
        var built = new List<string>();
        var owned = false;
        var yours = false;

        foreach (var line in block.Lines)
        {
            var trimmed = Settled(line.Trim(), fluid);
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith(BaitToken, StringComparison.OrdinalIgnoreCase))
            {
                if (baits) built.Add(BaitLine(future));
                continue;
            }

            if (trimmed.StartsWith(RememberNew, StringComparison.OrdinalIgnoreCase))
            {
                if (Known(next)) built.Add($"{NewDebuff} {next}");
                continue;
            }

            if (trimmed.StartsWith(Remember, StringComparison.OrdinalIgnoreCase))
            {
                if (Known(mine)) built.Add($"{YourDebuff} {mine}");
                continue;
            }

            trimmed = Resolve(trimmed, tower);

            var owner = Owner(trimmed);
            if (owner is null)
            {
                built.Add(CallWords.Line(trimmed));
                continue;
            }

            owned = true;

            if (mine is null || !string.Equals(owner, mine, StringComparison.OrdinalIgnoreCase))
                continue;

            yours = true;
            built.Add(CallWords.Line(Rest(trimmed)));
        }

        var offTower = false;
        if (owned && !yours
            && (baitsClones ? ownBait ?? NonTower(role) : LastTower(role)) is { } instead)
        {
            built.Insert(0, instead);
            offTower = true;
        }

        var trimmedDown = Once(built);

        var label = offTower ? null : Job(block.Label?.Trim());
        if (!string.IsNullOrEmpty(label)
            && !Repeats(label, trimmedDown.Where(l => !IsDebuffNote(l))))
            Tack(trimmedDown, label);

        if (partner && !offTower) Tack(trimmedDown, WithPartner, glue: " ");

        return trimmedDown;
    }

    public const string WithPartner = "with Partner";

    public static bool SharesWith(StrategyBlock block, StrategyBlock? partner)
    {
        if (partner is null) return false;
        if (!string.Equals(block.Label?.Trim(), partner.Label?.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        var mine = Spots(block);
        var theirs = Spots(partner);

        return mine.Count == 1 && theirs.Count == 1 &&
            string.Equals(mine[0], theirs[0], StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> Spots(StrategyBlock block)
    {
        var spots = new List<string>();

        foreach (var line in block.Lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;
            if (trimmed.StartsWith(BaitToken, StringComparison.OrdinalIgnoreCase)) continue;
            if (trimmed.StartsWith(RememberNew, StringComparison.OrdinalIgnoreCase)) continue;
            if (trimmed.StartsWith(Remember, StringComparison.OrdinalIgnoreCase)) continue;
            if (trimmed.Contains(Arrow)) return [];

            spots.Add(trimmed);
        }

        return spots;
    }

    private static readonly Dictionary<string, string> Jobs =
        new(StringComparer.OrdinalIgnoreCase) { ["Help Stack"] = "Take Stack" };

    public static string? Job(string? label) =>
        label is not null && Jobs.TryGetValue(label, out var said) ? said : label;

    public static void Tack(List<string> lines, string job, string glue = ", ")
    {
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (IsDebuffNote(lines[i]) || IsBaitLine(lines[i])) continue;
            lines[i] = $"{lines[i]}{glue}{job}";
            return;
        }

        lines.Add(job);
    }

    public static bool IsBaitLine(string line) =>
        line == BaitFuture || line == BaitPast || line == BaitUnknown;

    public static List<string> Once(IReadOnlyList<string> lines)
    {
        var kept = new List<string>();

        foreach (var line in lines)
        {
            var covered = false;

            for (var i = 0; i < kept.Count; i++)
            {
                if (kept[i].Contains(line, StringComparison.OrdinalIgnoreCase))
                {
                    covered = true;
                    break;
                }

                if (line.Contains(kept[i], StringComparison.OrdinalIgnoreCase))
                {
                    kept[i] = line;
                    covered = true;
                    break;
                }
            }

            if (!covered) kept.Add(line);
        }

        return kept;
    }

    public static string? Owner(string line)
    {
        if (!line.StartsWith(Owned, StringComparison.OrdinalIgnoreCase)) return null;

        var at = line.IndexOf(Arrow);
        if (at < 0) return null;

        var name = line[Owned.Length..at].Trim();
        return name.Length == 0 ? null : name;
    }

    public static string Rest(string line)
    {
        var at = line.IndexOf(Arrow);
        return at < 0 ? line : line[(at + 1)..].Trim();
    }

    public static StrategyCue Read(
        IReadOnlyList<string> lines, Func<string, string> turn, string? debuff = null)
    {
        if (lines.Count == 0) return StrategyCue.None;

        var display = new StringBuilder();
        var speech = new StringBuilder();

        foreach (var line in lines)
        {
            var turned = turn(line);

            if (display.Length > 0) display.Append(", ");
            display.Append(turned);

            var spoken = SpeechText.Of(turned);
            if (spoken.Length == 0) continue;
            if (speech.Length > 0) speech.Append(". ");
            speech.Append(spoken);
        }

        return new StrategyCue(Bracketed(display.ToString(), debuff), speech.ToString());
    }

    public static string Bracketed(string display, string? debuff)
    {
        if (!Known(debuff) || display.Length == 0) return display;
        if (display.Contains(debuff!, StringComparison.OrdinalIgnoreCase)) return display;

        return $"{display} ({debuff})";
    }
}
