using CommandDash.Core;

namespace CommandDash.Modules.Sample;

/// <summary>
/// Minimal example module demonstrating how to implement <see cref="IModule"/>.
/// Shows up in the sidebar under the "Samples" category.
/// </summary>
public sealed class SampleModule : IModule
{
    private IModuleContext? _context;

    public string Id => "commanddash.sample";

    public string DisplayName => "Sample Module";

    public string Description => "An example module demonstrating the CommandDash module contract.";

    public string Category => "Samples";

    public string Icon => "\uE7C1"; // Segoe Fluent Icons: puzzle piece / sample glyph placeholder

    public Version Version { get; } = new Version(1, 0, 0);

    public void OnLoaded(IModuleContext context)
    {
        _context = context;
        _context.Logger.Info($"{DisplayName} loaded.");
    }

    public void OnUnloaded()
    {
        _context?.Logger.Info($"{DisplayName} unloaded.");
        _context = null;
    }

    public IModulePage CreatePage() => new SampleModulePage();
}
