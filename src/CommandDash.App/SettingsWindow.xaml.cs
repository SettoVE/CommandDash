using System.Windows;
using System.Windows.Controls;
using CommandDash.Core;

namespace CommandDash.App;

/// <summary>
/// Generic settings shell: a <see cref="TreeView"/> of
/// <see cref="ISettingsNode"/>s (foobar2000-style preferences tree) and a
/// content area showing the selected node's page. There are no tabs —
/// nesting is expressed purely via the tree.
///
/// Adding a new built-in node only requires passing another
/// <see cref="ISettingsNode"/> instance into the constructor; this window
/// never needs to change.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly List<ISettingsNode> _rootNodes;

    public SettingsWindow(IEnumerable<ISettingsNode> rootNodes)
    {
        InitializeComponent();

        _rootNodes = rootNodes.ToList();
        BuildTree();
    }

    private void BuildTree()
    {
        SectionTree.Items.Clear();

        foreach (var node in _rootNodes)
        {
            SectionTree.Items.Add(BuildTreeItem(node));
        }

        ExpandAll(SectionTree.Items);

        var firstItem = FindFirstTreeViewItem(SectionTree.Items);
        if (firstItem is not null)
        {
            firstItem.IsSelected = true;
        }
    }

    private TreeViewItem BuildTreeItem(ISettingsNode node)
    {
        var item = new TreeViewItem
        {
            Header = node.DisplayName,
            Tag = node,
        };

        foreach (var child in node.Children)
        {
            item.Items.Add(BuildTreeItem(child));
        }

        return item;
    }

    private static void ExpandAll(System.Collections.IEnumerable items)
    {
        foreach (var obj in items)
        {
            if (obj is TreeViewItem item)
            {
                item.IsExpanded = true;
                ExpandAll(item.Items);
            }
        }
    }

    private static TreeViewItem? FindFirstTreeViewItem(System.Collections.IEnumerable items)
    {
        foreach (var obj in items)
        {
            if (obj is TreeViewItem item)
            {
                return item;
            }
        }

        return null;
    }

    private void SectionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not TreeViewItem { Tag: ISettingsNode node })
        {
            return;
        }

        SectionTitle.Text = node.DisplayName;
        SectionContentHost.Content = node.CreateContent();
    }
}
