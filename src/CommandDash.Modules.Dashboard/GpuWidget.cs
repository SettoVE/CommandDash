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
    private readonly Dictionary<string, PerformanceCounter> _counters = new();
    private DateTime _lastRefresh = DateTime.MinValue;
    private TextBlock? _value;
    private ProgressBar? _bar;
    private double _usage;

    public override string Id => "commanddash.home.gpu";

    public override WidgetGridPosition? DefaultPosition => new(1, 1);

    public override string DisplayName => "GPU";

    public override object CreateView()
    {
        _value = CreateText(32, "TextPrimaryBrush", FontWeights.SemiBold);
        _value.Text = "--%";
        _bar = CreateBar();
        return CreatePanel(_value, _bar);
    }

    protected override void Sample()
    {
        if (DateTime.UtcNow - _lastRefresh > TimeSpan.FromSeconds(5))
        {
            RefreshInstances();
            _lastRefresh = DateTime.UtcNow;
        }

        var byEngine = new Dictionary<string, double>();
        foreach (var (name, counter) in _counters.ToArray())
        {
            try
            {
                var engine = EngineType(name);
                byEngine[engine] = byEngine.GetValueOrDefault(engine) + counter.NextValue();
            }
            catch (InvalidOperationException)
            {
                counter.Dispose();
                _counters.Remove(name);
            }
        }

        _usage = byEngine.Count == 0 ? 0 : Math.Clamp(byEngine.Values.Max(), 0, 100);
    }

    private void RefreshInstances()
    {
        var category = new PerformanceCounterCategory("GPU Engine");
        var names = category.GetInstanceNames().ToHashSet();

        foreach (var stale in _counters.Keys.Where(k => !names.Contains(k)).ToList())
        {
            _counters[stale].Dispose();
            _counters.Remove(stale);
        }

        foreach (var name in names.Where(n => !_counters.ContainsKey(n)))
        {
            try
            {
                var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, true);
                counter.NextValue();
                _counters[name] = counter;
            }
            catch (InvalidOperationException)
            {
            }
        }
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
