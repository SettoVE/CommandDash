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
    private readonly ModuleBackgroundSettings _backgroundSettings = new();
    private string? _currentModuleId;
    private IModulePage? _currentPage;

    public MainWindow()
    {
        InitializeComponent();

        _backgroundSettings.BackgroundChanged += (_, moduleId) =>
        {
            if (moduleId == _currentModuleId)
            {
                ApplyBackground(moduleId);
            }
        };

        LoadModules();
        BuildSidebar();
        NavigateToDefaultModule();
    }

    private void LoadModules()
    {
        var modulesDirectory = Path.Combine(AppContext.BaseDirectory, "Modules");
        var loader = new ModuleLoader(_logger);
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
            var shortcutDigit = i < 10 ? ((i + 1) % 10).ToString() : null;
            var label = shortcutDigit is not null
                ? $"[{shortcutDigit}]    {module.DisplayName}"
                : module.DisplayName;

            var navItem = new RadioButton
            {
                Content = label,
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
        // Prefer the built-in Dashboard module; otherwise fall back to the
        // first registered module, if any.
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

        if (ReferenceEquals(_currentPage, page))
        {
            return;
        }

        _currentPage?.OnNavigatedFrom();
        _currentPage = page;
        _currentModuleId = moduleId;
        ApplyBackground(moduleId);

        //PageTitle.Text = module.DisplayName;
        ModuleContentHost.Content = page.View;
        page.OnNavigatedTo();

        var navItem = NavPanel.Children.OfType<RadioButton>().FirstOrDefault(r => (string)r.Tag == moduleId);
        if (navItem is not null && navItem.IsChecked != true)
        {
            navItem.IsChecked = true;
        }
    }

    private void ApplyBackground(string moduleId)
    {
        var path = _backgroundSettings.GetPath(moduleId);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            BackgroundPanel.Background = null;
            return;
        }

        try
        {
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            BackgroundPanel.Background = new System.Windows.Media.ImageBrush(image)
            {
                Stretch = System.Windows.Media.Stretch.UniformToFill,
                AlignmentX = System.Windows.Media.AlignmentX.Center,
                AlignmentY = System.Windows.Media.AlignmentY.Center,
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load background image '{path}'.", ex);
            BackgroundPanel.Background = null;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _currentPage?.OnNavigatedFrom();
        _currentPage = null;

        foreach (var module in _registry.Modules)
        {
            module.OnUnloaded();
        }

        base.OnClosed(e);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
            return;
        }

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

        var modulesNode = new ModulesSettingsNode(orderedModules, _orderSettings, _backgroundSettings);
        modulesNode.OrderSaved += (_, _) =>
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

        var rootNodes = new List<ISettingsNode>
        {
            new GeneralSettingsNode(),
            modulesNode,
        };

        var settingsWindow = new SettingsWindow(rootNodes)
        {
            Owner = this,
        };
        settingsWindow.ShowDialog();
    }
}
