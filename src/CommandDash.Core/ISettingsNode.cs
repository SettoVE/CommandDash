namespace CommandDash.Core;

/// <summary>
/// A single node in the Settings window's hierarchy (foobar2000-style
/// preferences tree). A node may have its own content page, child nodes,
/// or both — there is no separate "tab" concept; nesting is achieved by
/// composing nodes recursively.
/// </summary>
public interface ISettingsNode
{
    /// <summary>
    /// Stable, unique identifier for this node (e.g. "general",
    /// "modules", "modules.moduleorder", or "&lt;moduleId&gt;.settings").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name shown in the settings sidebar tree.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Child nodes nested beneath this one in the tree. Empty for a leaf
    /// node.
    /// </summary>
    IReadOnlyList<ISettingsNode> Children { get; }

    /// <summary>
    /// Creates the view (e.g. a WPF UserControl) shown when this node is
    /// selected, or <c>null</c> if this node is purely a category with no
    /// page of its own (selecting it would show nothing / an empty area).
    /// Called once per selection; implementations that want to cache their
    /// view can do so themselves.
    /// </summary>
    object? CreateContent();
}

/// <summary>
/// Simple, reusable <see cref="ISettingsNode"/> implementation backed by a
/// content factory delegate and an explicit list of children.
/// </summary>
public sealed class SettingsNode : ISettingsNode
{
    private readonly Func<object?> _contentFactory;

    public SettingsNode(
        string id,
        string displayName,
        Func<object?>? contentFactory = null,
        IReadOnlyList<ISettingsNode>? children = null)
    {
        Id = id;
        DisplayName = displayName;
        _contentFactory = contentFactory ?? (() => null);
        Children = children ?? Array.Empty<ISettingsNode>();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public IReadOnlyList<ISettingsNode> Children { get; }

    public object? CreateContent() => _contentFactory();
}
