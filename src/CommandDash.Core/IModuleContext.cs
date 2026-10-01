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

    /// <summary>
    /// Widgets contributed by every loaded <see cref="IWidgetProvider"/> module, de-duplicated by
    /// widget id. Intended for widget-hosting surfaces such as the dashboard; call when building
    /// the page, not during <see cref="IModule.OnLoaded"/>.
    /// </summary>
    IReadOnlyList<IWidget> GetAvailableWidgets();
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
