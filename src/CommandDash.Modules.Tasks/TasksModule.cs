using CommandDash.Core;

namespace CommandDash.Modules.Tasks;

/// <summary>
/// Placeholder built-in module for scheduled/background task management.
/// </summary>
public sealed class TasksModule : IModule
{
    private IModuleContext? _context;

    public string Id => "commanddash.tasks";

    public string DisplayName => "Tasks";

    public string Description => "View and manage scheduled or background tasks.";

    public string Category => "General";

    public string Icon => "\uE73A"; // Segoe Fluent Icons: list/tasks glyph placeholder

    public Version Version { get; } = new Version(1, 0, 0);

    public void OnLoaded(IModuleContext context)
    {
        _context = context;
        _context.Logger.Info($"{DisplayName} loaded.");
    }

    public void OnUnloaded()
    {
        _context?.Logger.Info($"{DisplayName} unloaded.");
        _context = null;
    }

    public IModulePage CreatePage() => new TasksModulePage();
}
