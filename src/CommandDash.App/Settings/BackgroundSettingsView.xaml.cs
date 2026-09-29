using System.Windows;
using System.Windows.Controls;
using CommandDash.Core;
using Microsoft.Win32;

namespace CommandDash.App.Settings;

/// <summary>
/// Lets the user pick a custom background image for each loaded module.
/// </summary>
public partial class BackgroundSettingsView : UserControl
{
    private readonly ModuleBackgroundSettings _settings;

    public BackgroundSettingsView(IReadOnlyList<IModule> modules, ModuleBackgroundSettings settings)
    {
        InitializeComponent();

        _settings = settings;
        ModuleComboBox.ItemsSource = modules;
        if (modules.Count > 0)
        {
            ModuleComboBox.SelectedIndex = 0;
        }
    }

    private IModule? SelectedModule => ModuleComboBox.SelectedItem as IModule;

    private void ModuleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PathTextBox.Text = SelectedModule is { } module ? _settings.GetPath(module.Id) : string.Empty;
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedModule is not { } module)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Select background image",
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
        };

        if (dialog.ShowDialog() == true)
        {
            _settings.SetPath(module.Id, dialog.FileName);
            PathTextBox.Text = dialog.FileName;
        }
    }

    private void Default_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedModule is not { } module)
        {
            return;
        }

        _settings.SetPath(module.Id, null);
        PathTextBox.Text = string.Empty;
    }
}
