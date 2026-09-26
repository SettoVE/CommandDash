using CommandDash.Core;
using System.IO;

namespace CommandDash.App.Modules;

/// <summary>
/// Default <see cref="IModuleContext"/> implementation supplied by the host
/// to each module at load time.
/// </summary>
public sealed class ModuleContext : IModuleContext
{
    public string DataDirectory { get; }

    public IModuleLogger Logger { get; }

    public ModuleContext(string moduleId, IModuleLogger logger)
    {
        DataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CommandDash", "Modules", moduleId);
        Directory.CreateDirectory(DataDirectory);
        Logger = logger;
    }
}

/// <summary>
/// Minimal <see cref="IModuleLogger"/> implementation that writes to
/// <see cref="System.Diagnostics.Debug"/>. Can be swapped for a real
/// logging framework later without changing the module contract.
/// </summary>
public sealed class DebugModuleLogger : IModuleLogger
{
    public void Info(string message) => System.Diagnostics.Debug.WriteLine($"[INFO] {message}");

    public void Warning(string message) => System.Diagnostics.Debug.WriteLine($"[WARN] {message}");

    public void Error(string message, Exception? exception = null) =>
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message} {exception}");
}
