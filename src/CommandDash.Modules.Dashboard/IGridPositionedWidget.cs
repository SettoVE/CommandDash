namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Optional interface a widget can implement to request a specific spot on the dashboard grid.
/// Widgets that don't implement it (or return null) flow left-to-right into free rows.
/// </summary>
public interface IGridPositionedWidget
{
    WidgetGridPosition? DefaultPosition { get; }
}
