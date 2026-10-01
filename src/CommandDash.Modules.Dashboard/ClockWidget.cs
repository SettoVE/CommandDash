using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Widget that displays the current local time and date.
/// </summary>
public sealed class ClockWidget : IWidget, IGridPositionedWidget
{
    private readonly DispatcherTimer _timer;
    private TextBlock? _timeText;
    private TextBlock? _dateText;

    public ClockWidget()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => UpdateText();
    }

    public string Id => "commanddash.home.clock";

    public string DisplayName => "Clock";

    public WidgetSize PreferredSize => WidgetSize.Medium;

    public bool IsResizable => false;

    public WidgetGridPosition? DefaultPosition => new(0, 0);

    public object CreateView()
    {
        _timeText = new TextBlock { FontSize = 32, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center };
        _timeText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
        _dateText = new TextBlock { FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center };
        _dateText.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");

        var panel = new StackPanel { Margin = new Thickness(16), VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(_timeText);
        panel.Children.Add(_dateText);

        UpdateText();
        return panel;
    }

    public void OnActivated()
    {
        UpdateText();
        _timer.Start();
    }

    public void OnDeactivated() => _timer.Stop();

    private void UpdateText()
    {
        var now = DateTime.Now;
        if (_timeText is not null) _timeText.Text = now.ToString("T");
        if (_dateText is not null) _dateText.Text = now.ToString("D");
    }
}
