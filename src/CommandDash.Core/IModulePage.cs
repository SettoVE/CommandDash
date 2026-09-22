namespace CommandDash.Core;

/// <summary>
/// Represents the UI page contributed by a module. The host (WPF shell) will
/// host <see cref="View"/> inside its content area.
/// </summary>
public interface IModulePage
{
    /// <summary>
    /// The framework-specific view object (e.g. a WPF UserControl instance).
    /// Typed as object so CommandDash.Core has no dependency on WPF.
    /// </summary>
    object View { get; }

    /// <summary>
    /// Called when the page becomes visible in the shell.
    /// </summary>
    void OnNavigatedTo();

    /// <summary>
    /// Called when the user navigates away from the page.
    /// </summary>
    void OnNavigatedFrom();
}
