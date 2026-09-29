using System.IO;
using System.Text.Json;

namespace CommandDash.App.Settings;

/// <summary>
/// Persists an optional custom background image path per module, stored as
/// JSON under %AppData%\CommandDash\backgrounds.json. A module with no entry
/// uses the default (no custom background).
/// </summary>
public sealed class ModuleBackgroundSettings
{
    private readonly string _filePath;
    private readonly Dictionary<string, string> _paths;

    public ModuleBackgroundSettings()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CommandDash");
        Directory.CreateDirectory(settingsDirectory);
        _filePath = Path.Combine(settingsDirectory, "backgrounds.json");
        _paths = Load();
    }

    /// <summary>
    /// Raised with the module id whose background path was changed.
    /// </summary>
    public event EventHandler<string>? BackgroundChanged;

    public string GetPath(string moduleId) =>
        _paths.TryGetValue(moduleId, out var path) ? path : string.Empty;

    public void SetPath(string moduleId, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            _paths.Remove(moduleId);
        }
        else
        {
            _paths[moduleId] = path;
        }

        Save();
        BackgroundChanged?.Invoke(this, moduleId);
    }

    private Dictionary<string, string> Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data is not null)
                {
                    return data;
                }
            }
        }
        catch (Exception)
        {
            // Corrupt or unreadable settings fall back to defaults.
        }

        return new Dictionary<string, string>();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_paths, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
