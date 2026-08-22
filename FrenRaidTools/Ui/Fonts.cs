using Dalamud.Interface.ManagedFontAtlas;

namespace FrenRaidTools.Ui;

public sealed class Fonts : IDisposable
{
    private sealed class Slot
    {
        public required IFontHandle Handle { get; init; }
        public required int Px { get; init; }
        public long Used { get; set; }
    }

    private const int MaxSlots = 8;
    private const int RetireFrames = 3;

    private readonly Dictionary<int, Slot> _slots = [];
    private readonly List<(IFontHandle Handle, long Frame)> _retired = [];
    private long _frame;

    public static int Snap(float px) => (int)MathF.Round(Math.Clamp(px, 10f, 96f) / 2f) * 2;

    public void Tick()
    {
        _frame++;
        if (_retired.Count == 0) return;

        for (var i = _retired.Count - 1; i >= 0; i--)
        {
            if (_frame - _retired[i].Frame < RetireFrames) continue;
            Drop(_retired[i].Handle);
            _retired.RemoveAt(i);
        }
    }

    public void Warm(float px) => Handle(Snap(px));

    public IDisposable Push(float px)
    {
        var want = Snap(px);

        if (Handle(want) is { Available: true } ready)
        {
            _lastGood = want;
            return ready.Push();
        }

        if (_lastGood != 0 && _slots.TryGetValue(_lastGood, out var last)
            && last.Handle.Available)
            return last.Handle.Push();

        if (Nearest(want) is { } near) return near.Handle.Push();

        return new Pushed();
    }

    private int _lastGood;

    private sealed class Pushed : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private IFontHandle? Handle(int px)
    {
        if (_slots.TryGetValue(px, out var hit))
        {
            hit.Used = _frame;
            return hit.Handle;
        }

        if (_slots.Count >= MaxSlots) Evict();

        try
        {
            var handle = Service.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(
                e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(px)));
            _slots[px] = new Slot { Handle = handle, Px = px, Used = _frame };
            return handle;
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Overlay font would not build.");
            return null;
        }
    }

    private void Evict()
    {
        var oldest = 0;
        var oldestUsed = long.MaxValue;
        foreach (var (px, slot) in _slots)
            if (slot.Used < oldestUsed) { oldestUsed = slot.Used; oldest = px; }

        if (oldest != 0 && _slots.Remove(oldest, out var dead)) _retired.Add((dead.Handle, _frame));
    }

    private (IFontHandle Handle, int Px)? Nearest(int want)
    {
        IFontHandle? best = null;
        var bestPx = 0;
        var bestGap = int.MaxValue;

        foreach (var slot in _slots.Values)
        {
            if (!slot.Handle.Available) continue;
            var gap = Math.Abs(slot.Px - want);
            if (gap >= bestGap) continue;
            bestGap = gap;
            best = slot.Handle;
            bestPx = slot.Px;
        }

        return best is null ? null : (best, bestPx);
    }

    private static void Drop(IFontHandle handle)
    {
        try
        {
            handle.Dispose();
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Overlay font would not drop.");
        }
    }

    public void Dispose()
    {
        foreach (var slot in _slots.Values) Drop(slot.Handle);
        _slots.Clear();
        foreach (var (handle, _) in _retired) Drop(handle);
        _retired.Clear();
    }
}
