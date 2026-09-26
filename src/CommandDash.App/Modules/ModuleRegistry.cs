using CommandDash.Core;

namespace CommandDash.App.Modules;

/// <summary>
/// Central registry of every module currently loaded in the shell,
/// regardless of whether it was compiled in-box or loaded from disk.
/// Provides lookup and ordering used by the sidebar and content host.
/// </summary>
public sealed class ModuleRegistry
{
    private readonly List<IModule> _modules = new();

    public IReadOnlyList<IModule> Modules => _modules;

    public void Register(IModule module)
    {
        if (_modules.Any(m => m.Id == module.Id))
        {
            throw new InvalidOperationException($"A module with id '{module.Id}' is already registered.");
        }

        _modules.Add(module);
    }

    public void RegisterRange(IEnumerable<IModule> modules)
    {
        foreach (var module in modules)
        {
            Register(module);
        }
    }

    public IModule? Find(string moduleId) => _modules.FirstOrDefault(m => m.Id == moduleId);

    /// <summary>
    /// Returns all registered modules ordered according to
    /// <paramref name="orderedIds"/> (typically the user's saved sidebar
    /// order). Modules not present in <paramref name="orderedIds"/> are
    /// appended at the end, in their original registration order.
    /// </summary>
    public IReadOnlyList<IModule> InOrder(IReadOnlyList<string> orderedIds)
    {
        var positions = new Dictionary<string, int>();
        for (var i = 0; i < orderedIds.Count; i++)
        {
            positions[orderedIds[i]] = i;
        }

        return _modules
            .Select((module, index) => (module, rank: positions.TryGetValue(module.Id, out var pos) ? pos : orderedIds.Count + index))
            .OrderBy(x => x.rank)
            .Select(x => x.module)
            .ToList();
    }
}
