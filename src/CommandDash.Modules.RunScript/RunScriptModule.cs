using CommandDash.Core;

namespace CommandDash.Modules.RunScript;

/// <summary>
/// Placeholder built-in module for running user-defined scripts
/// (PowerShell, batch, etc.) from the shell.
/// </summary>
public sealed class RunScriptModule : IModule
{
    private IModuleContext? _context;

    public string Id => "commanddash.runscript";

    public string DisplayName => "Run Script";

    public string Description => "Run and manage custom scripts.";

    public string Category => "General";

    public string Icon => "\uE756"; // Segoe Fluent Icons: script/code glyph placeholder

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

    public IModulePage CreatePage() => new RunScriptModulePage();
}
