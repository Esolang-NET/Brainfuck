#pragma warning disable
using Microsoft.CodeAnalysis;

namespace Esolang.Generator;

/// <summary>
/// Holds resolved type symbols for a compilation.
/// </summary>
public readonly struct KnownTypes
{
    public readonly INamedTypeSymbol? String;
    public readonly INamedTypeSymbol? Task;
    public readonly INamedTypeSymbol? TaskT;
    public readonly INamedTypeSymbol? ValueTask;
    public readonly INamedTypeSymbol? ValueTaskT;
    public readonly INamedTypeSymbol? IEnumerableT;
    public readonly INamedTypeSymbol? IAsyncEnumerableT;
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
        
        Task = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.Task");
        TaskT = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.Task`1");
        ValueTask = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.ValueTask");
        ValueTaskT = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        IEnumerableT = compilation.GetBestTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        IAsyncEnumerableT = compilation.GetBestTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
        PipeReader = compilation.GetBestTypeByMetadataName("System.IO.Pipelines.PipeReader");
        PipeWriter = compilation.GetBestTypeByMetadataName("System.IO.Pipelines.PipeWriter");
        TextReader = compilation.GetBestTypeByMetadataName("System.IO.TextReader");
        TextWriter = compilation.GetBestTypeByMetadataName("System.IO.TextWriter");
        CancellationToken = compilation.GetBestTypeByMetadataName("System.Threading.CancellationToken");
        ILogger = compilation.GetBestTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
        ILoggerT = compilation.GetBestTypeByMetadataName("Microsoft.Extensions.Logging.ILogger`1");
    }

    private static ITypeSymbol? Unwrap(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol namedType && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }
        return type;
    }

    private static bool EqualsDefinition(ITypeSymbol? type, ISymbol? symbol) => 
        type != null && symbol != null && SymbolEqualityComparer.Default.Equals(Unwrap(type)?.OriginalDefinition, symbol);
    
    private static bool EqualsType(ITypeSymbol? type, ISymbol? symbol) => 
        type != null && symbol != null && SymbolEqualityComparer.Default.Equals(Unwrap(type)?.WithNullableAnnotation(NullableAnnotation.None), symbol);

    public bool IsString(ITypeSymbol? type) => EqualsType(type, String);
    public bool IsTask(ITypeSymbol? type) => EqualsType(type, Task);
    public bool IsTaskT(ITypeSymbol? type) => EqualsDefinition(type, TaskT);
    public bool IsValueTask(ITypeSymbol? type) => EqualsType(type, ValueTask);
    public bool IsValueTaskT(ITypeSymbol? type) => EqualsDefinition(type, ValueTaskT);
    public bool IsPipeReader(ITypeSymbol? type) => EqualsType(type, PipeReader);
    public bool IsPipeWriter(ITypeSymbol? type) => EqualsType(type, PipeWriter);
    public bool IsTextReader(ITypeSymbol? type) => EqualsType(type, TextReader);
    public bool IsTextWriter(ITypeSymbol? type) => EqualsType(type, TextWriter);
    public bool IsCancellationToken(ITypeSymbol? type) => EqualsType(type, CancellationToken);
}

/// <summary>
/// Provides utility methods for resolving types from a <see cref="Compilation"/>.
/// </summary>
public static class TypeResolutionExtensions
{
    /// <summary>
    /// Resolves the best <see cref="INamedTypeSymbol"/> for the specified metadata name.
    /// </summary>
    public static INamedTypeSymbol? GetBestTypeByMetadataName(this Compilation compilation, string metadataName)
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
#pragma warning restore
