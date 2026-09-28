using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Built-in Dashboard module. The default landing page, hosting a grid of
/// widgets (see IWidget/IWidgetProvider).
/// </summary>
public sealed class DashboardModule : IModule, IWidgetProvider
{
    private IModuleContext? _context;
    private IReadOnlyList<IWidget>? _widgets;

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

    public IReadOnlyList<IWidget> CreateWidgets() => _widgets ??= new IWidget[] { new ClockWidget() };

    public IModulePage CreatePage() => new DashboardModulePage(CreateWidgets());
}
