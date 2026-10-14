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
    private GlobalHotkey? _hotkey;
    private string? _currentModuleId;
    private IModulePage? _currentPage;

    private void TryApplyLoadingBackground()
    {
        try
        {
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/Assets/LoadingBackground.img", UriKind.Absolute);
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            LoadingOverlay.Background = new System.Windows.Media.ImageBrush(image)
            {
                Stretch = System.Windows.Media.Stretch.UniformToFill,
            };
        }
        catch
        {
            // Missing or invalid image: keep the default background.
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        TitleBarTheme.Apply(this);
        TryApplyLoadingBackground();
        SetUpTrayIcon();

        var config = AppConfig.Load();
        Title = config.Title;
        Width = Math.Clamp(config.Width, MinWidth, SystemParameters.VirtualScreenWidth);
        Height = Math.Clamp(config.Height, MinHeight, SystemParameters.VirtualScreenHeight);
        if (config.Left is { } left && config.Top is { } top && IsOnScreen(new Rect(left, top, Width, Height)))
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left;
            Top = top;
        }

        Closing += (_, e) =>
        {
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            AppConfig.SaveWindowBounds(bounds.Left, bounds.Top, bounds.Width, bounds.Height);

            if (!_exiting)
            {
                e.Cancel = true;
                Hide();
            }
        };

        IsVisibleChanged += (_, _) => _trayIcon.Visible = !IsVisible && !_exiting;

        _backgroundSettings.BackgroundChanged += async (_, moduleId) =>
        {
            await PreloadBackgroundAsync(moduleId, warmUp: false);
            if (moduleId == _currentModuleId)
            {
                ApplyBackground(moduleId, restart: true);
            }
        };

        // Defer heavy work until the window (with the loading overlay) has been rendered.
        Loaded += async (_, _) => await InitializeOnceAsync();
    }

    private bool _initStarted;
    private bool _exiting;
    private readonly System.Windows.Forms.NotifyIcon _trayIcon = new();

    private Task InitializeOnceAsync()
    {
        if (_initStarted)
        {
            return Task.CompletedTask;
        }

        _initStarted = true;
        return InitializeAsync();
    }

    private void SetUpTrayIcon()
    {
        System.Drawing.Icon? icon = null;
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "app.ico");
            if (File.Exists(path))
            {
                icon = new System.Drawing.Icon(path);
            }
            else if (Environment.ProcessPath is { } exe)
            {
                icon = System.Drawing.Icon.ExtractAssociatedIcon(exe);
            }
        }
        catch
        {
        }

        _trayIcon.Icon = icon ?? System.Drawing.SystemIcons.Application;
        _trayIcon.Text = "CommandDash";
        _trayIcon.MouseDoubleClick += (_, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _hotkey?.Bring();
            }
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());
        TrayMenuTheme.Apply(menu);
        _trayIcon.ContextMenuStrip = menu;
    }

    private void ExitApplication()
    {
        _exiting = true;
        _trayIcon.Visible = false;
        Close();
        Application.Current.Shutdown();
    }

    public void ShowFromTray() => _hotkey?.Bring();

    public void StartInTray()
    {
        // Create the native window (registers the hotkey) without showing it.
        new System.Windows.Interop.WindowInteropHelper(this).EnsureHandle();
        _trayIcon.Visible = true;
        _ = InitializeOnceAsync();
    }

    private void ReportProgress(string status, int? step = null, int? total = null)
    {
        LoadingStatus.Text = status;
        if (step is { } s && total is { } t)
        {
            LoadingProgress.IsIndeterminate = false;
            LoadingProgress.Maximum = t;
            LoadingProgress.Value = s;
        }
    }

    private Task RenderAsync() =>
        Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Background).Task;

    private async Task InitializeAsync()
    {
        ReportProgress("Discovering modules…");
        var modulesDirectory = Path.Combine(AppContext.BaseDirectory, "modules");
        var discovered = await Task.Run(() => new ModuleLoader(_logger).LoadFrom(modulesDirectory));

        var count = discovered.Count;
        var total = (count * 2) + 2;
        for (var i = 0; i < count; i++)
        {
            var loadedModule = discovered[i];
            ReportProgress($"Initializing {loadedModule.Module.DisplayName}…", i, total);
            await RenderAsync();

            var context = new ModuleContext(loadedModule.Module.Id, _logger, _registry.GetWidgets);
            loadedModule.Module.OnLoaded(context);
            _registry.Register(loadedModule.Module);
        }

        ReportProgress("Building navigation…", count, total);
        await RenderAsync();
        BuildSidebar();

        var modules = _registry.Modules.ToList();
        for (var i = 0; i < modules.Count; i++)
        {
            var module = modules[i];
            ReportProgress($"Preparing {module.DisplayName}…", count + 1 + i, total);
            await RenderAsync();

            if (!_activePages.ContainsKey(module.Id))
            {
                _activePages[module.Id] = module.CreatePage();
            }

            await PreloadBackgroundAsync(module.Id);
        }

        ReportProgress("Opening dashboard…", total - 1, total);
        await RenderAsync();
        NavigateToDefaultModule();

        LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    // Ignores a saved position that would leave the window off-screen (e.g. a monitor was removed).
    private static bool IsOnScreen(Rect bounds)
    {
        var screen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
        var visible = Rect.Intersect(bounds, screen);
        return !visible.IsEmpty && visible.Width >= 100 && visible.Height >= 50;
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

    private void NextSlideshowImage()
    {
        if (string.IsNullOrEmpty(_currentModuleId))
        {
            return;
        }

        var config = _backgroundSettings.GetSlideshow(_currentModuleId);
        if (!config.Enabled || string.IsNullOrWhiteSpace(config.Folder))
        {
            return;
        }

        AdvanceSlideshow(_currentModuleId, config);
        _slideshowTimer.Stop();
        _slideshowTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, config.IntervalSeconds));
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
            var image = LoadImage(path);

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

    private readonly Dictionary<string, (DateTime Stamp, System.Windows.Media.Imaging.BitmapImage Image)> _imageCache = new();

    private readonly HashSet<string> _pinnedPaths = new();
    private readonly Dictionary<string, string> _pinnedByModule = new();

    private void PinImage(string moduleId, string path)
    {
        _pinnedByModule[moduleId] = path;
        _pinnedPaths.Clear();
        _pinnedPaths.UnionWith(_pinnedByModule.Values);
    }

    private void UnpinImage(string moduleId)
    {
        if (_pinnedByModule.Remove(moduleId))
        {
            _pinnedPaths.Clear();
            _pinnedPaths.UnionWith(_pinnedByModule.Values);
        }
    }

    private static System.Windows.Media.Imaging.BitmapImage DecodeImage(string path, int pixelWidth)
    {
        var image = new System.Windows.Media.Imaging.BitmapImage();
        image.BeginInit();
        image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = pixelWidth;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private void StoreImage(string path, DateTime stamp, System.Windows.Media.Imaging.BitmapImage image)
    {
        if (_imageCache.Count >= 12)
        {
            foreach (var key in _imageCache.Keys.Where(k => !_pinnedPaths.Contains(k)).ToList())
            {
                _imageCache.Remove(key);
            }
        }

        _imageCache[path] = (stamp, image);
    }

    private System.Windows.Media.Imaging.BitmapImage LoadImage(string path)
    {
        var stamp = File.GetLastWriteTimeUtc(path);
        if (_imageCache.TryGetValue(path, out var cached) && cached.Stamp == stamp)
        {
            return cached.Image;
        }

        var image = DecodeImage(path, (int)Math.Min(SystemParameters.VirtualScreenWidth, 3840));
        StoreImage(path, stamp, image);
        return image;
    }

    private async Task PreloadBackgroundAsync(string moduleId, bool warmUp = true)
    {
        var path = _backgroundSettings.GetPath(moduleId);
        var slideshow = _backgroundSettings.GetSlideshow(moduleId);
        if (slideshow.Enabled && !string.IsNullOrWhiteSpace(slideshow.Folder))
        {
            try
            {
                var files = Directory.EnumerateFiles(slideshow.Folder)
                    .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .ToArray();
                if (files.Length > 0)
                {
                    var index = slideshow.Shuffle && files.Length > 1 ? Random.Shared.Next(files.Length) : 0;
                    path = files[index];
                    _slideshowStates[moduleId] = new SlideshowState
                    {
                        Index = index,
                        File = path,
                        ChangedAt = DateTime.UtcNow,
                    };
                }
            }
            catch (Exception)
            {
            }
        }

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            UnpinImage(moduleId);
            return;
        }

        try
        {
            var stamp = File.GetLastWriteTimeUtc(path);
            var width = (int)Math.Min(SystemParameters.VirtualScreenWidth, 3840);
            var image = await Task.Run(() => DecodeImage(path, width));
            PinImage(moduleId, path);
            StoreImage(path, stamp, image);

            if (warmUp)
            {
                // Show it once behind the loading overlay so WPF uploads the bitmap to the GPU now, not on first navigation.
                ShowImage(path, animate: false);
                await RenderAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to preload background image '{path}'.", ex);
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

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _hotkey = new GlobalHotkey(this);
        _hotkey.Attach();
    }

    protected override void OnClosed(EventArgs e)
    {
        _hotkey?.Dispose();
        _trayIcon.Dispose();
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

        if (e.Key == Key.F5)
        {
            NextSlideshowImage();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers != ModifierKeys.None)
        {
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
