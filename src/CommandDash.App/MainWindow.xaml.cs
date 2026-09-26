using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommandDash.App.Modules;
using CommandDash.App.Settings;
using CommandDash.Core;

namespace CommandDash.App;

public partial class MainWindow : Window
{
    // 1-9 map to the 1st-9th module; 0 maps to the 10th. Only the first 10
    // modules get a number-key shortcut.
    private static readonly Key[] NumberKeysInOrder =
    {
        Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0,
    };

    private static readonly Key[] NumPadKeysInOrder =
    {
        Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5,
        Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.NumPad0,
    };

    private readonly ModuleRegistry _registry = new();
    private readonly Dictionary<string, IModulePage> _activePages = new();
    private readonly IModuleLogger _logger = new DebugModuleLogger();
    private readonly ModuleOrderSettings _orderSettings = new();

    public MainWindow()
    {
        InitializeComponent();

        LoadModules();
        BuildSidebar();
        NavigateToDefaultModule();
    }

    private void LoadModules()
    {
        // Built-in modules compiled directly into the app (e.g. Home) will be
        // registered here once implemented.

        var modulesDirectory = Path.Combine(AppContext.BaseDirectory, "Modules");
        var loader = new ModuleLoader();
        var discovered = loader.LoadFrom(modulesDirectory);

        foreach (var loadedModule in discovered)
        {
            var context = new ModuleContext(loadedModule.Module.Id, _logger);
            loadedModule.Module.OnLoaded(context);
            _registry.Register(loadedModule.Module);
        }
    }

    private void BuildSidebar()
    {
        NavPanel.Children.Clear();

        var savedOrder = _orderSettings.Load();
        var orderedModules = _registry.InOrder(savedOrder);

        for (var i = 0; i < orderedModules.Count; i++)
        {
            var module = orderedModules[i];
            var shortcutLabel = i < 10 ? ((i + 1) % 10).ToString() : null;

            var content = new StackPanel { Orientation = Orientation.Horizontal };
            if (shortcutLabel is not null)
            {
                content.Children.Add(new Border
                {
                    Width = 20,
                    Height = 20,
                    CornerRadius = new CornerRadius(4),
                    Background = (System.Windows.Media.Brush)FindResource("BorderBrush2"),
                    Margin = new Thickness(0, 0, 10, 0),
                    Child = new TextBlock
                    {
                        Text = shortcutLabel,
                        FontSize = 11,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush"),
                    },
                });
            }
            content.Children.Add(new TextBlock
            {
                Text = module.DisplayName,
                VerticalAlignment = VerticalAlignment.Center,
            });

            var navItem = new RadioButton
            {
                Content = content,
                GroupName = "Nav",
                Style = (Style)FindResource("NavItemStyle"),
                Tag = module.Id,
            };
            navItem.Checked += (_, _) => NavigateToModule(module.Id);
            NavPanel.Children.Add(navItem);
        }
    }

    private void NavigateToDefaultModule()
    {
        // Prefer a built-in "Home" module once it exists; otherwise fall back
        // to the first registered module, if any.
        var defaultModule = _registry.Find("commanddash.home") ?? _registry.Modules.FirstOrDefault();
        if (defaultModule is null)
        {
            return;
        }

        var navItem = NavPanel.Children.OfType<RadioButton>().FirstOrDefault(r => (string)r.Tag == defaultModule.Id);
        if (navItem is not null)
        {
            navItem.IsChecked = true; // triggers NavigateToModule via Checked handler
        }
        else
        {
            NavigateToModule(defaultModule.Id);
        }
    }

    private void NavigateToModule(string moduleId)
    {
        var module = _registry.Find(moduleId);
        if (module is null)
        {
            return;
        }

        if (!_activePages.TryGetValue(moduleId, out var page))
        {
            page = module.CreatePage();
            _activePages[moduleId] = page;
        }

        foreach (var existing in _activePages.Values)
        {
            existing.OnNavigatedFrom();
        }

        PageTitle.Text = module.DisplayName;
        ModuleContentHost.Content = page.View;
        page.OnNavigatedTo();

        var navItem = NavPanel.Children.OfType<RadioButton>().FirstOrDefault(r => (string)r.Tag == moduleId);
        if (navItem is not null && navItem.IsChecked != true)
        {
            navItem.IsChecked = true;
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var index = Array.IndexOf(NumberKeysInOrder, e.Key);
        if (index < 0)
        {
            index = Array.IndexOf(NumPadKeysInOrder, e.Key);
        }

        if (index < 0)
        {
            return;
        }

        var navItems = NavPanel.Children.OfType<RadioButton>().ToList();
        if (index >= navItems.Count)
        {
            return;
        }

        var moduleId = (string)navItems[index].Tag;
        NavigateToModule(moduleId);
        e.Handled = true;
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var savedOrder = _orderSettings.Load();
        var orderedModules = _registry.InOrder(savedOrder);

        var settingsWindow = new SettingsWindow(orderedModules, _orderSettings)
        {
            Owner = this,
        };
        settingsWindow.OrderSaved += (_, _) =>
        {
            var previouslySelectedId = NavPanel.Children.OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true)?.Tag as string;

            BuildSidebar();

            if (previouslySelectedId is not null)
            {
                var navItem = NavPanel.Children.OfType<RadioButton>()
                    .FirstOrDefault(r => (string)r.Tag == previouslySelectedId);
                if (navItem is not null)
                {
                    navItem.IsChecked = true;
                }
            }
        };
        settingsWindow.ShowDialog();
    }
}
