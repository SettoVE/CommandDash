using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CommandDash.App.Settings;

public partial class GeneralSettingsView : UserControl
{
    public GeneralSettingsView()
    {
        InitializeComponent();
    }

    private void OpenAppFolder_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = AppContext.BaseDirectory,
            UseShellExecute = true,
        });
    }
}
