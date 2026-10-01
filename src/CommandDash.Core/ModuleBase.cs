namespace CommandDash.Core;

/// <summary>
/// Optional convenience base for <see cref="IModule"/> implementations. Handles context
/// tracking and load/unload logging so modules only declare their metadata and page.
/// </summary>
public abstract class ModuleBase : IModule
{
    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract string Description { get; }

    public virtual string Category => "General";

    public abstract string Icon { get; }

    public virtual Version Version { get; } = new Version(1, 0, 0);

    /// <summary>The host context; null until loaded and after unload.</summary>
    protected IModuleContext? Context { get; private set; }

    public virtual void OnLoaded(IModuleContext context)
    {
        Context = context;
        context.Logger.Info($"{DisplayName} loaded.");
    }

    public virtual void OnUnloaded()
    {
        Context?.Logger.Info($"{DisplayName} unloaded.");
        Context = null;
    }

    public abstract IModulePage CreatePage();
}
