using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public sealed class DashboardModulePage : IModulePage
{
    private readonly DashboardModuleView _view = new();
    private readonly IReadOnlyList<IWidget> _widgets;

    public DashboardModulePage(IReadOnlyList<IWidget> widgets)
    {
        _widgets = widgets;
        foreach (var widget in widgets)
        {
            _view.AddWidget(widget.DisplayName, widget.PreferredSize, widget.CreateView());
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
