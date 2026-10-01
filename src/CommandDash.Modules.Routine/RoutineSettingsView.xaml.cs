using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CommandDash.Modules.Routine;

public partial class RoutineSettingsView : UserControl
{
    private readonly RoutineStore _store;
    private object? _selected;

    public RoutineSettingsView(RoutineStore store)
    {
        _store = store;
        InitializeComponent();

        var baseStyle = TryFindResource("SettingsTreeItemStyle") as Style;

        var categoryStyle = new Style(typeof(TreeViewItem), baseStyle);
        categoryStyle.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty,
            new Binding(nameof(RoutineCategory.IsExpanded)) { Mode = BindingMode.TwoWay }));
        categoryStyle.Setters.Add(new Setter(TreeViewItem.IsSelectedProperty,
            new Binding(nameof(RoutineCategory.IsSelected)) { Mode = BindingMode.TwoWay }));

        var itemStyle = new Style(typeof(TreeViewItem), baseStyle);
        itemStyle.Setters.Add(new Setter(TreeViewItem.IsSelectedProperty,
            new Binding(nameof(RoutineItem.IsSelected)) { Mode = BindingMode.TwoWay }));

        Tree.ItemContainerStyle = categoryStyle;
        if (Resources[new DataTemplateKey(typeof(RoutineCategory))] is HierarchicalDataTemplate template)
        {
            template.ItemContainerStyle = itemStyle;
        }

        Tree.ItemsSource = store.Categories;
    }

    private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        => _selected = e.NewValue;

    private RoutineCategory? CategoryOf(RoutineItem item)
        => _store.Categories.FirstOrDefault(c => c.Items.Contains(item));

    private void AddCategory_Click(object sender, RoutedEventArgs e)
    {
        var name = RoutineDialogs.PromptCategory(Window.GetWindow(this));
        if (name is null)
        {
            return;
        }

        var category = new RoutineCategory { Name = name };
        _store.Categories.Add(category);
        _store.Save();
        category.IsSelected = true;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (_store.Categories.Count == 0)
        {
            MessageBox.Show(Window.GetWindow(this), "Add a category first.", "Add Item",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var preselect = _selected switch
        {
            RoutineCategory c => c,
            RoutineItem i => CategoryOf(i),
            _ => null,
        };

        var result = RoutineDialogs.PromptItem(Window.GetWindow(this), _store.Categories, preselect);
        if (result is null)
        {
            return;
        }

        var item = new RoutineItem { Name = result.Value.Name, LastCompleted = DateTime.Now };
        result.Value.Category.Items.Add(item);
        result.Value.Category.IsExpanded = true;
        _store.Save();
        item.IsSelected = true;
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        switch (_selected)
        {
            case RoutineCategory category:
                var answer = MessageBox.Show(Window.GetWindow(this),
                    $"Delete category \"{category.Name}\" and all of its routine items?",
                    "Delete Category", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (answer == MessageBoxResult.Yes)
                {
                    _store.Categories.Remove(category);
                    _selected = null;
                    _store.Save();
                }
                break;
            case RoutineItem item:
                CategoryOf(item)?.Items.Remove(item);
                _selected = null;
                _store.Save();
                break;
        }
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e) => Move(-1);

    private void MoveDown_Click(object sender, RoutedEventArgs e) => Move(1);

    private void Move(int delta)
    {
        switch (_selected)
        {
            case RoutineCategory category:
                var ci = _store.Categories.IndexOf(category);
                var cn = ci + delta;
                if (ci >= 0 && cn >= 0 && cn < _store.Categories.Count)
                {
                    _store.Categories.Move(ci, cn);
                    _store.Save();
                    category.IsSelected = true;
                }
                break;
            case RoutineItem item:
                var owner = CategoryOf(item);
                if (owner is null)
                {
                    return;
                }
                var ii = owner.Items.IndexOf(item);
                var inew = ii + delta;
                if (inew >= 0 && inew < owner.Items.Count)
                {
                    owner.Items.Move(ii, inew);
                    _store.Save();
                    item.IsSelected = true;
                }
                break;
        }
    }
}
