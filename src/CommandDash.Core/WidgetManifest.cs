namespace CommandDash.Core;

/// <summary>
/// Lightweight, serializable snapshot of a widget's metadata, used by the
/// host to render widget pickers and persist dashboard layout without
/// needing to keep every widget instance alive in memory.
/// </summary>
public sealed class WidgetManifest
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public WidgetSize PreferredSize { get; init; } = WidgetSize.Medium;
    public bool IsResizable { get; init; }

    /// <summary>
    /// Id of the module that owns/provides this widget, so the host can
    /// re-create it on demand via that module's <see cref="IWidgetProvider"/>.
    /// </summary>
    public required string OwnerModuleId { get; init; }

    public static WidgetManifest FromWidget(IWidget widget, string ownerModuleId) => new()
    {
        Id = widget.Id,
        DisplayName = widget.DisplayName,
        PreferredSize = widget.PreferredSize,
        IsResizable = widget.IsResizable,
        OwnerModuleId = ownerModuleId,
    };
}
