using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace CommandDash.Modules.RunScript;

internal static class ScriptRunner
{
    /// <summary>Runs a script hidden and returns its exit code (null if it could not start) and combined output.</summary>
    public static async Task<(int? ExitCode, string Output)> RunAsync(string scriptPath, RunScriptStore store)
    {
        var extension = Path.GetExtension(scriptPath);
        var startInfo = new ProcessStartInfo
        {
            FileName = store.GetInterpreter(extension),
            WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? string.Empty,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        switch (extension.ToLowerInvariant())
        {
            case ".ps1":
                startInfo.ArgumentList.Add("-NoProfile");
                startInfo.ArgumentList.Add("-ExecutionPolicy");
                startInfo.ArgumentList.Add("Bypass");
                startInfo.ArgumentList.Add("-File");
                startInfo.ArgumentList.Add(scriptPath);
                break;
            case ".bat":
                startInfo.ArgumentList.Add("/c");
                startInfo.ArgumentList.Add(scriptPath);
                break;
            default:
                startInfo.ArgumentList.Add(scriptPath);
                break;
        }

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return (null, "Failed to start process.");
            }

            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var output = (await stdout + Environment.NewLine + await stderr).Trim();
            return (process.ExitCode, output);
        }
        catch (Win32Exception)
        {
            return (null, $"Could not start '{startInfo.FileName}'. Is it installed and on PATH (or set in settings)?");
        }
    }
}
