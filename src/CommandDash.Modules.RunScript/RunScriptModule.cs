using CommandDash.Core;

namespace CommandDash.Modules.RunScript;

/// <summary>
/// Placeholder built-in module for running user-defined scripts
/// (PowerShell, batch, etc.) from the shell.
/// </summary>
public sealed class RunScriptModule : ModuleBase
{
    public override string Id => "commanddash.runscript";

    public override string DisplayName => "Run Script";

    public override string Description => "Run and manage custom scripts.";

    public override string Icon => "\uE756"; // Segoe Fluent Icons: script/code glyph placeholder

    public override IModulePage CreatePage() => new RunScriptModulePage();
}
