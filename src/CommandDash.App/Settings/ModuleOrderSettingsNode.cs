using CommandDash.Core;

namespace CommandDash.App.Settings;

/// <summary>
/// Built-in "Module Order" node, nested under "Modules". Lets the user
/// customize the order in which modules appear in the sidebar.
/// </summary>
public sealed class ModuleOrderSettingsNode : ISettingsNode
{
    private readonly IReadOnlyList<IModule> _orderedModules;
    private readonly ModuleOrderSettings _settings;

    public ModuleOrderSettingsNode(IReadOnlyList<IModule> orderedModules, ModuleOrderSettings settings)
    {
        _orderedModules = orderedModules;
        _settings = settings;
    }

    public string Id => "modules.order";

    public string DisplayName => "Module Order";

    public IReadOnlyList<ISettingsNode> Children { get; } = Array.Empty<ISettingsNode>();

    /// <summary>
    /// Raised after the user saves a new order, so the shell can rebuild
    /// the sidebar without requiring an app restart.
    /// </summary>
    public event EventHandler? OrderSaved;

    public object? CreateContent()
    {
        var view = new ModuleOrderView(_orderedModules, _settings);
        view.OrderSaved += (_, _) => OrderSaved?.Invoke(this, EventArgs.Empty);
        return view;
    }
}
