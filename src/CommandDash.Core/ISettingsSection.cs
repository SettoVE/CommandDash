namespace CommandDash.Core;

/// <summary>
/// A single tab within a settings section. Content is created lazily via
/// <see cref="CreateContent"/> only when the tab is actually selected.
/// </summary>
public interface ISettingsTab
{
    /// <summary>
    /// Text shown on the tab header.
    /// </summary>
    string Header { get; }

    /// <summary>
    /// Creates the view (e.g. a WPF UserControl) shown when this tab is
    /// selected. Called once per selection; implementations that want to
    /// cache their view can do so themselves.
    /// </summary>
    object CreateContent();
}

/// <summary>
/// A single entry in the Settings window's sidebar. Selecting a section
/// shows its <see cref="Tabs"/> in a tab strip on the right.
/// </summary>
public interface ISettingsSection
{
    /// <summary>
    /// Stable, unique identifier for this section (e.g. "general",
    /// "moduleorder", or "&lt;moduleId&gt;.settings").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name shown in the settings sidebar.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Group used to organize sections in the settings sidebar.
    /// Sections with Group == "Default" render with no header (they're the
    /// app's own built-in settings). Any other group value (e.g. "Modules")
    /// renders with a label above the first section in that group.
    /// </summary>
    string Group { get; }

    /// <summary>
    /// Tabs shown for this section, in display order.
    /// </summary>
    IReadOnlyList<ISettingsTab> Tabs { get; }
}

/// <summary>
/// Simple, reusable <see cref="ISettingsTab"/> implementation backed by a
/// content factory delegate.
/// </summary>
public sealed class SettingsTab : ISettingsTab
{
    private readonly Func<object> _contentFactory;

    public SettingsTab(string header, Func<object> contentFactory)
    {
        Header = header;
        _contentFactory = contentFactory;
    }

    public string Header { get; }

    public object CreateContent() => _contentFactory();
}
