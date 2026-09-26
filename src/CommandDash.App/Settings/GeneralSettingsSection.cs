using CommandDash.Core;

namespace CommandDash.App.Settings;

/// <summary>
/// Built-in "General" settings section. Intentionally left blank for now;
/// add tabs here as general app-wide settings are introduced.
/// </summary>
public sealed class GeneralSettingsSection : ISettingsSection
{
    public string Id => "general";

    public string DisplayName => "General";

    public string Group => "Default";

    public IReadOnlyList<ISettingsTab> Tabs { get; } = new List<ISettingsTab>
    {
        new SettingsTab("General", () => new GeneralSettingsTabView()),
    };
}
