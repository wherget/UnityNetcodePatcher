using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Serilog;

namespace NetcodePatcher.Tools.Common;

class PatcherLoadContext : AssemblyLoadContext
{
    private AssemblyResolver Resolver;
    public PatcherLoadContext(string name, AssemblyResolver resolver) : base(name)
    {
        Resolver = resolver;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var loadedAssembly = Default.Assemblies
            .FirstOrDefault(assembly => assembly.GetName() == assemblyName);
        if (loadedAssembly is not null) return loadedAssembly;

        if (assemblyName.Name is null) return null;

        if (Resolver.IsSharedDependency(assemblyName))
        {
            string? sharedPath = Resolver.ResolveAssemblyToPath(assemblyName);
            if (sharedPath is null)
            {
                Log.Debug("Shared dependency {SharedName} not found in {CommonDir} or {SharedDir}, trying to load from system", assemblyName);
                return Default.LoadFromAssemblyName(assemblyName);
            }
            Log.Debug("Shared Dependency {SharedName} loading from {Directory}", assemblyName, sharedPath);
            return Default.LoadFromAssemblyPath(sharedPath);
        }

        string? assemblyPath = Resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is null) return null;
        return LoadFromAssemblyPath(assemblyPath);
    }
}
