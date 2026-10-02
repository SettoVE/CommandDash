# CommandDash UI guidelines

- The app uses a dark theme defined in `src/CommandDash.App/Themes/Theme.xaml`. Never rely on default WPF control colors (they render black text on the dark background).
- Use theme brushes for colors: `TextPrimaryBrush` for normal text, `TextSecondaryBrush` for muted text. Do not hardcode colors.
- Any new control type that shows text (CheckBox, RadioButton, TextBlock, Label, etc.) must have its Foreground set through `Theme.xaml` (implicit style) or an explicit `{StaticResource TextPrimaryBrush}`.
- Verify new UI is readable against the dark background.
