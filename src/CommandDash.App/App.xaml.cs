using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CommandDash.App;

public partial class App : Application
{
    private static BitmapFrame? _customIcon;

    public App()
    {
        TitleBarTheme.RegisterForAllWindows();
        LoadCustomIcon();
    }

    /// <summary>
    /// Checked once at startup: if app.ico exists next to the exe, it becomes the icon of every
    /// window; otherwise the default icon is left untouched.
    /// </summary>
    private static void LoadCustomIcon()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "app.ico");
            if (!File.Exists(path)) return;

            using var stream = File.OpenRead(path);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            _customIcon = decoder.Frames[0];
            _customIcon.Freeze();
        }
        catch
        {
            _customIcon = null;
            return;
        }

        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler((s, _) =>
        {
            if (s is Window { Icon: null } window) window.Icon = _customIcon;
        }));
    }
}
