using CommandDash.Core;

namespace CommandDash.App.Settings;

/// <summary>
/// Built-in "General" node. Intentionally blank for now; add content or
/// children here as general app-wide settings are introduced.
/// </summary>
public sealed class GeneralSettingsNode : ISettingsNode
{
    public string Id => "general";

    public string DisplayName => "General";

    public IReadOnlyList<ISettingsNode> Children { get; } = Array.Empty<ISettingsNode>();

    public object? CreateContent() => new GeneralSettingsView();
}
