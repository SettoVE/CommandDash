using System.IO;
using CommandDash.Core;

namespace CommandDash.App.Modules;

/// <summary>
/// Discovers and loads <see cref="IModule"/> implementations from a
/// directory of module DLLs, each in its own <see cref="ModuleLoadContext"/>.
/// </summary>
public sealed class ModuleLoader
{
    /// <summary>
    /// Scans <paramref name="modulesDirectory"/> (non-recursive) for *.dll
    /// files, loads each into its own context, and instantiates every
    /// public, concrete, parameterless-constructible type that implements
    /// <see cref="IModule"/>.
    /// </summary>
    public IReadOnlyList<LoadedModule> LoadFrom(string modulesDirectory)
    {
        var loaded = new List<LoadedModule>();

        if (!Directory.Exists(modulesDirectory))
        {
            return loaded;
        }

        foreach (var dllPath in Directory.EnumerateFiles(modulesDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            IReadOnlyList<LoadedModule> modulesInAssembly;
            try
            {
                modulesInAssembly = LoadModulesFromAssembly(dllPath);
            }
            catch (Exception)
            {
                // A single bad/incompatible module shouldn't prevent the rest
                // from loading. Diagnostics/logging will be added later.
                continue;
            }

            loaded.AddRange(modulesInAssembly);
        }

        return loaded;
    }

    private static IReadOnlyList<LoadedModule> LoadModulesFromAssembly(string dllPath)
    {
        var context = new ModuleLoadContext(dllPath);
        var assembly = context.LoadFromAssemblyPath(dllPath);

        var results = new List<LoadedModule>();

        foreach (var type in assembly.GetExportedTypes())
        {
            if (!typeof(IModule).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            if (type.GetConstructor(Type.EmptyTypes) is null)
            {
                continue;
            }

            if (Activator.CreateInstance(type) is IModule module)
            {
                results.Add(new LoadedModule(module, context, dllPath));
            }
        }

        return results;
    }
}

/// <summary>
/// An <see cref="IModule"/> instance together with the load context and
/// source path it came from, so the host can later unload it if needed.
/// </summary>
public sealed record LoadedModule(IModule Module, ModuleLoadContext Context, string AssemblyPath);
