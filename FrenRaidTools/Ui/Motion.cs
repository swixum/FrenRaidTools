namespace FrenRaidTools.Ui;

internal static class Motion
{
    private const float Beats = 7.4f;

    public static float Pulse(double now, float low, float high) =>
        low + (high - low) * (0.5f + 0.5f * MathF.Sin((float)now * Beats));
}
