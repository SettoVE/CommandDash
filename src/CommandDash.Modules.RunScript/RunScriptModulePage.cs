using System.Windows;
using System.Windows.Input;
using CommandDash.Core;

namespace CommandDash.Modules.RunScript;

public sealed class RunScriptModulePage : IModulePage
{
    private readonly RunScriptModuleView _view;
    private Window? _hookedWindow;

    internal RunScriptModulePage(RunScriptStore store)
    {
        _view = new RunScriptModuleView(store);
    }

    public object View => _view;

    public void OnNavigatedTo()
    {
        _view.Refresh();
        _hookedWindow ??= Window.GetWindow(_view) ?? Application.Current?.MainWindow;
        if (_hookedWindow is not null)
        {
            _hookedWindow.PreviewKeyDown -= OnWindowPreviewKeyDown;
            _hookedWindow.PreviewKeyDown += OnWindowPreviewKeyDown;
        }
    }

    public void OnNavigatedFrom()
    {
        if (_hookedWindow is not null)
        {
            _hookedWindow.PreviewKeyDown -= OnWindowPreviewKeyDown;
        }
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!e.Handled && _view.HandleKey(e))
        {
            e.Handled = true;
        }
    }
}
