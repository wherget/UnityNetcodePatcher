using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Serilog;

namespace NetcodePatcher.Tools.Common;

class AssemblyResolver {
    private static readonly HashSet<string> SpecificAssemblyNames = [ "NetcodePatcher", "Unity.Netcode.Runtime" ];
    private static readonly HashSet<string> SharedDependencyAssemblyNames = [ "Serilog" ];
    private readonly PatcherConfiguration _configuration;

    public AssemblyResolver(PatcherConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? ResolveAssemblyToPath(AssemblyName assemblyName)
    {
        if (assemblyName.Name is null) return null;

        if (SpecificAssemblyNames.Contains(assemblyName.Name))
        {
            if (TryResolveSpecificAssemblyToPath(assemblyName, out var specificPath))
                return specificPath;
        }

        if (TryResolveCommonAssemblyToPath(assemblyName, out var commonPath))
            return commonPath;

        return null;
    }

    public bool IsSharedDependency(AssemblyName assemblyName)
    {
        if (assemblyName.Name is null)
            return false;
        return SharedDependencyAssemblyNames.Contains(assemblyName.Name);
    }
    
    private bool TryResolveCommonAssemblyToPath(AssemblyName assemblyName, out string path)
    {
        path = Path.Combine(_configuration.PatcherCommonAssemblyDir, $"{assemblyName.Name}.dll");
        if (File.Exists(path))
            return true;
        Log.Debug("Expected shared assmbly at {path} not found", path);
        return false;
    }

    private bool TryResolveSpecificAssemblyToPath(AssemblyName assemblyName, out string path)
    {
        path = Path.Combine(_configuration.PatcherNetcodeSpecificAssemblyDir, $"{assemblyName.Name}.dll");
        if (File.Exists(path))
            return true;
        Log.Debug("Expected specific assmbly at {path} not found", path);
        return false;
    }

}