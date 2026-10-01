using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace CommandDash.App;

/// <summary>
/// Colors a window's native title bar to match the app theme using the DWM
/// caption attributes (Windows 11 22000+). On older Windows versions the
/// unsupported attributes are ignored and the default title bar is kept.
/// </summary>
internal static class TitleBarTheme
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static void Apply(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            var dark = 1;
            DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));

            if (Application.Current.TryFindResource("BackgroundColor") is Color background)
            {
                var caption = ToColorRef(background);
                DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref caption, sizeof(int));
            }

            if (Application.Current.TryFindResource("TextPrimaryColor") is Color text)
            {
                var textColor = ToColorRef(text);
                DwmSetWindowAttribute(handle, DwmwaTextColor, ref textColor, sizeof(int));
            }
        };
    }

    // COLORREF is 0x00BBGGRR.
    private static int ToColorRef(Color color) => color.R | (color.G << 8) | (color.B << 16);
}
