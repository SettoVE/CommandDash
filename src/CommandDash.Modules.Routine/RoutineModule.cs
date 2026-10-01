using System.IO;
using CommandDash.Core;

namespace CommandDash.Modules.Routine;

/// <summary>
/// Tracks how long it has been since routine tasks were last completed.
/// </summary>
public sealed class RoutineModule : IModule, IModuleWithSettings
{
    private IModuleContext? _context;
    private RoutineStore? _store;

    public string Id => "commanddash.routine";

    public string DisplayName => "Routine";

    public string Description => "Track how long it has been since routine tasks were last completed.";

    public string Category => "General";

    public string Icon => "\uE81C"; // Segoe Fluent Icons: history

    public Version Version { get; } = new Version(1, 0, 0);

    private RoutineStore Store => _store ??= new RoutineStore(
        _context?.DataDirectory
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandDash", "Routine"));

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

    public IModulePage CreatePage() => new RoutineModulePage(Store);

    public ISettingsNode GetSettingsNode()
        => new SettingsNode("commanddash.routine.settings", DisplayName, () => new RoutineSettingsView(Store));
}
