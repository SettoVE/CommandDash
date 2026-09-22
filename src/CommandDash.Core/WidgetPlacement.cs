namespace CommandDash.Core;

/// <summary>
/// Persisted record of a single widget instance placed on a dashboard
/// (e.g. the Home module's grid): which widget, and where/how big it is.
/// </summary>
public sealed class WidgetPlacement
{
    public required string WidgetId { get; init; }
    public required string OwnerModuleId { get; init; }
    public int Column { get; set; }
    public int Row { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}
