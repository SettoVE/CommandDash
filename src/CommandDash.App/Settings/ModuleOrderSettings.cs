using System.IO;
using System.Text.Json;

namespace CommandDash.App.Settings;

/// <summary>
/// Persists the user-customizable sidebar load/display order of modules as
/// a simple ordered list of module ids, stored as JSON under
/// %AppData%\CommandDash\settings.json.
/// </summary>
public sealed class ModuleOrderSettings
{
    private readonly string _filePath;

    public ModuleOrderSettings()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CommandDash");
        Directory.CreateDirectory(settingsDirectory);
        _filePath = Path.Combine(settingsDirectory, "settings.json");
    }

    public List<string> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<string>();
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            return data?.ModuleOrder ?? new List<string>();
        }
        catch (Exception)
        {
            // Corrupt or unreadable settings should not prevent the app from
            // starting; fall back to default (registration) order.
            return new List<string>();
        }
    }

    public void Save(IReadOnlyList<string> moduleOrder)
    {
        var data = new SettingsData { ModuleOrder = moduleOrder.ToList() };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private sealed class SettingsData
    {
        public List<string> ModuleOrder { get; set; } = new();
    }
}
