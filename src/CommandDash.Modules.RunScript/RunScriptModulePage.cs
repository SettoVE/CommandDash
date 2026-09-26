using CommandDash.Core;

namespace CommandDash.Modules.RunScript;

public sealed class RunScriptModulePage : IModulePage
{
    private readonly RunScriptModuleView _view = new();

    public object View => _view;

    public void OnNavigatedTo() { }

    public void OnNavigatedFrom() { }
}
