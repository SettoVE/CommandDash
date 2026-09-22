namespace CommandDash.Core;

/// <summary>
/// Services and information the host provides to a module at load time.
/// </summary>
public interface IModuleContext
{
    /// <summary>
    /// Full path to a writable directory the module can use for its own
    /// settings/data storage.
    /// </summary>
    string DataDirectory { get; }

    /// <summary>
    /// Application-wide logger the module can use instead of writing its own.
    /// </summary>
    IModuleLogger Logger { get; }
}

/// <summary>
/// Minimal logging abstraction exposed to modules so they don't need to take
/// a dependency on a specific logging framework.
/// </summary>
public interface IModuleLogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}
