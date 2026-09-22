namespace CommandDash.Core;

/// <summary>
/// Lightweight, serializable snapshot of a module's metadata, used by the
/// shell to render sidebar entries and module cards without needing to keep
/// the module instance itself in memory.
/// </summary>
public sealed class ModuleManifest
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public string Description { get; init; } = string.Empty;
    public required string Category { get; init; }
    public string Icon { get; init; } = string.Empty;
    public Version Version { get; init; } = new Version(1, 0, 0);

    public static ModuleManifest FromModule(IModule module) => new()
    {
        Id = module.Id,
        DisplayName = module.DisplayName,
        Description = module.Description,
        Category = module.Category,
        Icon = module.Icon,
        Version = module.Version,
    };
}
