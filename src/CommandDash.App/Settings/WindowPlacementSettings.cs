using System.IO;
using System.Text.Json;
using System.Windows;

namespace CommandDash.App.Settings;

/// <summary>
/// Persists the main window's position, size and state as JSON under
/// %AppData%\CommandDash\window.json.
/// </summary>
public sealed class WindowPlacementSettings
{
    private readonly string _filePath;

    public WindowPlacementSettings()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CommandDash");
        _filePath = Path.Combine(settingsDirectory, "window.json");
    }

    public void Restore(Window window)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            var data = JsonSerializer.Deserialize<PlacementData>(File.ReadAllText(_filePath));
            if (data is null || data.Width < window.MinWidth || data.Height < window.MinHeight
                || data.Width <= 0 || data.Height <= 0)
            {
                return;
            }

            var saved = new Rect(data.Left, data.Top, data.Width, data.Height);
            var screen = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            // Ignore saved placement if the window would be off-screen (e.g. monitor removed).
            var visible = Rect.Intersect(saved, screen);
            if (visible.IsEmpty || visible.Width < 100 || visible.Height < 50)
            {
                return;
            }

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = data.Left;
            window.Top = data.Top;
            window.Width = data.Width;
            window.Height = data.Height;

            if (data.Maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }
        catch (Exception)
        {
            // Corrupt or unreadable file: fall back to default placement.
        }
    }

    public void Save(Window window)
    {
        try
        {
            var bounds = window.WindowState == WindowState.Normal ? new Rect(window.Left, window.Top, window.Width, window.Height) : window.RestoreBounds;
            var data = new PlacementData
            {
                Left = bounds.Left,
                Top = bounds.Top,
                Width = bounds.Width,
                Height = bounds.Height,
                Maximized = window.WindowState == WindowState.Maximized,
            };

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(data));
        }
        catch (Exception)
        {
            // Failing to persist window placement must never block closing.
        }
    }

    private sealed class PlacementData
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Maximized { get; set; }
    }
}
