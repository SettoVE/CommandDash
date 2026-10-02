using System.Windows.Input;

namespace CommandDash.App.Settings;

/// <summary>A key plus any combination of Ctrl/Alt/Shift used as the global focus hotkey.</summary>
public readonly record struct AppHotkey(ModifierKeys Modifiers, Key Key)
{
    private const ModifierKeys Supported = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift;

    public static AppHotkey Default { get; } = new(ModifierKeys.Alt, Key.Oem3);

    public static bool IsModifierKey(Key key)
        => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin or Key.System or Key.None;

    /// <summary>Builds a hotkey from a key event, or null when only a modifier was pressed.</summary>
    public static AppHotkey? FromEvent(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.ImeProcessed)
        {
            key = e.ImeProcessedKey;
        }

        return IsModifierKey(key) ? null : new AppHotkey(Keyboard.Modifiers & Supported, key);
    }

    /// <summary>True when the hotkey may be used globally (has a modifier and is not Escape).</summary>
    public bool IsAllowed => Key != Key.Escape && Modifiers != ModifierKeys.None;

    public override string ToString()
    {
        var parts = new List<string>();
        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        parts.Add(Key switch
        {
            Key.Oem3 => "`",
            >= Key.D0 and <= Key.D9 => ((char)('0' + (Key - Key.D0))).ToString(),
            _ => Key.ToString(),
        });
        return string.Join("+", parts);
    }

    public static bool TryParse(string? text, out AppHotkey hotkey)
    {
        hotkey = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split('+', StringSplitOptions.TrimEntries);
        var modifiers = ModifierKeys.None;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            switch (parts[i])
            {
                case "Ctrl": modifiers |= ModifierKeys.Control; break;
                case "Alt": modifiers |= ModifierKeys.Alt; break;
                case "Shift": modifiers |= ModifierKeys.Shift; break;
                default: return false;
            }
        }

        var keyText = parts[^1];
        Key key;
        if (keyText == "`")
        {
            key = Key.Oem3;
        }
        else
        {
            if (keyText.Length == 1 && char.IsDigit(keyText[0]))
            {
                keyText = "D" + keyText;
            }

            if (!Enum.TryParse(keyText, out key))
            {
                return false;
            }
        }

        hotkey = new AppHotkey(modifiers, key);
        return true;
    }
}
