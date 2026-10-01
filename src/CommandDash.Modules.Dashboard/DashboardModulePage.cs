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
            _view.AddWidget(widget.DisplayName, widget.CreateView(), GetPlacement(i));
        }
    }

    // Default flow layout: fill columns left to right, then wrap to the next row.
    // Replace with a configurable lookup (e.g. by widget id) for custom placement.
    private static WidgetPlacement GetPlacement(int index) =>
        new(index / DashboardModuleView.ColumnCount, index % DashboardModuleView.ColumnCount);

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
