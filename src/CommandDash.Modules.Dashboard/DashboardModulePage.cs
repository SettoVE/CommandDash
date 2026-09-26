using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public sealed class DashboardModulePage : IModulePage
{
    private readonly DashboardModuleView _view = new();

    public object View => _view;

    public void OnNavigatedTo() { }

    public void OnNavigatedFrom() { }
}
