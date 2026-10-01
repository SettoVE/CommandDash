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

    /// <summary>Number of equal-width columns in the widget grid.</summary>
    public const int ColumnCount = 2;

    public void AddWidget(string title, object content, WidgetGridPosition placement)
    {
        if (WidgetGrid.ColumnDefinitions.Count == 0)
        {
            for (var c = 0; c < ColumnCount; c++)
                WidgetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        var lastRow = placement.Row + placement.RowSpan - 1;
        while (WidgetGrid.RowDefinitions.Count <= lastRow)
            WidgetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var column = Math.Clamp(placement.Column, 0, ColumnCount - 1);
        var card = CreateCard(title, content);
        Grid.SetRow(card, placement.Row);
        Grid.SetColumn(card, column);
        Grid.SetColumnSpan(card, Math.Clamp(placement.ColumnSpan, 1, ColumnCount - column));
        Grid.SetRowSpan(card, placement.RowSpan);
        WidgetGrid.Children.Add(card);
    }

    private static Border CreateCard(string title, object content)
    {
        var header = new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(12, 8, 12, 0) };        header.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition());
        grid.Children.Add(header);
        var body = (UIElement)content;
        Grid.SetRow(body, 1);
        grid.Children.Add(body);

        var card = new Border
        {
            Height = CellSize,
            Margin = new Thickness(0, 0, 8, 8),
            ClipToBounds = true,
        };
        card.SizeChanged += (_, e) =>
            card.Clip = new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), CornerRadius, CornerRadius);

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
        return card;
    }
}
