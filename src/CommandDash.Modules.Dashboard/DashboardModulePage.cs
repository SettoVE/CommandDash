using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public sealed class DashboardModulePage : IModulePage
{
    private readonly DashboardModuleView _view = new();
    private readonly IReadOnlyList<IWidget> _widgets;

    internal DashboardModulePage(IReadOnlyList<IWidget> widgets, DashboardSettings settings)
    {
        _widgets = widgets;
        _view.ApplyAlignment(settings.HorizontalAlignment, settings.VerticalAlignment);
        settings.Changed += (_, _) => _view.Dispatcher.Invoke(
            () => _view.ApplyAlignment(settings.HorizontalAlignment, settings.VerticalAlignment));

        // Widgets that request a position keep it; the rest flow left-to-right
        // into rows below the last explicitly placed row.
        var positions = widgets.Select(w => (w as IGridPositionedWidget)?.DefaultPosition).ToList();
        var flowStartRow = positions.Where(p => p.HasValue).Select(p => p!.Value.Row + p.Value.RowSpan).DefaultIfEmpty(0).Max();

        var flowIndex = 0;
        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            var position = positions[i] ?? new WidgetGridPosition(
                flowStartRow + flowIndex / DashboardModuleView.ColumnCount,
                flowIndex++ % DashboardModuleView.ColumnCount);
            _view.AddWidget(widget.DisplayName, widget.CreateView(), position, widget.Subtitle);
        }
    }

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
