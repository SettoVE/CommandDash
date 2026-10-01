using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CommandDash.Modules.Routine;

public partial class RoutineModuleView : UserControl
{
    private readonly RoutineStore _store;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    public RoutineModuleView(RoutineStore store)
    {
        _store = store;
        InitializeComponent();
        CategoryList.ItemsSource = store.Categories;

        _timer.Tick += (_, _) => _store.Tick();
        Loaded += (_, _) =>
        {
            _store.Tick();
            _timer.Start();
        };
        Unloaded += (_, _) => _timer.Stop();
    }

    private void Complete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: RoutineItem item })
        {
            item.LastCompleted = DateTime.Now;
            _store.Save();
        }
    }
}
