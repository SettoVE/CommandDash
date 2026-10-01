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
        IntervalComboBox.ItemsSource = Intervals.Select(i => i.Label).ToList();
        ModuleComboBox.ItemsSource = modules;
        if (modules.Count > 0)
        {
            ModuleComboBox.SelectedIndex = 0;
        }
    }

    private static readonly (string Label, int Seconds)[] Intervals =
    {
        ("15s", 15), ("30s", 30), ("1m", 60), ("5m", 300), ("30m", 1800), ("1h", 3600),
    };

    private bool _loading;

    private IModule? SelectedModule => ModuleComboBox.SelectedItem as IModule;

    private void ModuleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PathTextBox.Text = SelectedModule is { } module ? _settings.GetPath(module.Id) : string.Empty;
        LoadSlideshow();
    }

    private void LoadSlideshow()
    {
        _loading = true;
        var config = SelectedModule is { } module ? _settings.GetSlideshow(module.Id) : new SlideshowConfig();
        SlideshowCheckBox.IsChecked = config.Enabled;
        FolderTextBox.Text = config.Folder;
        var index = Array.FindIndex(Intervals, i => i.Seconds == config.IntervalSeconds);
        IntervalComboBox.SelectedIndex = index < 0 ? 0 : index;
        ShuffleCheckBox.IsChecked = config.Shuffle;
        UpdateSlideshowEnabledState();
        _loading = false;
    }

    private void UpdateSlideshowEnabledState()
    {
        var enabled = SlideshowCheckBox.IsChecked == true;
        SlideshowFolderRow.IsEnabled = enabled;
        SlideshowIntervalRow.IsEnabled = enabled;
    }

    private void SaveSlideshow()
    {
        if (_loading || SelectedModule is not { } module)
        {
            return;
        }

        var index = IntervalComboBox.SelectedIndex < 0 ? 0 : IntervalComboBox.SelectedIndex;
        _settings.SetSlideshow(module.Id, new SlideshowConfig
        {
            Enabled = SlideshowCheckBox.IsChecked == true,
            Folder = FolderTextBox.Text,
            IntervalSeconds = Intervals[index].Seconds,
            Shuffle = ShuffleCheckBox.IsChecked == true,
        });
    }

    private void Slideshow_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading)
        {
            return;
        }

        UpdateSlideshowEnabledState();
        SaveSlideshow();
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Select slideshow folder" };
        if (dialog.ShowDialog() == true)
        {
            FolderTextBox.Text = dialog.FolderName;
            SaveSlideshow();
        }
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
