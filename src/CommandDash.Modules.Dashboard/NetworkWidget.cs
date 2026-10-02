using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Shows live upload/download speed plus usage for the current month and the last 24 hours.
/// Usage is accumulated into hourly buckets persisted under %AppData%\CommandDash.
/// </summary>
public sealed class NetworkWidget : HardwareWidgetBase
{
    private sealed record Bucket(long Down, long Up);

    private static readonly string StorePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CommandDash", "network-usage.json");

    private readonly Dictionary<string, Bucket> _buckets = new();
    private readonly object _lock = new();
    private long _lastDown = -1;
    private long _lastUp = -1;
    private DateTime _lastSample;
    private DateTime _lastSave = DateTime.UtcNow;
    private bool _dirty;
    private bool _loaded;

    private double _downSpeed;
    private double _upSpeed;
    private long _month;
    private long _day;

    private TextBlock? _downText;
    private TextBlock? _upText;
    private Run? _downValue;
    private Run? _upValue;
    private TextBlock? _monthText;
    private TextBlock? _dayText;

    public override string Id => "commanddash.home.network";

    public override WidgetGridPosition? DefaultPosition => new(2, 1);

    public override string DisplayName => "Network";

    public override object CreateView()
    {
        _downText = CreateText(20, "TextPrimaryBrush", FontWeights.SemiBold);
        _upText = CreateText(20, "TextPrimaryBrush", FontWeights.SemiBold);
        _downValue = new Run(" --");
        _upValue = new Run(" --");
        var downArrow = new Run("⬇");
        downArrow.SetResourceReference(TextElement.ForegroundProperty, "DownloadBrush");
        var upArrow = new Run("⬆");
        upArrow.SetResourceReference(TextElement.ForegroundProperty, "UploadBrush");
        _downText.Inlines.Add(downArrow);
        _downText.Inlines.Add(_downValue);
        _upText.Inlines.Add(upArrow);
        _upText.Inlines.Add(_upValue);
        var speeds = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch };
        speeds.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        speeds.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(_upText, 1);
        speeds.Children.Add(_downText);
        speeds.Children.Add(_upText);
        _dayText = CreateText(14, "TextSecondaryBrush");
        _monthText = CreateText(14, "TextSecondaryBrush");
        _dayText.Margin = new Thickness(0, 8, 0, 0);
        return CreatePanel(speeds, _dayText, _monthText);
    }

    protected override void Sample()
    {
        if (!_loaded) { Load(); _loaded = true; }

        long down = 0, up = 0;
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up) continue;
            if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
            var stats = nic.GetIPv4Statistics();
            down += stats.BytesReceived;
            up += stats.BytesSent;
        }

        var now = DateTime.UtcNow;
        if (_lastDown >= 0 && down >= _lastDown && up >= _lastUp)
        {
            var seconds = Math.Max((now - _lastSample).TotalSeconds, 0.001);
            var dDown = down - _lastDown;
            var dUp = up - _lastUp;
            _downSpeed = dDown / seconds;
            _upSpeed = dUp / seconds;
            AddUsage(now, dDown, dUp);
        }
        else
        {
            _downSpeed = _upSpeed = 0;
        }

        _lastDown = down;
        _lastUp = up;
        _lastSample = now;
        ComputeTotals(now);

        if (_dirty && now - _lastSave > TimeSpan.FromSeconds(30)) Save();
    }

    private static string KeyFor(DateTime utc) => new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc).ToString("O");

    private void AddUsage(DateTime utc, long down, long up)
    {
        lock (_lock)
        {
            var key = KeyFor(utc);
            _buckets.TryGetValue(key, out var b);
            _buckets[key] = new Bucket((b?.Down ?? 0) + down, (b?.Up ?? 0) + up);
            _dirty = true;
        }
    }

    private void ComputeTotals(DateTime utc)
    {
        var local = utc.ToLocalTime();
        var monthStart = new DateTime(local.Year, local.Month, 1, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
        var dayStart = utc.AddHours(-24);
        long month = 0, day = 0;
        lock (_lock)
        {
            foreach (var (key, b) in _buckets)
            {
                var t = DateTime.Parse(key, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var total = b.Down + b.Up;
                if (t >= monthStart) month += total;
                if (t.AddHours(1) > dayStart) day += total;
            }
        }
        _month = month;
        _day = day;
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(StorePath)) return;
            var data = JsonSerializer.Deserialize<Dictionary<string, long[]>>(File.ReadAllText(StorePath));
            if (data is null) return;
            lock (_lock)
            {
                foreach (var (k, v) in data)
                    if (v.Length == 2) _buckets[k] = new Bucket(v[0], v[1]);
            }
        }
        catch
        {
        }
    }

    private void Save()
    {
        try
        {
            Dictionary<string, long[]> data;
            lock (_lock)
            {
                var cutoff = DateTime.UtcNow.AddDays(-62);
                foreach (var key in _buckets.Keys.Where(k => DateTime.Parse(k, null, System.Globalization.DateTimeStyles.RoundtripKind) < cutoff).ToList())
                    _buckets.Remove(key);
                data = _buckets.ToDictionary(p => p.Key, p => new[] { p.Value.Down, p.Value.Up });
            }
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            File.WriteAllText(StorePath, JsonSerializer.Serialize(data));
            _dirty = false;
            _lastSave = DateTime.UtcNow;
        }
        catch
        {
        }
    }

    protected override void UpdateView()
    {
        if (_downValue is not null) _downValue.Text = $" {FormatSpeed(_downSpeed)}";
        if (_upValue is not null) _upValue.Text = $" {FormatSpeed(_upSpeed)}";
        if (_dayText is not null) _dayText.Text = $"24 hr: {FormatBytes(_day)}";
        if (_monthText is not null) _monthText.Text = $"Month: {FormatBytes(_month)}";
    }

    private static string FormatSpeed(double bytesPerSecond) => FormatBytes(bytesPerSecond) + "/s";

    private static string FormatBytes(double bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        var i = 0;
        while (bytes >= 1024 && i < units.Length - 1) { bytes /= 1024; i++; }
        return i == 0 ? $"{bytes:0} {units[i]}" : $"{bytes:0.0} {units[i]}";
    }
}
