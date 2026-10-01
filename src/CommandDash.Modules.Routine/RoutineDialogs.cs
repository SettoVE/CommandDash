using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CommandDash.Modules.Routine;

internal static class RoutineDialogs
{
    public static string? PromptCategory(Window? owner)
    {
        var box = new TextBox { Margin = new Thickness(0, 4, 0, 12), MinWidth = 260 };
        var window = CreateWindow(owner, "Add Category", out var panel);
        panel.Children.Add(Label("Name"));
        panel.Children.Add(box);
        return Show(window, panel, box, () => box.Text.Trim()) is { Length: > 0 } name ? name : null;
    }

    public static (RoutineCategory Category, string Name)? PromptItem(
        Window? owner, ObservableCollection<RoutineCategory> categories, RoutineCategory? preselect)
    {
        var combo = new ComboBox
        {
            ItemsSource = categories,
            DisplayMemberPath = nameof(RoutineCategory.Name),
            SelectedItem = preselect ?? categories.FirstOrDefault(),
            Margin = new Thickness(0, 4, 0, 8),
            MinWidth = 260,
        };
        var box = new TextBox { Margin = new Thickness(0, 4, 0, 12) };
        var window = CreateWindow(owner, "Add Item", out var panel);
        panel.Children.Add(Label("Category"));
        panel.Children.Add(combo);
        panel.Children.Add(Label("Name"));
        panel.Children.Add(box);

        var name = Show(window, panel, box, () => box.Text.Trim());
        if (string.IsNullOrEmpty(name) || combo.SelectedItem is not RoutineCategory category)
        {
            return null;
        }

        return (category, name);
    }

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextPrimaryBrush"),
    };

    private static Window CreateWindow(Window? owner, string title, out StackPanel panel)
    {
        panel = new StackPanel { Margin = new Thickness(16) };
        var window = new Window
        {
            Title = title,
            Owner = owner,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Content = panel,
        };
        window.SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");
        return window;
    }

    private static string? Show(Window window, StackPanel panel, TextBox focus, Func<string> read)
    {
        var ok = new Button { Content = "OK", IsDefault = true, MinWidth = 70, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 70 };
        ok.SetResourceReference(FrameworkElement.StyleProperty, "PrimaryButtonStyle");
        ok.Click += (_, _) => window.DialogResult = true;
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { ok, cancel },
        });
        window.Loaded += (_, _) => focus.Focus();
        return window.ShowDialog() == true ? read() : null;
    }
}
