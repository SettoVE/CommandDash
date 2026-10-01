using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Base for widgets that sample a hardware metric once per second while the dashboard is visible.
/// Sampling runs on a background thread; results are applied on the UI thread.
/// </summary>
public abstract class HardwareWidgetBase : IWidget, IGridPositionedWidget
{
    private readonly DispatcherTimer _timer;
    private bool _sampling;

    protected HardwareWidgetBase()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += async (_, _) => await TickAsync();
    }

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public WidgetSize PreferredSize => WidgetSize.Medium;

    public bool IsResizable => false;

    public virtual WidgetGridPosition? DefaultPosition => null;

    public abstract object CreateView();

    public void OnActivated()
    {
        _ = TickAsync();
        _timer.Start();
    }

    public void OnDeactivated() => _timer.Stop();

    /// <summary>Runs on a background thread.</summary>
    protected abstract void Sample();

    /// <summary>Runs on the UI thread after each sample.</summary>
    protected abstract void UpdateView();

    private async Task TickAsync()
    {
        if (_sampling) return;
        _sampling = true;
        try
        {
            await Task.Run(Sample);
            UpdateView();
        }
        catch
        {
            // Metric unavailable; keep last displayed values.
        }
        finally
        {
            _sampling = false;
        }
    }

    protected static TextBlock CreateText(double size, string brushKey, FontWeight? weight = null)
    {
        var text = new TextBlock { FontSize = size, HorizontalAlignment = HorizontalAlignment.Center };
        if (weight is not null) text.FontWeight = weight.Value;
        text.SetResourceReference(TextBlock.ForegroundProperty, brushKey);
        return text;
    }

    protected static ProgressBar CreateBar()
    {
        var bar = new ProgressBar { Minimum = 0, Maximum = 100, Height = 6, Margin = new Thickness(0, 8, 0, 0) };
        bar.SetResourceReference(Control.ForegroundProperty, "AccentBrush");
        bar.SetResourceReference(Control.BackgroundProperty, "CardBrush");
        bar.BorderThickness = new Thickness(0);
        return bar;
    }

    protected static StackPanel CreatePanel(params UIElement[] children)
    {
        var panel = new StackPanel { Margin = new Thickness(16), VerticalAlignment = VerticalAlignment.Center };
        foreach (var c in children) panel.Children.Add(c);
        return panel;
    }
}
