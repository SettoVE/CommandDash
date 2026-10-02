using System.Globalization;
using System.IO;
using System.Text;

namespace CommandDash.App.Settings;

/// <summary>
/// Application configuration stored in config.ini next to the exe. The file is created with
/// default values on first run and can be edited by hand while the app is closed.
/// </summary>
public sealed class AppConfig
{
    private const string DefaultTitle = "CommandDash";
    private const double DefaultWidth = 1040;
    private const double DefaultHeight = 680;

    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "config.ini");

    /// <summary>Text shown in the main window's title bar.</summary>
    public string Title { get; set; } = DefaultTitle;

    /// <summary>Main window width, in device-independent pixels. Saved on exit.</summary>
    public double Width { get; set; } = DefaultWidth;

    /// <summary>Main window height, in device-independent pixels. Saved on exit.</summary>
    public double Height { get; set; } = DefaultHeight;

    /// <summary>Main window left edge. Null (default) centers the window on screen. Saved on exit.</summary>
    public double? Left { get; set; }

    /// <summary>Main window top edge. Null (default) centers the window on screen. Saved on exit.</summary>
    public double? Top { get; set; }

    /// <summary>
    /// Reads the config file, creating it with default values if it does not exist.
    /// Missing or invalid values fall back to their defaults.
    /// </summary>
    public static AppConfig Load()
    {
        var config = new AppConfig();
        try
        {
            if (File.Exists(FilePath))
            {
                config.Parse(File.ReadAllLines(FilePath));
            }
            else
            {
                config.Write();
            }
        }
        catch (Exception)
        {
            // Unreadable file: use defaults without overwriting it.
            config = new AppConfig();
        }

        config.Normalize();
        return config;
    }

    /// <summary>
    /// Saves the current window bounds, keeping any other values (e.g. a title edited by hand) as they are on disk.
    /// </summary>
    public static void SaveWindowBounds(double left, double top, double width, double height)
    {
        var config = Load();
        config.Left = left;
        config.Top = top;
        config.Width = width;
        config.Height = height;
        config.Normalize();
        config.Write();
    }

    private void Parse(IEnumerable<string> lines)
    {
        var section = string.Empty;
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] is ';' or '#') continue;

            if (line[0] == '[' && line[^1] == ']')
            {
                section = line[1..^1].Trim();
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0 || !IsKnownSection(section)) continue;

            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();
            switch (key.ToLowerInvariant())
            {
                case "title": Title = value; break;
                case "width": Width = ParseDouble(value) ?? DefaultWidth; break;
                case "height": Height = ParseDouble(value) ?? DefaultHeight; break;
                case "left": Left = ParseDouble(value); break;
                case "top": Top = ParseDouble(value); break;
            }
        }
    }

    private static bool IsKnownSection(string section)
        => section.Equals("Window", StringComparison.OrdinalIgnoreCase)
           || section.Equals("Resolution", StringComparison.OrdinalIgnoreCase)
           || section.Equals("Position", StringComparison.OrdinalIgnoreCase);

    private static double? ParseDouble(string value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) && double.IsFinite(d) ? d : null;

    private void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            Title = DefaultTitle;
        }

        if (!double.IsFinite(Width) || Width <= 0)
        {
            Width = DefaultWidth;
        }

        if (!double.IsFinite(Height) || Height <= 0)
        {
            Height = DefaultHeight;
        }

        if (Left is null || Top is null)
        {
            Left = null;
            Top = null;
        }
    }

    private void Write()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Window]");
            sb.AppendLine($"Title={Title}");
            sb.AppendLine("[Resolution]");
            sb.AppendLine($"Width={Format(Width)}");
            sb.AppendLine($"Height={Format(Height)}");
            sb.AppendLine("[Position]");
            sb.AppendLine($"Left={(Left is { } l ? Format(l) : string.Empty)}");
            sb.AppendLine($"Top={(Top is { } t ? Format(t) : string.Empty)}");
            File.WriteAllText(FilePath, sb.ToString());
        }
        catch (Exception)
        {
            // Read-only or protected location: never block startup or closing.
        }
    }

    private static string Format(double value) => Math.Round(value).ToString(CultureInfo.InvariantCulture);
}
