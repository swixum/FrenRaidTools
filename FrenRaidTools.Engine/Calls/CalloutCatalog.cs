using System.Reflection;

namespace FrenRaidTools.Engine;

public sealed record CatalogEntry(string Group, string Key, Callout Call);

public sealed class DuplicateCalloutKey(string key, string group)
    : Exception($"Two callouts in '{group}' share the key '{key}'.");

public sealed class CalloutCatalog
{
    private readonly List<CatalogEntry> _entries = [];
    private readonly Dictionary<string, CatalogEntry> _byKey = [];

    private readonly Dictionary<Callout, Callout> _keyed =
        new(ReferenceEqualityComparer.Instance);

    private readonly Dictionary<string, Callout> _byDescription = new(StringComparer.Ordinal);
    private readonly HashSet<string> _shared = new(StringComparer.Ordinal);

    public IReadOnlyList<CatalogEntry> Entries => _entries;

    public int Count => _entries.Count;

    public IEnumerable<string> Groups => _entries.Select(e => e.Group).Distinct();

    public CatalogEntry? Find(string key) => _byKey.GetValueOrDefault(key);

    public IEnumerable<CatalogEntry> InGroup(string group) =>
        _entries.Where(e => e.Group == group);

    public IEnumerable<(string Group, Callout Call)> ForUi() =>
        _entries.Select(e => (e.Group, e.Call));

    public Callout Add(string group, string key, Callout call) => Add(group, key, call, 0, "");

    public Callout Add(string group, string key, Callout call, int phase, string mechanic)
    {
        if (_byKey.ContainsKey(key)) throw new DuplicateCalloutKey(key, group);

        var keyed = call with
        {
            Key = key,
            Phase = call.Phase != 0 ? call.Phase : phase,
            Mechanic = call.Mechanic.Length > 0 ? call.Mechanic : mechanic,
        };
        var entry = new CatalogEntry(group, key, keyed);
        _entries.Add(entry);
        _byKey[key] = entry;
        _keyed[call] = keyed;

        if (!_byDescription.TryAdd(keyed.Description, keyed)) _shared.Add(keyed.Description);

        return keyed;
    }

    public Callout WithKey(Callout call)
    {
        if (_keyed.TryGetValue(call, out var byReference)) return byReference;
        if (call.Key.Length > 0) return call;

        if (_shared.Contains(call.Description)) return call;
        if (!_byDescription.TryGetValue(call.Description, out var named)) return call;

        return call with
        {
            Key = named.Key,
            Phase = call.Phase != 0 ? call.Phase : named.Phase,
            Mechanic = call.Mechanic.Length > 0 ? call.Mechanic : named.Mechanic,
        };
    }

    public CallSink WithKey(CallSink inner) =>
        (callout, on, args) => inner(WithKey(callout), on, args);

    public void Register(string group, object holder) => Register(group, holder, 0, "");

    public void Register(string group, object holder, int phase, string mechanic)
    {
        foreach (var (name, call) in Declared(holder))
            Add(group, name, call, phase, mechanic);
    }

    public IEnumerable<CatalogEntry> InPhase(int phase) =>
        _entries.Where(e => e.Call.Phase == phase);

    public IEnumerable<int> PhasesPresent =>
        _entries.Select(e => e.Call.Phase).Where(p => p > 0).Distinct().Order();

    public static IEnumerable<(string Name, Callout Call)> Declared(object holder)
    {
        var type = holder.GetType();

        const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var fields = type.GetFields(Flags)
            .Where(f => f.FieldType == typeof(Callout))
            .OrderBy(f => f.MetadataToken);

        foreach (var field in fields)
            if (field.GetValue(holder) is Callout call)
                yield return (field.Name, call);
    }
}
