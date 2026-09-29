using CommandDash.Core;

namespace CommandDash.App.Settings;

/// <summary>
/// Built-in "Modules" node. Its own page is the Module Order UI (moving
/// modules up/down in the sidebar); its children are the settings nodes
/// contributed by any loaded module that implements
/// <see cref="IModuleWithSettings"/> (modules that don't implement it
/// simply are not listed).
/// </summary>
public sealed class ModulesSettingsNode : ISettingsNode
{
    private readonly IReadOnlyList<IModule> _orderedModules;
    private readonly ModuleOrderSettings _settings;
    private readonly ModuleBackgroundSettings _backgroundSettings;

    public ModulesSettingsNode(
        IReadOnlyList<IModule> orderedModules,
        ModuleOrderSettings settings,
        ModuleBackgroundSettings backgroundSettings)
    {
        _orderedModules = orderedModules;
        _settings = settings;
        _backgroundSettings = backgroundSettings;

        var children = new List<ISettingsNode>();
        foreach (var module in orderedModules)
        {
            if (module is IModuleWithSettings withSettings)
            {
                children.Add(withSettings.GetSettingsNode());
            }
        }

        Children = children;
    }

    public string Id => "modules";

    public string DisplayName => "Modules";

    public IReadOnlyList<ISettingsNode> Children { get; }

    /// <summary>
    /// Raised after the user saves a new module order, so the shell can
    /// rebuild the sidebar without requiring an app restart.
    /// </summary>
    public event EventHandler? OrderSaved;

    public object? CreateContent()
    {
        var view = new ModuleOrderView(_orderedModules, _settings);
        view.OrderSaved += (_, _) => OrderSaved?.Invoke(this, EventArgs.Empty);

        var panel = new System.Windows.Controls.StackPanel();
        panel.Children.Add(view);
        panel.Children.Add(new BackgroundSettingsView(_orderedModules, _backgroundSettings));

        return new System.Windows.Controls.ScrollViewer
        {
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            Content = panel,
        };
    }
}
