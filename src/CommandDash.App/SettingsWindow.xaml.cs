using System.Windows;
using System.Windows.Controls;
using CommandDash.Core;

namespace CommandDash.App;

/// <summary>
/// Generic settings shell: a sidebar of <see cref="ISettingsSection"/>s
/// (primary navigation, grouped) and a tab strip for the selected
/// section's <see cref="ISettingsTab"/>s (secondary navigation).
///
/// Adding a new built-in section only requires passing another
/// <see cref="ISettingsSection"/> instance into the constructor; this
/// window never needs to change.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly List<ISettingsSection> _sections;

    public SettingsWindow(IEnumerable<ISettingsSection> sections)
    {
        InitializeComponent();

        _sections = sections.ToList();
        BuildSectionList();
    }

    private void BuildSectionList()
    {
        SectionPanel.Children.Clear();

        string? lastGroup = null;
        foreach (var section in _sections)
        {
            if (section.Group != "Default" && section.Group != lastGroup)
            {
                SectionPanel.Children.Add(new TextBlock
                {
                    Text = section.Group.ToUpperInvariant(),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(8, 16, 0, 4),
                    Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush"),
                });
            }

            lastGroup = section.Group;

            var sectionItem = new RadioButton
            {
                Content = section.DisplayName,
                GroupName = "SettingsSections",
                Style = (Style)FindResource("NavItemStyle"),
                Tag = section,
            };
            sectionItem.Checked += (_, _) => SelectSection(section);
            SectionPanel.Children.Add(sectionItem);
        }

        var firstItem = SectionPanel.Children.OfType<RadioButton>().FirstOrDefault();
        if (firstItem is not null)
        {
            firstItem.IsChecked = true; // triggers SelectSection via Checked handler
        }
    }

    private void SelectSection(ISettingsSection section)
    {
        SectionTitle.Text = section.DisplayName;

        SectionTabControl.Items.Clear();
        foreach (var tab in section.Tabs)
        {
            SectionTabControl.Items.Add(new TabItem
            {
                Header = tab.Header,
                Content = tab.CreateContent(),
            });
        }
    }
}
