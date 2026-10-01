using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CommandDash.Modules.Dashboard;

public sealed class CpuWidget : HardwareWidgetBase
{
    private PerformanceCounter? _counter;
    private TextBlock? _value;
    private ProgressBar? _bar;
    private double _usage;

    public override string Id => "commanddash.home.cpu";

    public override string DisplayName => "CPU";

    public override object CreateView()
    {
        _value = CreateText(32, "TextPrimaryBrush", FontWeights.SemiBold);
        _value.Text = "--%";
        _bar = CreateBar();
        return CreatePanel(_value, _bar);
    }

    protected override void Sample()
    {
        _counter ??= new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
        _usage = Math.Clamp(_counter.NextValue(), 0, 100);
    }

    protected override void UpdateView()
    {
        if (_value is not null) _value.Text = $"{_usage:0}%";
        if (_bar is not null) _bar.Value = _usage;
    }
}
