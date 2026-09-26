namespace CommandDash.Core;

/// <summary>
/// Optional contract a module can implement to contribute its own entry
/// (and, if desired, further nested entries) to the Settings window's
/// "Modules" node. Modules that do not implement this interface simply do
/// not appear under "Modules".
/// </summary>
public interface IModuleWithSettings
{
    /// <summary>
    /// Creates the settings node representing this module. The node's
    /// <see cref="ISettingsNode.DisplayName"/> is typically the module's
    /// own display name, and its <see cref="ISettingsNode.CreateContent"/>
    /// / <see cref="ISettingsNode.Children"/> describe the module's
    /// settings UI.
    /// </summary>
    ISettingsNode GetSettingsNode();
}
