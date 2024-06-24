using System;
using System.Reflection;
#if !(NETFRAMEWORK || NETSTANDARD)
using System.Runtime.Loader;
using System.Linq;
#endif
using Serilog;

namespace NetcodePatcher.Tools.Common;

#if NETFRAMEWORK || NETSTANDARD
class PatcherLoadContext
{
    private AssemblyResolver Resolver;
    private AppDomain Domain;

    public PatcherLoadContext(string name, AssemblyResolver resolver)
    {
        Resolver = resolver;
        Domain = AppDomain.CurrentDomain;
        Domain.AssemblyResolve += ResolveHandler;
    }

    private Assembly? ResolveHandler(object source, ResolveEventArgs args)
    {
        Log.Debug("Trying to resolve {name}", args.Name);
        var assemblyName = new AssemblyName(args.Name);
        var path = Resolver.ResolveAssemblyToPath(assemblyName);
        if (path is not null)
        {
            Log.Debug("Resolved {name} as {path}", assemblyName.Name, path);
            return Assembly.LoadFrom(path);
        }
        return null;
    }

    public Assembly LoadFromAssemblyName(AssemblyName name)
    {
        return Domain.Load(name);
    }
}
#else
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
#endif