using FrenRaidTools.Engine.DancingMad;
using FrenRaidTools.Engine.Fru;

namespace FrenRaidTools.Engine;

public static class ControlIds
{
    public static readonly IReadOnlySet<uint> Watched = new HashSet<uint>
    {
        GravenImage.GlowingHandControl,
        Celestriad.TowerControl,
        TeleTrouncing.GazeControl,
        Trines.TrineControl,
        FruApoc.SpinControl,
        FruFulgent.WaveControl,
        FruUtopianSky.MarkCategory,
    };
}
