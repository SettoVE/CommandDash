using CommandDash.Core;

namespace CommandDash.Modules.Routine;

public sealed class RoutineModulePage : IModulePage
{
    private readonly RoutineModuleView _view;

    public RoutineModulePage(RoutineStore store)
    {
        _view = new RoutineModuleView(store);
    }

    public object View => _view;

    public void OnNavigatedTo() { }

    public void OnNavigatedFrom() { }
}
