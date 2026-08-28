using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

internal static class JobSpotRows
{
    public const string Note =
        "Each job's spot on a fill. You go first.";

    public static string Badge(JobSpots spots) =>
        spots.Any ? $"{spots.Count} set" : "auto";

    public static bool List(JobSpots spots, string id)
    {
        var changed = false;

        foreach (var job in JobSpots.Jobs)
            changed |= Row(spots, job, id);

        return changed;
    }

    public static bool Row(JobSpots spots, string job, string id)
    {
        var (first, second) = Slots.Pair(JobKinds.Kind(job));
        if (first < 0) return false;

        var picked = spots.SpotOf(job);
        var names = new[] { Slots.Names[first], Slots.Names[second] };

        Widgets.RowBegin(job, Hint(job, picked, first, second),
            Widgets.ButtonWidth("Auto", names[0], names[1]) + Theme.S(2f),
            sub: true, controlHeight: ImGui.GetTextLineHeight(),
            id: id + job, edgeColor: JobLook.Color(job));

        var changed = false;

        Widgets.SegmentBegin();

        if (Widgets.Segment("Auto", picked < 0))
        {
            spots.Unset(job);
            changed = true;
        }

        for (var i = 0; i < names.Length; i++)
        {
            var slot = i == 0 ? first : second;
            ImGui.SameLine();
            if (!Widgets.Segment(names[i], picked == slot)) continue;
            spots.Set(job, slot);
            changed = true;
        }

        Widgets.SegmentEnd();
        Widgets.RowEnd();
        return changed;
    }

    public static bool ResetRow(JobSpots spots, string id)
    {
        if (!spots.Any) return false;

        Widgets.RowBegin("Back to auto", "Drop every pick", Widgets.ButtonWidth("Reset"), id: id);
        var hit = Widgets.GhostButton("Reset");
        if (hit) spots.Clear();
        Widgets.RowEnd();

        return hit;
    }

    public static string Hint(string job, int picked, int first, int second)
    {
        if (picked >= 0) return $"Always {Slots.Names[picked]}";

        var wantsFirst = Slots.Prefers(first, job);
        var wantsSecond = Slots.Prefers(second, job);

        if (wantsFirst && !wantsSecond) return $"Auto picks {Slots.Names[first]}";
        if (wantsSecond && !wantsFirst) return $"Auto picks {Slots.Names[second]}";

        return $"Auto picks {Slots.Names[first]} or {Slots.Names[second]}";
    }
}
