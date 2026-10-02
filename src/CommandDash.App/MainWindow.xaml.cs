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
    private readonly WindowPlacementSettings _placementSettings = new();
    private string? _currentModuleId;
    private IModulePage? _currentPage;

    public MainWindow()
    {
        InitializeComponent();
        TitleBarTheme.Apply(this);

        _placementSettings.Restore(this);
        Closing += (_, _) => _placementSettings.Save(this);

        _backgroundSettings.BackgroundChanged += (_, moduleId) =>
        {
            if (moduleId == _currentModuleId)
            {
                ApplyBackground(moduleId, restart: true);
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
            var context = new ModuleContext(loadedModule.Module.Id, _logger, _registry.GetWidgets);
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
                ? $"[{shortcutDigit}]  {module.DisplayName}"
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

    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    private readonly System.Windows.Threading.DispatcherTimer _slideshowTimer = new();
    private sealed class SlideshowState
    {
        public int Index { get; set; } = -1;

        public string? File { get; set; }

        public DateTime ChangedAt { get; set; }
    }

    private readonly Dictionary<string, SlideshowState> _slideshowStates = new();
    private string? _shownPath;

    private void StartSlideshow(string moduleId, SlideshowConfig config, bool restart)
    {
        _slideshowTimer.Stop();
        _slideshowTimer.Tick -= SlideshowTimer_Tick;
        _slideshowTimer.Tick += SlideshowTimer_Tick;
        _slideshowTimer.Tag = moduleId;

        var interval = TimeSpan.FromSeconds(Math.Max(1, config.IntervalSeconds));
        var hasState = _slideshowStates.TryGetValue(moduleId, out var state)
            && state.File is not null
            && File.Exists(state.File)
            && string.Equals(
                Path.GetFullPath(Path.GetDirectoryName(state.File) ?? string.Empty),
                Path.GetFullPath(config.Folder),
                StringComparison.OrdinalIgnoreCase);
        if (!hasState)
        {
            _slideshowStates[moduleId] = new SlideshowState();
            AdvanceSlideshow(moduleId, config, restart);
            _slideshowTimer.Interval = interval;
        }
        else
        {
            ShowImage(state!.File!, restart);
            if (restart)
            {
                state.ChangedAt = DateTime.UtcNow;
            }

            var remaining = interval - (DateTime.UtcNow - state.ChangedAt);
            _slideshowTimer.Interval = remaining < TimeSpan.FromMilliseconds(100)
                ? TimeSpan.FromMilliseconds(100)
                : remaining;
        }

        _slideshowTimer.Start();
    }

    private void SlideshowTimer_Tick(object? sender, EventArgs e)
    {
        if (_slideshowTimer.Tag is string moduleId && moduleId == _currentModuleId)
        {
            var config = _backgroundSettings.GetSlideshow(moduleId);
            AdvanceSlideshow(moduleId, config);
            _slideshowTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, config.IntervalSeconds));
        }
    }

    private void AdvanceSlideshow(string moduleId, SlideshowConfig config, bool animate = true)
    {
        if (!_slideshowStates.TryGetValue(moduleId, out var state))
        {
            state = new SlideshowState();
            _slideshowStates[moduleId] = state;
        }
        string[] files;
        try
        {
            files = Directory.EnumerateFiles(config.Folder)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }
        catch (Exception)
        {
            files = Array.Empty<string>();
        }

        if (files.Length == 0)
        {
            ClearBackground();
            return;
        }

        if (config.Shuffle && files.Length > 1)
        {
            int next;
            do
            {
                next = Random.Shared.Next(files.Length);
            }
            while (next == state.Index);
            state.Index = next;
        }
        else
        {
            state.Index = (state.Index + 1) % files.Length;
        }

        state.File = files[state.Index];
        state.ChangedAt = DateTime.UtcNow;
        ShowImage(state.File, animate);
    }

    private void ApplyBackground(string moduleId, bool restart = false)
    {
        var slideshow = _backgroundSettings.GetSlideshow(moduleId);
        if (slideshow.Enabled && !string.IsNullOrWhiteSpace(slideshow.Folder))
        {
            StartSlideshow(moduleId, slideshow, restart);
            return;
        }

        _slideshowTimer.Stop();
        _slideshowStates.Remove(moduleId);
        ShowImage(_backgroundSettings.GetPath(moduleId), animate: restart);
    }

    private void ShowImage(string path, bool animate = true)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            ClearBackground();
            return;
        }

        if (path == _shownPath)
        {
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

            CommitFade();
            _shownPath = path;
            var brush = new System.Windows.Media.ImageBrush(image)
            {
                Stretch = System.Windows.Media.Stretch.UniformToFill,
                AlignmentX = System.Windows.Media.AlignmentX.Center,
                AlignmentY = System.Windows.Media.AlignmentY.Center,
                Opacity = animate ? 0 : 1,
            };

            if (!animate)
            {
                BackgroundFadePanel.Background = null;
                BackgroundPanel.Background = brush;
                return;
            }
            BackgroundFadePanel.Background = brush;
            var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(800));
            fade.Completed += (_, _) =>
            {
                if (ReferenceEquals(BackgroundFadePanel.Background, brush))
                {
                    CommitFade();
                }
            };
            brush.BeginAnimation(System.Windows.Media.Brush.OpacityProperty, fade);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load background image '{path}'.", ex);
            ClearBackground();
        }
    }

    private void CommitFade()
    {
        if (BackgroundFadePanel.Background is System.Windows.Media.ImageBrush top)
        {
            top.BeginAnimation(System.Windows.Media.Brush.OpacityProperty, null);
            top.Opacity = 1;
            BackgroundPanel.Background = top;
            BackgroundFadePanel.Background = null;
        }
    }

    private void ClearBackground()
    {
        _shownPath = null;
        BackgroundFadePanel.Background = null;
        BackgroundPanel.Background = null;
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

        if (e.Key == Key.F1)
        {
            OpenSettings_Click(sender, e);
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
