using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public sealed class DashboardModulePage : IModulePage
{
    private readonly DashboardModuleView _view = new();
    private readonly IReadOnlyList<IWidget> _widgets;

    public DashboardModulePage(IReadOnlyList<IWidget> widgets)
    {
        _widgets = widgets;
        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            _view.AddWidget(widget.DisplayName, widget.CreateView(), GetPlacement(widget, i));
        }
    }

    // Explicit placements by widget id (row, column). Widgets not listed fall back to
    // left-to-right flow after the last explicitly placed row.
    private static readonly Dictionary<string, WidgetPlacement> Placements = new()
    {
        ["commanddash.home.clock"] = new(0, 0),
        ["commanddash.home.cpu"] = new(1, 0),
        ["commanddash.home.gpu"] = new(1, 1),
        ["commanddash.home.memory"] = new(2, 0),
        ["commanddash.home.network"] = new(2, 1),
    };

    private static WidgetPlacement GetPlacement(IWidget widget, int index) =>
        Placements.TryGetValue(widget.Id, out var placement)
            ? placement
            : new(Placements.Count > 0 ? 3 + index / DashboardModuleView.ColumnCount : index / DashboardModuleView.ColumnCount,
                  index % DashboardModuleView.ColumnCount);

    public object View => _view;

    public void OnNavigatedTo()
    {
        foreach (var widget in _widgets) widget.OnActivated();
    }

    public void OnNavigatedFrom()
    {
        foreach (var widget in _widgets) widget.OnDeactivated();
    }
}
