using System.IO;
using CommandDash.Core;

namespace CommandDash.Modules.RunScript;

/// <summary>
/// Runs scripts (Python, PowerShell, Ruby, batch) from a configurable folder.
/// </summary>
public sealed class RunScriptModule : ModuleBase, IModuleWithSettings
{
    private RunScriptStore? _store;

    public override string Id => "commanddash.runscript";

    public override string DisplayName => "Run Script";

    public override string Description => "Run and manage custom scripts.";

    public override string Icon => "\uE756"; // Segoe Fluent Icons: script/code glyph placeholder

    private RunScriptStore Store => _store ??= new RunScriptStore(
        Context?.DataDirectory
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandDash", "RunScript"));

    public override IModulePage CreatePage() => new RunScriptModulePage(Store);

    public ISettingsNode GetSettingsNode()
        => new SettingsNode("commanddash.runscript.settings", DisplayName, () => new RunScriptSettingsView(Store));
}
