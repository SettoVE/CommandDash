using System.Windows.Input;

namespace CommandDash.Modules.RunScript;

/// <summary>A key plus any combination of Ctrl/Alt/Shift used to launch a script.</summary>
internal readonly record struct ScriptHotkey(ModifierKeys Modifiers, Key Key)
{
    private const ModifierKeys Supported = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift;

    public static ModifierKeys CurrentModifiers => Keyboard.Modifiers & Supported;

    public static bool IsModifierKey(Key key)
        => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin or Key.System or Key.None;

    public static bool IsNumberKey(Key key)
        => key is >= Key.D0 and <= Key.D9 || key is >= Key.NumPad0 and <= Key.NumPad9;

    /// <summary>Builds a hotkey from a key event, or null when only a modifier was pressed.</summary>
    public static ScriptHotkey? FromEvent(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.ImeProcessed)
        {
            key = e.ImeProcessedKey;
        }

        return IsModifierKey(key) ? null : new ScriptHotkey(CurrentModifiers, key);
    }

    /// <summary>True when the hotkey may be bound (not Escape, F1, or a number key on its own).</summary>
    public bool IsAllowed
        => Key != Key.Escape
            && !(Modifiers == ModifierKeys.None && (IsNumberKey(Key) || Key == Key.F1));

    public override string ToString()
    {
        var parts = new List<string>();
        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        parts.Add(Key is >= Key.D0 and <= Key.D9 ? ((char)('0' + (Key - Key.D0))).ToString() : Key.ToString());
        return string.Join("+", parts);
    }

    public static bool TryParse(string? text, out ScriptHotkey hotkey)
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
        if (keyText.Length == 1 && char.IsDigit(keyText[0]))
        {
            keyText = "D" + keyText;
        }

        if (!Enum.TryParse<Key>(keyText, out var key))
        {
            return false;
        }

        hotkey = new ScriptHotkey(modifiers, key);
        return true;
    }
}
