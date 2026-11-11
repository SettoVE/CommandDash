using System.Windows;
using System.Windows.Controls;
using CommandDash.Core;
using Microsoft.Win32;

namespace CommandDash.App.Settings;

/// <summary>Built-in "Startup Scripts" node: manage scripts run when the app starts.</summary>
public sealed class StartupScriptsSettingsNode : ISettingsNode
{
    public string Id => "startupscripts";

    public string DisplayName => "Startup Scripts";

    public IReadOnlyList<ISettingsNode> Children { get; } = Array.Empty<ISettingsNode>();

    public object? CreateContent()
    {
        var settings = StartupScriptsSettings.Load();

        var list = new ListBox
        {
            Background = System.Windows.Media.Brushes.Transparent,
            MinHeight = 120,
            Margin = new Thickness(0, 0, 0, 8),
        };
        list.SetResourceReference(Control.ForegroundProperty, "TextPrimaryBrush");
        foreach (var script in settings.Scripts)
        {
            list.Items.Add(script);
        }

        var add = new Button { Content = "Add", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 8, 0) };
        add.Click += (_, _) =>
        {
            var dialog = new OpenFileDialog { Title = "Select startup script", Multiselect = true };
            if (dialog.ShowDialog() != true) return;

            foreach (var file in dialog.FileNames)
            {
                if (!settings.Scripts.Contains(file, StringComparer.OrdinalIgnoreCase))
                {
                    settings.Scripts.Add(file);
                    list.Items.Add(file);
                }
            }

            settings.Save();
        };

        var remove = new Button { Content = "Remove", Padding = new Thickness(10, 4, 10, 4) };
        remove.Click += (_, _) =>
        {
            if (list.SelectedItem is not string selected) return;

            settings.Scripts.Remove(selected);
            list.Items.Remove(selected);
            settings.Save();
        };

        var onlyOnBoot = new CheckBox
        {
            Content = "Run scripts only on initial Windows boot",
            IsChecked = settings.OnlyOnWindowsStartup,
            Margin = new Thickness(0, 0, 0, 8),
        };
        onlyOnBoot.SetResourceReference(Control.ForegroundProperty, "TextPrimaryBrush");
        onlyOnBoot.Click += (_, _) =>
        {
            settings.OnlyOnWindowsStartup = onlyOnBoot.IsChecked == true;
            settings.Save();
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(add);
        buttons.Children.Add(remove);

        var description = new TextBlock
        {
            Text = "Scripts in this list run when CommandDash starts.",
            Margin = new Thickness(0, 0, 0, 8),
        };
        description.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");

        var panel = new StackPanel { Margin = new Thickness(4) };
        panel.Children.Add(new SettingsSectionHeader { Title = "Startup Scripts" });
        panel.Children.Add(description);
        panel.Children.Add(onlyOnBoot);
        panel.Children.Add(list);
        panel.Children.Add(buttons);
        return panel;
    }
}
