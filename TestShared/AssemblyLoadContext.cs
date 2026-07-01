
#if NET
// using System.Runtime.Loader;
#else
using Assembly = System.Reflection.Assembly;
#endif

namespace TestShared;

class AssemblyLoadContext
    :

#if NET
    System.Runtime.Loader.AssemblyLoadContext,
#endif
    IDisposable
{
#if !NET
    const string DOMAIN_NAME = "test domain";
    AppDomain _domain;
#endif

    public AssemblyLoadContext()
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
        : base(isCollectible: true)
#endif
#pragma warning disable IDE0021 // Use expression body for constructors
    {
#if !NET
        _domain = AppDomain.CreateDomain(DOMAIN_NAME);
#endif
    }
#if !NET
    public Assembly LoadFromAssemblyPath(string assemblyFile) => Assembly.LoadFrom(assemblyFile);
    public Assembly LoadFromStream(Stream assembly, Stream? pdbStream = null)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }
        byte[] rowAssembly;
        {
            if (assembly is MemoryStream memoryStream)
            {
                if (memoryStream.CanSeek && memoryStream.Position > 0)
                    memoryStream.Seek(0, SeekOrigin.Begin);
                rowAssembly = memoryStream.ToArray();
            }
            else
            {
                using var stream = new MemoryStream();
                assembly.CopyTo(stream);
                stream.Seek(0, SeekOrigin.End);
                rowAssembly = stream.ToArray();
            }
        }
        byte[]? rawSymbolStore = null;
        if (pdbStream != null)
        {

            if (assembly is MemoryStream memoryStream)
            {
                if (memoryStream.CanSeek && memoryStream.Position > 0)
                    memoryStream.Seek(0, SeekOrigin.Begin);
                rawSymbolStore = memoryStream.ToArray();
            }
            else
            {
                using var stream = new MemoryStream();
                assembly.CopyTo(stream);
                stream.Seek(0, SeekOrigin.End);
                rawSymbolStore = stream.ToArray();
            }
        }
        if (rawSymbolStore is { Length: > 0 })
#pragma warning disable RS1035 // アナライザーに対して禁止された API を使用しない
            return Assembly.Load(rowAssembly, rawSymbolStore);
        return Assembly.Load(rowAssembly);
#pragma warning restore RS1035 // アナライザーに対して禁止された API を使用しない
    }
#endif
#pragma warning restore IDE0021 // Use expression body for constructors

    public void Dispose()
#pragma warning disable IDE0022 // Use expression body for methods
    {
#if NET
        Unload();
#else
        if (_domain is null) return;
        AppDomain.Unload(_domain);
        _domain = null!;
#endif
    }
#pragma warning restore IDE0022 // Use expression body for methods
}
