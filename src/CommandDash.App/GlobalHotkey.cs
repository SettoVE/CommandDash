using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CommandDash.App.Settings;

namespace CommandDash.App;

/// <summary>
/// Registers a system-wide hotkey that brings the window to the foreground.
/// Re-registers whenever <see cref="GeneralSettings"/> change.
/// </summary>
public sealed class GlobalHotkey : IDisposable
{
    private const int HotkeyId = 0x0C0D;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;

    private readonly Window _window;
    private HwndSource? _source;
    private bool _registered;

    public GlobalHotkey(Window window)
    {
        _window = window;
    }

    public void Attach()
    {
        var hwnd = new WindowInteropHelper(_window).Handle;
        _source = HwndSource.FromHwnd(hwnd);
        _source?.AddHook(WndProc);
        GeneralSettings.Changed += OnSettingsChanged;
        Register();
    }

    public void Dispose()
    {
        GeneralSettings.Changed -= OnSettingsChanged;
        Unregister();
        _source?.RemoveHook(WndProc);
        _source = null;
    }

    private void OnSettingsChanged(object? sender, EventArgs e) => Register();

    private void Register()
    {
        Unregister();
        if (_source is null)
        {
            return;
        }

        var settings = GeneralSettings.Load();
        if (settings.GetHotkey() is not { } hotkey)
        {
            return;
        }

        uint modifiers = ModNoRepeat;
        if (hotkey.Modifiers.HasFlag(ModifierKeys.Control)) modifiers |= ModControl;
        if (hotkey.Modifiers.HasFlag(ModifierKeys.Alt)) modifiers |= ModAlt;
        if (hotkey.Modifiers.HasFlag(ModifierKeys.Shift)) modifiers |= ModShift;

        var vk = (uint)KeyInterop.VirtualKeyFromKey(hotkey.Key);
        _registered = RegisterHotKey(_source.Handle, HotkeyId, modifiers, vk);
    }

    private void Unregister()
    {
        if (_registered && _source is not null)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
        }

        _registered = false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            Bring();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void Bring()
    {
        if (!_window.IsVisible)
        {
            _window.Show();
        }

        if (_window.WindowState == WindowState.Minimized)
        {
            _window.WindowState = WindowState.Normal;
        }

        _window.Activate();
        _window.Topmost = true;
        _window.Topmost = false;
        _window.Focus();
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
