using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Placeholder built-in Dashboard module. Intended to become the default
/// landing page hosting a customizable widget grid (see IWidget/IWidgetProvider).
/// </summary>
public sealed class DashboardModule : IModule
{
    private IModuleContext? _context;

    public string Id => "commanddash.home";

    public string DisplayName => "Dashboard";

    public string Description => "At-a-glance overview and customizable widget dashboard.";

    public string Category => "General";

    public string Icon => "\uE80F"; // Segoe Fluent Icons: home glyph placeholder

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

    public IModulePage CreatePage() => new DashboardModulePage();
}
