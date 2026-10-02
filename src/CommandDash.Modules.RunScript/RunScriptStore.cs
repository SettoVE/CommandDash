using System.IO;
using System.Text.Json;

namespace CommandDash.Modules.RunScript;

/// <summary>
/// Persists the Run Script module settings and per-script hotkey bindings, and scans the scripts folder.
/// </summary>
internal sealed class RunScriptStore
{
    public static readonly string[] SupportedExtensions = { ".py", ".pyw", ".ps1", ".rb", ".bat" };

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _settingsPath;
    private SettingsData _data;

    public RunScriptStore(string dataDirectory)
    {
        _settingsPath = Path.Combine(dataDirectory, "runscript-settings.json");
        _data = Load<SettingsData>(_settingsPath) ?? new SettingsData();
    }

    public static string DefaultFolder => Path.Combine(AppContext.BaseDirectory, "scripts");

    /// <summary>Folder that is scanned for scripts (the default folder when none is chosen).</summary>
    public string ScriptsFolder
        => string.IsNullOrWhiteSpace(_data.ScriptsFolder) ? DefaultFolder : _data.ScriptsFolder;

    public bool UsesDefaultFolder => string.IsNullOrWhiteSpace(_data.ScriptsFolder);

    public string PythonPath => _data.PythonPath ?? string.Empty;

    public string PowerShellPath => _data.PowerShellPath ?? string.Empty;

    public string RubyPath => _data.RubyPath ?? string.Empty;

    public void SetScriptsFolder(string? folder)
    {
        _data.ScriptsFolder = string.IsNullOrWhiteSpace(folder) ? null : folder;
        Save();
    }

    public void SetInterpreters(string python, string powerShell, string ruby)
    {
        _data.PythonPath = python;
        _data.PowerShellPath = powerShell;
        _data.RubyPath = ruby;
        Save();
    }

    /// <summary>Returns the executable for a script extension, honoring settings overrides.</summary>
    public string GetInterpreter(string extension) => extension.ToLowerInvariant() switch
    {
        ".py" or ".pyw" => OrDefault(PythonPath, "python"),
        ".ps1" => OrDefault(PowerShellPath, "powershell"),
        ".rb" => OrDefault(RubyPath, "ruby"),
        _ => "cmd.exe",
    };

    public IReadOnlyList<string> ScanScripts()
    {
        var folder = ScriptsFolder;
        try
        {
            if (!Directory.Exists(folder))
            {
                if (!UsesDefaultFolder)
                {
                    return Array.Empty<string>();
                }

                Directory.CreateDirectory(folder);
            }

            return Directory.EnumerateFiles(folder)
                .Where(f => SupportedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    private string BindingsPath
    {
        get
        {
            var folder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(ScriptsFolder));
            return Path.Combine(Path.GetDirectoryName(folder) ?? folder, "script-bindings.json");
        }
    }

    /// <summary>Loads bindings keyed by script file name.</summary>
    public Dictionary<string, ScriptHotkey> LoadBindings()
    {
        var result = new Dictionary<string, ScriptHotkey>(StringComparer.OrdinalIgnoreCase);
        var raw = Load<Dictionary<string, string>>(BindingsPath);
        if (raw is null)
        {
            return result;
        }

        foreach (var (name, text) in raw)
        {
            if (ScriptHotkey.TryParse(text, out var hotkey))
            {
                result[name] = hotkey;
            }
        }

        return result;
    }

    public void SaveBindings(Dictionary<string, ScriptHotkey> bindings)
    {
        var raw = bindings.ToDictionary(b => b.Key, b => b.Value.ToString());
        Write(BindingsPath, raw);
    }

    private void Save() => Write(_settingsPath, _data);

    private static string OrDefault(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static T? Load<T>(string path) where T : class
    {
        try
        {
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : null;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static void Write<T>(string path, T value)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    private sealed class SettingsData
    {
        public string? ScriptsFolder { get; set; }

        public string? PythonPath { get; set; }

        public string? PowerShellPath { get; set; }

        public string? RubyPath { get; set; }
    }
}
