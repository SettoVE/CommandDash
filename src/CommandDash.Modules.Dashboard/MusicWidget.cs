using System.Windows;
using System.Windows.Controls;
using CommandDash.Core;
using Windows.Media.Control;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Widget that shows and controls music playback from foobar2000 and Spotify
/// (System Media Transport Controls).
/// </summary>
public sealed class MusicWidget : IWidget, IGridPositionedWidget
{
    private const string PlayGlyph = "\uE768";
    private const string PauseGlyph = "\uE769";

    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private GlobalSystemMediaTransportControlsSession? _session;
    private TextBlock? _titleText;
    private TextBlock? _artistText;
    private Button? _playPauseButton;
    private Button? _previousButton;
    private Button? _nextButton;
    private bool _active;

    public string Id => "commanddash.home.media";

    public string DisplayName => "Music";

    public WidgetSize PreferredSize => WidgetSize.Medium;

    public bool IsResizable => false;

    public WidgetGridPosition? DefaultPosition => new(0, 1);

    public object CreateView()
    {
        _titleText = new TextBlock { FontSize = 20, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Text = string.Empty };
        _titleText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
        _artistText = new TextBlock { FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _artistText.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");

        _previousButton = CreateButton("\uE892", "Previous", async () => await RunAsync(s => s.TrySkipPreviousAsync()));
        _playPauseButton = CreateButton(PlayGlyph, "Play/Pause", async () => await RunAsync(s => s.TryTogglePlayPauseAsync()));
        _nextButton = CreateButton("\uE893", "Next", async () => await RunAsync(s => s.TrySkipNextAsync()));

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) };
        buttons.Children.Add(_previousButton);
        buttons.Children.Add(_playPauseButton);
        buttons.Children.Add(_nextButton);

        var panel = new StackPanel { Margin = new Thickness(16), VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(_titleText);
        panel.Children.Add(_artistText);
        panel.Children.Add(buttons);

        UpdateView(null);
        return panel;
    }

    public async void OnActivated()
    {
        _active = true;
        try
        {
            _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            if (!_active) return;
            _manager.SessionsChanged += OnSessionsChanged;
            WatchAllSessions();
            AttachSession(PickSession());
        }
        catch
        {
            UpdateView(null);
        }
    }

    public void OnDeactivated()
    {
        _active = false;
        if (_manager is not null) _manager.SessionsChanged -= OnSessionsChanged;
        UnwatchAllSessions();
        AttachSession(null);
    }

    private readonly List<GlobalSystemMediaTransportControlsSession> _watched = new();

    private void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, object args)
        => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            if (!_active) return;
            WatchAllSessions();
            if (_session is null || !_watched.Contains(_session)) AttachSession(PickSession());
        });

    private static bool IsSupported(GlobalSystemMediaTransportControlsSession session)
    {
        var id = session.SourceAppUserModelId ?? string.Empty;
        return id.Contains("foobar", StringComparison.OrdinalIgnoreCase)
            || id.Contains("spotify", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlaying(GlobalSystemMediaTransportControlsSession session)
        => session.GetPlaybackInfo()?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

    private GlobalSystemMediaTransportControlsSession? PickSession()
        => _watched.FirstOrDefault(IsPlaying) ?? _watched.FirstOrDefault();

    private void WatchAllSessions()
    {
        UnwatchAllSessions();
        if (_manager is null) return;
        foreach (var s in _manager.GetSessions().Where(IsSupported))
        {
            s.PlaybackInfoChanged += OnAnyPlaybackChanged;
            s.MediaPropertiesChanged += OnAnyPlaybackChanged;
            _watched.Add(s);
        }
    }

    private void UnwatchAllSessions()
    {
        foreach (var s in _watched)
        {
            s.PlaybackInfoChanged -= OnAnyPlaybackChanged;
            s.MediaPropertiesChanged -= OnAnyPlaybackChanged;
        }
        _watched.Clear();
    }

    private void OnAnyPlaybackChanged(GlobalSystemMediaTransportControlsSession sender, object args)
        => Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            if (!_active || ReferenceEquals(sender, _session)) return;
            if (_session is null || !IsPlaying(_session) || IsPlaying(sender)) AttachSession(IsPlaying(sender) ? sender : PickSession());
        });

    private static Button CreateButton(string glyph, string tooltip, Action onClick)
    {
        var icon = new TextBlock
        {
            Text = glyph,
            FontFamily = new System.Windows.Media.FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        icon.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var circle = new Border
        {
            Width = 38,
            Height = 38,
            CornerRadius = new CornerRadius(19),
            BorderThickness = new Thickness(1.5),
            Background = System.Windows.Media.Brushes.Transparent,
            Child = icon,
        };
        circle.SetResourceReference(Border.BorderBrushProperty, "TextPrimaryBrush");

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        var template = new ControlTemplate(typeof(Button)) { VisualTree = presenter };
        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(UIElement.OpacityProperty, 0.7));
        var disabled = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter(UIElement.OpacityProperty, 0.35));
        template.Triggers.Add(hover);
        template.Triggers.Add(disabled);

        var button = new Button
        {
            Content = circle,
            ToolTip = tooltip,
            Template = template,
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(16, 4, 16, 0),
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private async Task RunAsync(Func<GlobalSystemMediaTransportControlsSession, Windows.Foundation.IAsyncOperation<bool>> action)
    {
        var session = _session;
        if (session is null) return;
        try { await action(session); } catch { }
    }

    private void AttachSession(GlobalSystemMediaTransportControlsSession? session)
    {
        if (_session is not null)
        {
            _session.MediaPropertiesChanged -= OnSessionChanged;
            _session.PlaybackInfoChanged -= OnSessionChanged;
        }

        _session = session;

        if (_session is not null)
        {
            _session.MediaPropertiesChanged += OnSessionChanged;
            _session.PlaybackInfoChanged += OnSessionChanged;
        }

        Refresh();
    }

    private void OnSessionChanged(GlobalSystemMediaTransportControlsSession sender, object args)
        => Application.Current?.Dispatcher.BeginInvoke(Refresh);

    private async void Refresh()
    {
        var session = _session;
        if (session is null)
        {
            UpdateView(null);
            return;
        }

        try
        {
            var props = await session.TryGetMediaPropertiesAsync();
            if (session != _session) return;
            var playing = session.GetPlaybackInfo()?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            UpdateView((props.Title, props.Artist, playing));
        }
        catch
        {
            UpdateView(null);
        }
    }

    private void UpdateView((string Title, string Artist, bool Playing)? info)
    {
        if (_titleText is null || _artistText is null) return;
        var enabled = info is not null;
        _titleText.Text = info is { } i && !string.IsNullOrWhiteSpace(i.Title) ? i.Title : (enabled ? "Unknown title" : string.Empty);
        _artistText.Text = info?.Artist ?? string.Empty;
        if ((_playPauseButton?.Content as Border)?.Child is TextBlock glyph)
            glyph.Text = info is { Playing: true } ? PauseGlyph : PlayGlyph;
        if (_playPauseButton is not null) _playPauseButton.IsEnabled = enabled;
        if (_previousButton is not null) _previousButton.IsEnabled = enabled;        if (_nextButton is not null) _nextButton.IsEnabled = enabled;
    }
}
