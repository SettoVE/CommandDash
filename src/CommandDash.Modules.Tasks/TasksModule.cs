using CommandDash.Core;

namespace CommandDash.Modules.Tasks;

/// <summary>
/// Placeholder built-in module for scheduled/background task management.
/// </summary>
public sealed class TasksModule : ModuleBase
{
    public override string Id => "commanddash.tasks";

    public override string DisplayName => "Tasks";

    public override string Description => "View and manage scheduled or background tasks.";

    public override string Icon => "\uE73A"; // Segoe Fluent Icons: list/tasks glyph placeholder

    public override IModulePage CreatePage() => new TasksModulePage();
}
