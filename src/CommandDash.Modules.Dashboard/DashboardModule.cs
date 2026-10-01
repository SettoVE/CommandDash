using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Built-in Dashboard module. The default landing page, hosting a grid of
/// widgets (see IWidget/IWidgetProvider).
/// </summary>
public sealed class DashboardModule : ModuleBase, IWidgetProvider
{
    private IReadOnlyList<IWidget>? _widgets;

    public override string Id => "commanddash.home";

    public override string DisplayName => "Dashboard";

    public override string Description => "At-a-glance overview and customizable widget dashboard.";

    public override string Icon => "\uE80F"; // Segoe Fluent Icons: home glyph placeholder

    public IReadOnlyList<IWidget> CreateWidgets() => _widgets ??= new IWidget[]
        {
            new ClockWidget(),
            new NetworkWidget(),
            new CpuWidget(),
            new GpuWidget(),
            new MemoryWidget(),
        };

    public override IModulePage CreatePage() => new DashboardModulePage(Context?.GetAvailableWidgets() ?? CreateWidgets());
}
