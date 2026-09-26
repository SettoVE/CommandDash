using CommandDash.Core;

namespace CommandDash.App.Settings;

/// <summary>
/// Built-in "Modules" node. Always contains "Module Order" as its first
/// child; additionally contains one child per loaded module that
/// implements <see cref="IModuleWithSettings"/> (modules that don't
/// implement it simply are not listed).
/// </summary>
public sealed class ModulesSettingsNode : ISettingsNode
{
    public ModulesSettingsNode(IReadOnlyList<IModule> orderedModules, ModuleOrderSettings settings)
    {
        var children = new List<ISettingsNode>();

        var moduleOrderNode = new ModuleOrderSettingsNode(orderedModules, settings);
        moduleOrderNode.OrderSaved += (_, _) => OrderSaved?.Invoke(this, EventArgs.Empty);
        children.Add(moduleOrderNode);

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

    public object? CreateContent() => null;
}
