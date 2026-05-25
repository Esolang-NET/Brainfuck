using Microsoft.CodeAnalysis;

namespace Esolang.Brainfuck.Generator;

internal readonly struct KnownTypes
{
    public readonly INamedTypeSymbol? String;
    public readonly INamedTypeSymbol? Task;
    public readonly INamedTypeSymbol? TaskInt;
    public readonly INamedTypeSymbol? TaskString;
    public readonly INamedTypeSymbol? ValueTask;
    public readonly INamedTypeSymbol? ValueTaskInt;
    public readonly INamedTypeSymbol? ValueTaskString;
    public readonly INamedTypeSymbol? IEnumerableByte;
    public readonly INamedTypeSymbol? IAsyncEnumerableByte;
    public readonly INamedTypeSymbol? PipeReader;
    public readonly INamedTypeSymbol? PipeWriter;
    public readonly INamedTypeSymbol? TextReader;
    public readonly INamedTypeSymbol? TextWriter;
    public readonly INamedTypeSymbol? CancellationToken;
    public readonly INamedTypeSymbol? ILogger;
    public readonly INamedTypeSymbol? ILoggerT;

    public KnownTypes(Compilation compilation)
    {
        String = compilation.GetSpecialType(SpecialType.System_String);
        var byteSymbol = compilation.GetSpecialType(SpecialType.System_Byte);
        var intSymbol = compilation.GetSpecialType(SpecialType.System_Int32);

        var taskGeneric = GetBestTypeByMetadataName(compilation, "System.Threading.Tasks.Task`1");
        Task = GetBestTypeByMetadataName(compilation, "System.Threading.Tasks.Task");
        TaskInt = taskGeneric?.Construct(intSymbol);
        TaskString = taskGeneric?.Construct(String);

        var valueTaskGeneric = GetBestTypeByMetadataName(compilation, "System.Threading.Tasks.ValueTask`1");
        ValueTask = GetBestTypeByMetadataName(compilation, "System.Threading.Tasks.ValueTask");
        ValueTaskInt = valueTaskGeneric?.Construct(intSymbol);
        ValueTaskString = valueTaskGeneric?.Construct(String);

        var enumerableGeneric = GetBestTypeByMetadataName(compilation, "System.Collections.Generic.IEnumerable`1");
        IEnumerableByte = enumerableGeneric?.Construct(byteSymbol);

        var asyncEnumerableGeneric = GetBestTypeByMetadataName(compilation, "System.Collections.Generic.IAsyncEnumerable`1");
        IAsyncEnumerableByte = asyncEnumerableGeneric?.Construct(byteSymbol);

        PipeReader = GetBestTypeByMetadataName(compilation, "System.IO.Pipelines.PipeReader");
        PipeWriter = GetBestTypeByMetadataName(compilation, "System.IO.Pipelines.PipeWriter");
        TextReader = GetBestTypeByMetadataName(compilation, "System.IO.TextReader");
        TextWriter = GetBestTypeByMetadataName(compilation, "System.IO.TextWriter");
        CancellationToken = GetBestTypeByMetadataName(compilation, "System.Threading.CancellationToken");
        ILogger = GetBestTypeByMetadataName(compilation, "Microsoft.Extensions.Logging.ILogger");
        ILoggerT = GetBestTypeByMetadataName(compilation, "Microsoft.Extensions.Logging.ILogger`1");
    }

    private static INamedTypeSymbol? GetBestTypeByMetadataName(Compilation compilation, string metadataName)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        if (type != null) return type;

        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            var found = assembly.GetTypeByMetadataName(metadataName);
            if (found != null) return found;
        }
        return null;
    }
}
