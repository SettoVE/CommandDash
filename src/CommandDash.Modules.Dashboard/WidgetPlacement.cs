namespace CommandDash.Modules.Dashboard;

/// <summary>
/// Position of a widget on the dashboard's column grid. A future configurable layout
/// can supply these per widget id instead of the default left-to-right flow.
/// </summary>
public readonly record struct WidgetPlacement(int Row, int Column, int ColumnSpan = 1, int RowSpan = 1);
