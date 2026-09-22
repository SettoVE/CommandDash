namespace CommandDash.Core;

/// <summary>
/// A small, self-contained UI component that can be placed on the Home
/// dashboard (or any other widget-hosting surface). Unlike <see cref="IModule"/>,
/// a widget does not own a sidebar entry or the full content area — multiple
/// widgets are typically arranged together on one page.
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Stable, unique identifier for this widget (e.g. "commanddash.network.speed").
    /// Used to persist layout (position/size) between sessions.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable title shown in the widget's header.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Preferred size for this widget when first added to a dashboard.
    /// The host may allow the user to resize it afterwards if
    /// <see cref="IsResizable"/> is true.
    /// </summary>
    WidgetSize PreferredSize { get; }

    /// <summary>
    /// Whether the user is allowed to resize this widget on the dashboard.
    /// </summary>
    bool IsResizable { get; }

    /// <summary>
    /// The framework-specific view object (e.g. a WPF UserControl instance)
    /// rendered inside the widget's card/frame on the dashboard.
    /// </summary>
    object CreateView();

    /// <summary>
    /// Called when the widget is added to a dashboard and becomes visible.
    /// Use this to start timers, subscribe to events, begin polling, etc.
    /// </summary>
    void OnActivated();

    /// <summary>
    /// Called when the widget is removed from the dashboard or the dashboard
    /// is unloaded. Use this to release resources / stop timers.
    /// </summary>
    void OnDeactivated();
}

/// <summary>
/// Coarse size classes for widgets, similar to Windows 11 widget/tile sizing.
/// The host maps these to actual grid cell spans.
/// </summary>
public enum WidgetSize
{
    Small,   // 1x1
    Medium,  // 2x1
    Large,   // 2x2
    Wide,    // 4x1
}
