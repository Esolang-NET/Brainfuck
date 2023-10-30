
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
using System.Runtime.Loader;
#else
using System.Reflection;
#endif

namespace Brainfuck.TestShared;
internal class TestAssemblyLoadContext :

#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
    AssemblyLoadContext,
#endif
    IDisposable
{
#if !(NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER)
    const string DOMAIN_NAME = "test domain";
    private AppDomain _domain;
#endif

    public TestAssemblyLoadContext()
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
        : base(isCollectible: true)
#endif
#pragma warning disable IDE0021 // コンストラクターに式本体を使用する
    {
#if !(NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER)
        _domain = AppDomain.CreateDomain(DOMAIN_NAME);
#endif
    }
#if !(NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER)
    public Assembly LoadFromAssemblyPath(string assemblyFile) => Assembly.LoadFrom(assemblyFile);
    public Assembly LoadFromStream(Stream assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }
        byte[] rowAssembly;
        if (assembly is MemoryStream memoryStream)
        {
            if (memoryStream.CanSeek)
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
        return Assembly.Load(rowAssembly);
    }
#endif
#pragma warning restore IDE0021 // コンストラクターに式本体を使用する

    public void Dispose()
#pragma warning disable IDE0022 // メソッドに式本体を使用する
    {
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
        Unload();
#else
        if (_domain is null) return;
        AppDomain.Unload(_domain);
        _domain = null!;
#endif
    }
#pragma warning restore IDE0022 // メソッドに式本体を使用する
}
