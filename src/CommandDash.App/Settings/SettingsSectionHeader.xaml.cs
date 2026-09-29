using System.Windows;
using System.Windows.Controls;

namespace CommandDash.App.Settings;

/// <summary>
/// Standard title separator used to group options on a settings page:
/// left-aligned title text followed by a horizontal line that fills the
/// remaining width. Use this everywhere so section titles look identical.
/// </summary>
public partial class SettingsSectionHeader : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(SettingsSectionHeader),
        new PropertyMetadata(string.Empty, (d, e) => ((SettingsSectionHeader)d).TitleText.Text = (string)e.NewValue));

    public SettingsSectionHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
