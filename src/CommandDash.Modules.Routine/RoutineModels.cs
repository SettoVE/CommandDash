using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommandDash.Modules.Routine;

public abstract class ObservableBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class RoutineItem : ObservableBase
{
    private string _name = string.Empty;
    private DateTime _lastCompleted = DateTime.Now;
    private string _elapsedText = string.Empty;
    private bool _isSelected;

    public string Name
    {
        get => _name;
        set { _name = value; Raise(); }
    }

    public DateTime LastCompleted
    {
        get => _lastCompleted;
        set { _lastCompleted = value; Raise(); Refresh(); }
    }

    [JsonIgnore]
    public string ElapsedText
    {
        get
        {
            if (_elapsedText.Length == 0)
            {
                _elapsedText = Format(DateTime.Now - _lastCompleted);
            }
            return _elapsedText;
        }
    }

    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; Raise(); }
    }

    public void Refresh()
    {
        var text = Format(DateTime.Now - _lastCompleted);
        if (text != _elapsedText)
        {
            _elapsedText = text;
            Raise(nameof(ElapsedText));
        }
    }

    private static string Format(TimeSpan span)
    {
        if (span < TimeSpan.Zero)
        {
            span = TimeSpan.Zero;
        }

        if (span.Days > 0)
        {
            return $"{span.Days}d {span.Hours}h {span.Minutes}m";
        }

        if (span.Hours > 0)
        {
            return $"{span.Hours}h {span.Minutes}m";
        }

        return $"{span.Minutes}m";
    }
}

public sealed class RoutineCategory : ObservableBase
{
    private string _name = string.Empty;
    private bool _isSelected;
    private bool _isExpanded = true;

    public string Name
    {
        get => _name;
        set { _name = value; Raise(); }
    }

    public ObservableCollection<RoutineItem> Items { get; set; } = new();

    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; Raise(); }
    }

    [JsonIgnore]
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; Raise(); }
    }
}

public sealed class RoutineStore
{
    private readonly string _path;

    public RoutineStore(string directory)
    {
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "routines.json");
        Load();
    }

    public ObservableCollection<RoutineCategory> Categories { get; private set; } = new();

    private void Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var loaded = JsonSerializer.Deserialize<ObservableCollection<RoutineCategory>>(File.ReadAllText(_path));
                if (loaded is not null)
                {
                    Categories = loaded;
                }
            }
        }
        catch (Exception)
        {
            Categories = new ObservableCollection<RoutineCategory>();
        }
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(Categories, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (IOException)
        {
        }
    }

    public void Tick()
    {
        foreach (var category in Categories)
        {
            foreach (var item in category.Items)
            {
                item.Refresh();
            }
        }
    }
}
