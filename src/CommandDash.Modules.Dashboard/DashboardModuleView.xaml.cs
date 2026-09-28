using System.Windows;
using System.Windows.Controls;
using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public partial class DashboardModuleView : UserControl
{
    private const double CellSize = 160;

    public DashboardModuleView()
    {
        InitializeComponent();
    }

    public void AddWidget(string title, WidgetSize size, object content)
    {
        var (cols, rows) = size switch
        {
            WidgetSize.Small => (1, 1),
            WidgetSize.Medium => (2, 1),
            WidgetSize.Large => (2, 2),
            WidgetSize.Wide => (4, 1),
            _ => (2, 1),
        };

        var header = new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(12, 8, 12, 0) };
        header.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition());
        grid.Children.Add(header);
        var body = (UIElement)content;
        Grid.SetRow(body, 1);
        grid.Children.Add(body);

        var card = new Border
        {
            Width = cols * CellSize,
            Height = rows * CellSize,
            Margin = new Thickness(0, 0, 8, 8),
            CornerRadius = new CornerRadius(8),
            Child = grid,
        };
        card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
        WidgetPanel.Children.Add(card);
    }
}
