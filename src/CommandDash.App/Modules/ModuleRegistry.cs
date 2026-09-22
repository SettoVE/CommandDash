using CommandDash.Core;

namespace CommandDash.App.Modules;

/// <summary>
/// Central registry of every module currently loaded in the shell,
/// regardless of whether it was compiled in-box or loaded from disk.
/// Provides lookup and grouping used by the sidebar and content host.
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
    /// Modules grouped by <see cref="IModule.Category"/>, in the order
    /// categories were first encountered, for rendering the sidebar.
    /// </summary>
    public IEnumerable<IGrouping<string, IModule>> GroupedByCategory() =>
        _modules.GroupBy(m => m.Category);
}
