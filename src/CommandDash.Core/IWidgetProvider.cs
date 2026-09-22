namespace CommandDash.Core;

/// <summary>
/// Optional interface a module can implement, in addition to <see cref="IModule"/>,
/// to contribute one or more widgets to the Home dashboard (or other
/// widget-hosting surfaces) without those widgets requiring their own
/// sidebar entry.
/// </summary>
public interface IWidgetProvider
{
    /// <summary>
    /// Creates the set of widgets this module wants to make available.
    /// Called once after the owning module's <see cref="IModule.OnLoaded"/>
    /// has completed. The host decides which of these (if any) are actually
    /// placed on the dashboard, based on saved layout / user choice.
    /// </summary>
    IReadOnlyList<IWidget> CreateWidgets();
}
