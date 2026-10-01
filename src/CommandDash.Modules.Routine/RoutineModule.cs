using System.IO;
using CommandDash.Core;

namespace CommandDash.Modules.Routine;

/// <summary>
/// Tracks how long it has been since routine tasks were last completed.
/// </summary>
public sealed class RoutineModule : ModuleBase, IModuleWithSettings
{
    private RoutineStore? _store;

    public override string Id => "commanddash.routine";

    public override string DisplayName => "Routine";

    public override string Description => "Track how long it has been since routine tasks were last completed.";

    public override string Icon => "\uE81C"; // Segoe Fluent Icons: history

    private RoutineStore Store => _store ??= new RoutineStore(
        Context?.DataDirectory
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandDash", "Routine"));

    public override IModulePage CreatePage() => new RoutineModulePage(Store);

    public ISettingsNode GetSettingsNode()
        => new SettingsNode("commanddash.routine.settings", DisplayName, () => new RoutineSettingsView(Store));
}
