using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// GPU usage from the Windows "GPU Engine" performance counters (same source as Task Manager).
/// Utilization is summed per engine type; the busiest engine type is reported.
/// </summary>
public sealed class GpuWidget : HardwareWidgetBase
{
    private static readonly PerformanceCounterCategory Category = new("GPU Engine");
    private Dictionary<string, CounterSample> _previous = new();
    private TextBlock? _value;
    private ProgressBar? _bar;
    private double _usage;

    public override string Id => "commanddash.home.gpu";

    public override WidgetGridPosition? DefaultPosition => new(1, 1);

    public override string DisplayName => "GPU";

    public override string? Subtitle => HardwareInfo.GpuName;

    public override object CreateView()
    {
        _value = CreateText(32, "TextPrimaryBrush", FontWeights.SemiBold);
        _value.Text = "--%";
        _bar = CreateBar();
        _bar.Margin = new Thickness(0, 14, 0, 0);
        return CreatePanel(_value, _bar);
    }

    protected override void Sample()
    {
        var byEngine = new Dictionary<string, double>();
        var current = new Dictionary<string, CounterSample>();

        try
        {
            var data = Category.ReadCategory();
            if (data.Contains("Utilization Percentage"))
            {
                foreach (InstanceData instance in data["Utilization Percentage"].Values)
                {
                    var sample = instance.Sample;
                    current[instance.InstanceName] = sample;
                    if (!_previous.TryGetValue(instance.InstanceName, out var previous)) continue;

                    var engine = EngineType(instance.InstanceName);
                    byEngine[engine] = byEngine.GetValueOrDefault(engine) + CounterSample.Calculate(previous, sample);
                }
            }
        }
        catch (InvalidOperationException)
        {
        }

        _previous = current;
        _usage = byEngine.Count == 0 ? 0 : Math.Clamp(byEngine.Values.Max(), 0, 100);
    }

    private static string EngineType(string instance)
    {
        var i = instance.IndexOf("engtype_", StringComparison.Ordinal);
        return i < 0 ? instance : instance[i..];
    }

    protected override void UpdateView()
    {
        if (_value is not null) _value.Text = $"{_usage:0}%";
        if (_bar is not null) _bar.Value = _usage;
    }
}
