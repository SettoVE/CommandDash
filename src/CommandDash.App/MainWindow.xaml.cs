using System.Windows;

namespace CommandDash.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Module cards will be populated dynamically once the module loader
        // (IModule discovery) is implemented in a later step.
    }
}
