using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommandDash.App.Modules;
using CommandDash.App.Settings;
using CommandDash.Core;

namespace CommandDash.App;

public partial class MainWindow : Window
{
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

        foreach (var module in orderedModules)
        {
            var navItem = new RadioButton
            {
                Content = module.DisplayName,
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
