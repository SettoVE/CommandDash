using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace CommandDash.App.Settings;

/// <summary>
/// List of script paths run when the app starts, stored under %AppData%\CommandDash\startupscripts.json.
/// </summary>
public sealed class StartupScriptsSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CommandDash",
        "startupscripts.json");

    public List<string> Scripts { get; set; } = new();

    /// <summary>When true, scripts only run when the app is launched by Windows at boot.</summary>
    public bool OnlyOnWindowsStartup { get; set; } = true;

    public static StartupScriptsSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                return JsonSerializer.Deserialize<StartupScriptsSettings>(File.ReadAllText(FilePath)) ?? new StartupScriptsSettings();
            }
        }
        catch (Exception)
        {
            // Corrupt or unreadable settings fall back to defaults.
        }

        return new StartupScriptsSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>Launches every configured script hidden; failures are ignored.</summary>
    public static void RunAll()
    {
        foreach (var script in Load().Scripts)
        {
            try
            {
                if (!File.Exists(script)) continue;

                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(script) ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                switch (Path.GetExtension(script).ToLowerInvariant())
                {
                    case ".ps1":
                        startInfo.FileName = "powershell.exe";
                        startInfo.ArgumentList.Add("-NoProfile");
                        startInfo.ArgumentList.Add("-ExecutionPolicy");
                        startInfo.ArgumentList.Add("Bypass");
                        startInfo.ArgumentList.Add("-File");
                        startInfo.ArgumentList.Add(script);
                        break;
                    case ".bat":
                    case ".cmd":
                        startInfo.FileName = "cmd.exe";
                        startInfo.ArgumentList.Add("/c");
                        startInfo.ArgumentList.Add(script);
                        break;
                    default:
                        startInfo.FileName = script;
                        startInfo.UseShellExecute = true;
                        break;
                }

                Process.Start(startInfo)?.Dispose();
            }
            catch (Exception)
            {
                // A failing script must not prevent app startup.
            }
        }
    }
}
