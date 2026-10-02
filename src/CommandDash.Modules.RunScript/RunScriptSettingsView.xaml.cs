using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CommandDash.Modules.RunScript;

public partial class RunScriptSettingsView : UserControl
{
    private readonly RunScriptStore _store;

    internal RunScriptSettingsView(RunScriptStore store)
    {
        _store = store;
        InitializeComponent();
        FolderTextBox.Text = store.ScriptsFolder;
        PythonTextBox.Text = store.PythonPath;
        PowerShellTextBox.Text = store.PowerShellPath;
        RubyTextBox.Text = store.RubyPath;
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Select scripts folder" };
        if (dialog.ShowDialog() == true)
        {
            _store.SetScriptsFolder(dialog.FolderName);
            FolderTextBox.Text = _store.ScriptsFolder;
        }
    }

    private void DefaultFolder_Click(object sender, RoutedEventArgs e)
    {
        _store.SetScriptsFolder(null);
        FolderTextBox.Text = _store.ScriptsFolder;
    }

    private TextBox? BoxFor(object sender) => (sender as FrameworkElement)?.Tag switch
    {
        "Python" => PythonTextBox,
        "PowerShell" => PowerShellTextBox,
        "Ruby" => RubyTextBox,
        _ => null,
    };

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        if (BoxFor(sender) is not { } box)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Select interpreter",
            Filter = "Executables|*.exe|All files|*.*",
        };

        if (dialog.ShowDialog() == true)
        {
            box.Text = dialog.FileName;
            SaveInterpreters();
        }
    }

    private void ClearInterpreter_Click(object sender, RoutedEventArgs e)
    {
        if (BoxFor(sender) is { } box)
        {
            box.Text = string.Empty;
            SaveInterpreters();
        }
    }

    private void Interpreter_Changed(object sender, RoutedEventArgs e) => SaveInterpreters();

    private void SaveInterpreters()
        => _store.SetInterpreters(PythonTextBox.Text.Trim(), PowerShellTextBox.Text.Trim(), RubyTextBox.Text.Trim());
}
