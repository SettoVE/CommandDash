namespace CommandDash.Core;

/// <summary>
/// Contract that every CommandDash module must implement in order to be
/// discovered and hosted by the application shell.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Stable, unique identifier for this module (e.g. "commanddash.network").
    /// Should not change between versions.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name shown in the sidebar and module card.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Short description shown on the module card / tooltip.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Category used to group modules in the sidebar (e.g. "System", "Network", "Appearance").
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Icon glyph (e.g. Segoe Fluent Icons codepoint) or a resource path,
    /// used for the sidebar entry and module card.
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// Module version, used for display and update checks.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Called once when the module is first loaded by the host, before any
    /// page is created. Use this for one-time initialization.
    /// </summary>
    void OnLoaded(IModuleContext context);

    /// <summary>
    /// Called when the module is being unloaded (app shutdown or module
    /// removal). Use this to release resources.
    /// </summary>
    void OnUnloaded();

    /// <summary>
    /// Creates the page/view for this module. Called each time the user
    /// navigates to the module from the shell.
    /// </summary>
    IModulePage CreatePage();
}
