using System.IO;
using System.Text.Json;
using System.Windows;

namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Persists the Dashboard module settings (widget alignment within the dashboard display).
/// </summary>
internal sealed class DashboardSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;
    private readonly SettingsData _data;

    public DashboardSettings(string dataDirectory)
    {
        _path = Path.Combine(dataDirectory, "dashboard-settings.json");
        _data = Load() ?? new SettingsData();
    }

    public event EventHandler? Changed;

    public HorizontalAlignment HorizontalAlignment
    {
        get => _data.HorizontalAlignment is HorizontalAlignment.Left or HorizontalAlignment.Center or HorizontalAlignment.Right or HorizontalAlignment.Stretch
            ? _data.HorizontalAlignment
            : HorizontalAlignment.Left;
        set { _data.HorizontalAlignment = value; Save(); }
    }

    public VerticalAlignment VerticalAlignment
    {
        get => _data.VerticalAlignment is VerticalAlignment.Top or VerticalAlignment.Center or VerticalAlignment.Bottom or VerticalAlignment.Stretch
            ? _data.VerticalAlignment
            : VerticalAlignment.Top;
        set { _data.VerticalAlignment = value; Save(); }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(_data, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private SettingsData? Load()
    {
        try
        {
            return File.Exists(_path) ? JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path)) : null;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private sealed class SettingsData
    {
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
    }
}
