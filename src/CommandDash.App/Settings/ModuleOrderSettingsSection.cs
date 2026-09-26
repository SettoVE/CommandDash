using CommandDash.Core;

namespace CommandDash.App.Settings;

/// <summary>
/// Built-in "Module Order" settings section. Lets the user customize the
/// order in which modules appear in the sidebar.
/// </summary>
public sealed class ModuleOrderSettingsSection : ISettingsSection
{
    public ModuleOrderSettingsSection(IReadOnlyList<IModule> orderedModules, ModuleOrderSettings settings)
    {
        Tabs = new List<ISettingsTab>
        {
            new SettingsTab("Order", () =>
            {
                var view = new ModuleOrderTabView(orderedModules, settings);
                view.OrderSaved += (_, _) => OrderSaved?.Invoke(this, EventArgs.Empty);
                return view;
            }),
        };
    }

    public string Id => "moduleorder";

    public string DisplayName => "Module Order";

    public string Group => "Default";

    public IReadOnlyList<ISettingsTab> Tabs { get; }

    /// <summary>
    /// Raised after the user saves a new order, so the shell can rebuild
    /// the sidebar without requiring an app restart.
    /// </summary>
    public event EventHandler? OrderSaved;
}
