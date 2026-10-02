using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace CommandDash.Modules.RunScript;

internal sealed class ScriptItem : INotifyPropertyChanged
{
    private ScriptHotkey? _hotkey;
    private string _status = string.Empty;
    private bool _isRunning;

    public ScriptItem(string fullPath)
    {
        FullPath = fullPath;
        Name = Path.GetFileName(fullPath);
    }

    public string FullPath { get; }

    public string Name { get; }

    public ScriptHotkey? Hotkey
    {
        get => _hotkey;
        set
        {
            _hotkey = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HotkeyText));
        }
    }

    public string HotkeyText => _hotkey?.ToString() ?? string.Empty;

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatus));
        }
    }

    public bool HasStatus => _status.Length > 0;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
