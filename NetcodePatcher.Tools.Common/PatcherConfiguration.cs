using System;
using System.IO;

namespace NetcodePatcher.Tools.Common;

public record PatcherConfiguration
{
#if NETFRAMEWORK || NETSTANDARD
    /* can't guarantee "required", "init" of those in netstandard */
    public Version UnityVersion { get; set; }
    public Version NetcodeVersion { get; set; }
    public Version TransportVersion { get; set; }
    public bool NativeCollectionSupport { get; set; }

    public PatcherConfiguration() {
        UnityVersion = Version.Parse("2022.3.9");
        NetcodeVersion = Version.Parse("1.5.2");
        TransportVersion = Version.Parse("1.0.0");
        NativeCollectionSupport = false;
    }
#else
    public required Version UnityVersion { get; init; }
    public required Version NetcodeVersion { get; init; }
    public required Version TransportVersion { get; init; }
    public required bool NativeCollectionSupport { get; init; }
#endif

    private readonly Lazy<string> _executingDir = new(
        () => Path.GetFullPath(Path.GetDirectoryName(typeof(PatcherConfiguration).Assembly.Location)!)
    );
    public string ExecutingDir => _executingDir.Value;

    public string PatcherCommonAssemblyDir => Path.Combine(
        ExecutingDir,
        $"unity-v{UnityVersion.Major}.{UnityVersion.Minor}",
        $"unity-transport-v{TransportVersion}"
    );

    public string PatcherNetcodeSpecificAssemblyDir => Path.Combine(
        PatcherCommonAssemblyDir,
        $"netcode-v{NetcodeVersion}",
        NativeCollectionSupport ? "with-native-collection-support" : "without-native-collection-support"
    );

    public string PatcherAssemblyFile => Path.Combine(
        PatcherNetcodeSpecificAssemblyDir,
        "NetcodePatcher.dll"
    );

    public override string ToString()
    {
        return $"PatcherConfiguration {{\nUnity {UnityVersion},\nNetcode {NetcodeVersion},\nTransport {TransportVersion},\nNative collection support? {NativeCollectionSupport}\n}}";
    }
}
