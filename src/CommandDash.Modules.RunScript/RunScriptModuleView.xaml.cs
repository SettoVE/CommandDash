using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommandDash.Modules.RunScript;

public partial class RunScriptModuleView : UserControl
{
    private readonly RunScriptStore _store;
    private readonly ObservableCollection<ScriptItem> _items = new();

    internal RunScriptModuleView(RunScriptStore store)
    {
        _store = store;
        InitializeComponent();
        ScriptList.ItemsSource = _items;
    }

    internal void Refresh()
    {
        var bindings = _store.LoadBindings();
        _items.Clear();
        foreach (var path in _store.ScanScripts())
        {
            var item = new ScriptItem(path);
            if (bindings.TryGetValue(item.Name, out var hotkey))
            {
                item.Hotkey = hotkey;
            }

            _items.Add(item);
        }

        EmptyText.Text = $"No scripts found in {_store.ScriptsFolder}";
        EmptyText.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Runs the script bound to the pressed key, returning true when one was launched.</summary>
    internal bool HandleKey(KeyEventArgs e)
    {
        if (ScriptHotkey.FromEvent(e) is not { } hotkey)
        {
            return false;
        }

        var item = _items.FirstOrDefault(i => i.Hotkey == hotkey);
        if (item is null)
        {
            return false;
        }

        _ = RunAsync(item);
        return true;
    }

    private async Task RunAsync(ScriptItem item)
    {
        if (item.IsRunning)
        {
            return;
        }

        item.IsRunning = true;
        item.Status = "Running...";
        var (exitCode, output) = await ScriptRunner.RunAsync(item.FullPath, _store);
        item.IsRunning = false;
        var header = exitCode is { } code ? $"Exit code: {code}" : "Failed to run";
        item.Status = output.Length == 0 ? header : $"{header}{Environment.NewLine}{output}";
    }

    private static ScriptItem? ItemOf(object sender) => (sender as FrameworkElement)?.DataContext as ScriptItem;

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var folder = _store.ScriptsFolder;
        if (!Directory.Exists(folder))
        {
            return;
        }

        Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {
        if (ItemOf(sender) is { } item)
        {
            _ = RunAsync(item);
        }
    }

    private void Bind_Click(object sender, RoutedEventArgs e)
    {
        if (ItemOf(sender) is not { } item)
        {
            return;
        }

        var window = new BindWindow(Window.GetWindow(this), item.Name, item.Hotkey,
            hotkey => !_items.Any(i => i != item && i.Hotkey == hotkey));
        window.ShowDialog();

        switch (window.Outcome)
        {
            case BindOutcome.Ok:
                item.Hotkey = window.Result;
                break;
            case BindOutcome.Clear:
                item.Hotkey = null;
                break;
            default:
                return;
        }

        _store.SaveBindings(_items.Where(i => i.Hotkey is not null)
            .ToDictionary(i => i.Name, i => i.Hotkey!.Value, StringComparer.OrdinalIgnoreCase));
    }
}
