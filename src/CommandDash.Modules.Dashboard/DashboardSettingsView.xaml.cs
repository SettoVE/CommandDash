using System.Windows;
using System.Windows.Controls;

namespace CommandDash.Modules.Dashboard;

public partial class DashboardSettingsView : UserControl
{
    private readonly DashboardSettings _settings;

    internal DashboardSettingsView(DashboardSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        HorizontalCombo.SelectedIndex = _settings.HorizontalAlignment switch
        {
            HorizontalAlignment.Center => 1,
            HorizontalAlignment.Right => 2,
                        HorizontalAlignment.Stretch => 3,
            _ => 0,
        };
        VerticalCombo.SelectedIndex = _settings.VerticalAlignment switch
        {
            VerticalAlignment.Center => 1,
            VerticalAlignment.Bottom => 2,
                        VerticalAlignment.Stretch => 3,
            _ => 0,
        };
    }

    private void Horizontal_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (HorizontalCombo.SelectedItem is ComboBoxItem { Tag: string tag }
            && Enum.TryParse<HorizontalAlignment>(tag, out var value)
            && value != _settings.HorizontalAlignment)
            _settings.HorizontalAlignment = value;
    }

    private void Vertical_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (VerticalCombo.SelectedItem is ComboBoxItem { Tag: string tag }
            && Enum.TryParse<VerticalAlignment>(tag, out var value)
            && value != _settings.VerticalAlignment)
            _settings.VerticalAlignment = value;
    }
}
