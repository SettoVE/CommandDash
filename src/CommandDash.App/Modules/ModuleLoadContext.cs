using System.Reflection;
using System.Runtime.Loader;

namespace CommandDash.App.Modules;

/// <summary>
/// A dedicated <see cref="AssemblyLoadContext"/> per module assembly so that
/// modules can be loaded (and, in the future, unloaded) in isolation from
/// the host application and from each other.
/// </summary>
public sealed class ModuleLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public ModuleLoadContext(string modulePath)
        : base(name: Path.GetFileNameWithoutExtension(modulePath), isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(modulePath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Let shared contracts (CommandDash.Core) resolve against the host's
        // already-loaded assembly instead of loading a second copy, so that
        // types like IModule are assignable across the load boundary.
        if (assemblyName.Name == "CommandDash.Core")
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath is not null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is not null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
