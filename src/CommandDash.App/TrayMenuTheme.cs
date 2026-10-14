using System.Windows;
using System.Windows.Media;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace CommandDash.App;

/// <summary>Styles the tray context menu with the colors from Themes/Theme.xaml.</summary>
internal static class TrayMenuTheme
{
    public static void Apply(Forms.ContextMenuStrip menu)
    {
        var table = new ThemeColorTable();
        menu.Renderer = new Forms.ToolStripProfessionalRenderer(table) { RoundedEdges = false };
        menu.BackColor = table.ToolStripDropDownBackground;
        menu.ForeColor = Get("TextPrimaryColor", Drawing.Color.WhiteSmoke);
        foreach (Forms.ToolStripItem item in menu.Items)
        {
            item.ForeColor = menu.ForeColor;
        }
    }

    private static Drawing.Color Get(string key, Drawing.Color fallback)
    {
        if (Application.Current?.TryFindResource(key) is Color c)
        {
            return Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        return fallback;
    }

    private sealed class ThemeColorTable : Forms.ProfessionalColorTable
    {
        private readonly Drawing.Color _background = Get("CardColor", Drawing.Color.FromArgb(0x2F, 0x31, 0x36));
        private readonly Drawing.Color _hover = Get("CardHoverColor", Drawing.Color.FromArgb(0x38, 0x3A, 0x40));
        private readonly Drawing.Color _border = Get("BorderColor", Drawing.Color.FromArgb(0x3A, 0x3C, 0x41));

        public ThemeColorTable()
        {
            UseSystemColors = false;
        }

        public override Drawing.Color ToolStripDropDownBackground => _background;
        public override Drawing.Color ImageMarginGradientBegin => _background;
        public override Drawing.Color ImageMarginGradientMiddle => _background;
        public override Drawing.Color ImageMarginGradientEnd => _background;
        public override Drawing.Color MenuBorder => _border;
        public override Drawing.Color MenuItemBorder => _border;
        public override Drawing.Color MenuItemSelected => _hover;
        public override Drawing.Color MenuItemSelectedGradientBegin => _hover;
        public override Drawing.Color MenuItemSelectedGradientEnd => _hover;
        public override Drawing.Color SeparatorDark => _border;
    }
}
