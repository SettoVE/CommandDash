using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommandDash.Core;

namespace CommandDash.Modules.Dashboard;

public partial class DashboardModuleView : UserControl
{
    private const double CellSize = 160;
    private const double CornerRadius = 8;

    private const double MinWidgetWidth = 250;
    private const double MaxWidgetWidth = 400;
    private const double CardGap = 8;
    private const double RootMargin = 8;

    /// <summary>Smallest width this view needs to show one widget, including margins.</summary>
    public const double MinContentWidth = 2 * RootMargin + MinWidgetWidth + CardGap;

    private readonly List<(Border Card, WidgetGridPosition Placement)> _widgets = new();
    private bool? _singleColumn;

    public DashboardModuleView()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateLayout(false);
        Loaded += (_, _) => ApplyWindowMinWidth();
        Unloaded += (_, _) => RestoreWindowMinWidth();
    }

    private Window? _window;
    private double _originalMinWidth;

    private void ApplyWindowMinWidth()
    {
        _window = Window.GetWindow(this);
        if (_window is null || ActualWidth <= 0) return;
        _originalMinWidth = _window.MinWidth;
        // Everything outside this view (sidebar, margins, window frame) stays constant.
        var outside = _window.ActualWidth - ActualWidth;
        _window.MinWidth = Math.Max(_originalMinWidth, outside + MinContentWidth);
    }

    private void RestoreWindowMinWidth()
    {
        if (_window is not null) _window.MinWidth = _originalMinWidth;
        _window = null;
    }

    /// <summary>Number of equal-width columns in the widget grid.</summary>
    public const int ColumnCount = 2;

    public void AddWidget(string title, object content, WidgetGridPosition placement, string? subtitle = null)
    {
        if (WidgetGrid.ColumnDefinitions.Count == 0)
        {
            for (var c = 0; c < ColumnCount; c++)
                WidgetGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star),
                    MinWidth = MinWidgetWidth + CardGap,
                    MaxWidth = MaxWidgetWidth + CardGap,
                });
        }

        var card = CreateCard(title, content, subtitle);
        _widgets.Add((card, placement));
        WidgetGrid.Children.Add(card);
        UpdateLayout(true);
    }

    private void UpdateLayout(bool force)
    {
        var single = ActualWidth > 0 && ActualWidth - 2 * RootMargin < ColumnCount * (MinWidgetWidth + CardGap);
        if (!force && _singleColumn == single) return;
        _singleColumn = single;

        WidgetGrid.RowDefinitions.Clear();
        var columns = WidgetGrid.ColumnDefinitions;
        columns[0].MaxWidth = single ? double.PositiveInfinity : MaxWidgetWidth + CardGap;
        for (var c = 1; c < columns.Count; c++)
        {
            columns[c].MinWidth = single ? 0 : MinWidgetWidth + CardGap;
            columns[c].MaxWidth = single ? 0 : MaxWidgetWidth + CardGap;
        }

        var ordered = single
            ? _widgets.OrderBy(w => w.Placement.Row).ThenBy(w => w.Placement.Column).ToList()
            : _widgets;

        var nextRow = 0;
        foreach (var (card, placement) in ordered)
        {
            var row = single ? nextRow : placement.Row;
            var rowSpan = single ? 1 : placement.RowSpan;
            var column = single ? 0 : Math.Clamp(placement.Column, 0, ColumnCount - 1);
            var columnSpan = single ? 1 : Math.Clamp(placement.ColumnSpan, 1, ColumnCount - column);
            nextRow++;

            while (WidgetGrid.RowDefinitions.Count < row + rowSpan)
                WidgetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(card, row);
            card.MaxWidth = single ? MaxWidgetWidth : double.PositiveInfinity;
            card.Margin = single ? new Thickness(CardGap / 2, 0, CardGap / 2, CardGap) : new Thickness(0, 0, CardGap, CardGap);
            Grid.SetColumn(card, column);
            Grid.SetColumnSpan(card, columnSpan);
            Grid.SetRowSpan(card, rowSpan);
        }
    }

    private static Border CreateCard(string title, object content, string? subtitle)
    {
        var titleText = new TextBlock { Text = title, FontWeight = FontWeights.SemiBold };
        titleText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var header = new Grid { Margin = new Thickness(12, 8, 12, 0) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.Children.Add(titleText);
        if (!string.IsNullOrEmpty(subtitle))
        {
            var subtitleText = new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Margin = new Thickness(12, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = subtitle,
            };
            subtitleText.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");
            Grid.SetColumn(subtitleText, 1);
            header.Children.Add(subtitleText);
        }

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
