using CommandDash.Core;

namespace CommandDash.Modules.Tasks;

public sealed class TasksModulePage : IModulePage
{
    private readonly TasksModuleView _view = new();

    public object View => _view;

    public void OnNavigatedTo() { }

    public void OnNavigatedFrom() { }
}
