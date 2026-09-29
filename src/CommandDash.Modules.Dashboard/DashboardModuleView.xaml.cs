using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public partial class DashboardModuleView : UserControl
{
    private const double CellSize = 160;
    private const double CornerRadius = 8;

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
            ClipToBounds = true,
        };
        card.Clip = new RectangleGeometry(new Rect(0, 0, card.Width, card.Height), CornerRadius, CornerRadius);

        var layers = new Grid();

        var glass = new Border
        {
            IsHitTestVisible = false,
            CornerRadius = new CornerRadius(CornerRadius),
            BorderThickness = new Thickness(1),
        };
        glass.SetResourceReference(Border.BackgroundProperty, "WidgetGlassBrush");
        glass.SetResourceReference(Border.BorderBrushProperty, "WidgetGlassBorderBrush");

        var sheen = new Border { IsHitTestVisible = false, CornerRadius = new CornerRadius(CornerRadius) };
        sheen.SetResourceReference(Border.BackgroundProperty, "WidgetGlassSheenBrush");

        layers.Children.Add(glass);
        layers.Children.Add(sheen);
        layers.Children.Add(grid);

        card.Child = layers;
        WidgetPanel.Children.Add(card);
    }
}
