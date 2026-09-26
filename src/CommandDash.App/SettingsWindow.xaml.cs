using System.Windows;
using System.Windows.Controls;
using CommandDash.App.Modules;
using CommandDash.App.Settings;
using CommandDash.Core;

namespace CommandDash.App;

/// <summary>
/// Lets the user customize the order in which modules appear in the
/// sidebar. Saves the new order via <see cref="ModuleOrderSettings"/>.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly List<IModule> _orderedModules;
    private readonly ModuleOrderSettings _settings;

    public SettingsWindow(IReadOnlyList<IModule> orderedModules, ModuleOrderSettings settings)
    {
        InitializeComponent();

        _orderedModules = orderedModules.ToList();
        _settings = settings;

        RefreshList();
    }

    /// <summary>
    /// Raised after the user saves a new order, so the shell can rebuild
    /// the sidebar without requiring an app restart.
    /// </summary>
    public event EventHandler? OrderSaved;

    private void RefreshList()
    {
        ModuleListBox.ItemsSource = null;
        ModuleListBox.ItemsSource = _orderedModules.Select(m => m.DisplayName).ToList();
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        var index = ModuleListBox.SelectedIndex;
        if (index <= 0)
        {
            return;
        }

        (_orderedModules[index - 1], _orderedModules[index]) = (_orderedModules[index], _orderedModules[index - 1]);
        RefreshList();
        ModuleListBox.SelectedIndex = index - 1;
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        var index = ModuleListBox.SelectedIndex;
        if (index < 0 || index >= _orderedModules.Count - 1)
        {
            return;
        }

        (_orderedModules[index + 1], _orderedModules[index]) = (_orderedModules[index], _orderedModules[index + 1]);
        RefreshList();
        ModuleListBox.SelectedIndex = index + 1;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.Save(_orderedModules.Select(m => m.Id).ToList());
        OrderSaved?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
