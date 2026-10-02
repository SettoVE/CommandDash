using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommandDash.App.Settings;

internal enum BindOutcome
{
    Cancel,
    Ok,
    Clear,
}

/// <summary>Records a key combination for the global hotkey and lets the user bind, clear, or cancel.</summary>
internal sealed class HotkeyBindWindow : Window
{
    private readonly TextBlock _lastText;
    private readonly TextBlock _errorText;
    private AppHotkey? _last;

    public HotkeyBindWindow(Window? owner, AppHotkey? current)
    {
        Title = "Bind - Focus Hotkey";
        Owner = owner;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        SetResourceReference(BackgroundProperty, "BackgroundBrush");

        var panel = new StackPanel { Margin = new Thickness(16), MinWidth = 300 };
        panel.Children.Add(Text("Press a key or key combination (a modifier is required).", "TextSecondaryBrush", new Thickness(0, 0, 0, 12)));
        panel.Children.Add(Text($"Current binding: {(current?.ToString() ?? "(none)")}", "TextPrimaryBrush", new Thickness(0, 0, 0, 4)));
        _lastText = Text("Last keystroke: (none)", "TextPrimaryBrush", new Thickness(0, 0, 0, 8));
        panel.Children.Add(_lastText);

        _errorText = Text("Invalid Keybind", "ErrorBrush", new Thickness(0, 0, 0, 8));
        _errorText.Visibility = Visibility.Collapsed;
        panel.Children.Add(_errorText);

        var ok = new Button { Content = "Ok", MinWidth = 70, Margin = new Thickness(0, 0, 8, 0), Focusable = false };
        var clear = new Button { Content = "Clear", MinWidth = 70, Margin = new Thickness(0, 0, 8, 0), Focusable = false };
        var cancel = new Button { Content = "Cancel", MinWidth = 70, Focusable = false };
        ok.SetResourceReference(StyleProperty, "PrimaryButtonStyle");
        ok.Click += (_, _) => Accept();
        clear.Click += (_, _) => Finish(BindOutcome.Clear);
        cancel.Click += (_, _) => Finish(BindOutcome.Cancel);
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0),
            Children = { ok, clear, cancel },
        });

        Content = panel;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public BindOutcome Outcome { get; private set; } = BindOutcome.Cancel;

    public AppHotkey? Result { get; private set; }

    private static TextBlock Text(string text, string brushKey, Thickness margin)
    {
        var block = new TextBlock { Text = text, Margin = margin, TextWrapping = TextWrapping.Wrap };
        block.SetResourceReference(TextBlock.ForegroundProperty, brushKey);
        return block;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        if (e.Key == Key.Escape)
        {
            Finish(BindOutcome.Cancel);
            return;
        }

        if (AppHotkey.FromEvent(e) is { } hotkey)
        {
            _last = hotkey;
            _lastText.Text = $"Last keystroke: {hotkey}";
            _errorText.Visibility = Visibility.Collapsed;
        }
    }

    private void Accept()
    {
        if (_last is not { } hotkey || !hotkey.IsAllowed)
        {
            _errorText.Visibility = Visibility.Visible;
            return;
        }

        Result = hotkey;
        Finish(BindOutcome.Ok);
    }

    private void Finish(BindOutcome outcome)
    {
        Outcome = outcome;
        Close();
    }
}
