using System.Windows;
using System.Windows.Controls;

namespace CommandDash.Modules.Sample;

public partial class SampleModuleView : UserControl
{
    private int _clickCount;

    public SampleModuleView()
    {
        InitializeComponent();
    }

    public void OnShown()
    {
        // Called whenever the shell navigates to this module's page.
    }

    public void OnHidden()
    {
        // Called whenever the shell navigates away from this module's page.
    }

    private void ClickButton_Click(object sender, RoutedEventArgs e)
    {
        _clickCount++;
        ClickCountText.Text = $"Clicked {_clickCount} time{(_clickCount == 1 ? string.Empty : "s")}";
    }
}
