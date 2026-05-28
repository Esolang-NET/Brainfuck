using Microsoft.CodeAnalysis;

namespace Esolang.Generator;

/// <summary>
/// Holds resolved type symbols for a compilation.
/// </summary>
public readonly struct KnownTypes
{
    /// <summary>The <c>string</c> type symbol.</summary>
    public readonly INamedTypeSymbol? String;
    /// <summary>The <c>byte</c> type symbol.</summary>
    public readonly INamedTypeSymbol? Byte;
    /// <summary>The <c>int</c> type symbol.</summary>
    public readonly INamedTypeSymbol? Int32;
    /// <summary>The <c>System.Threading.Tasks.Task</c> type symbol.</summary>
    public readonly INamedTypeSymbol? Task;
    /// <summary>The <c>System.Threading.Tasks.Task{TResult}</c> type symbol.</summary>
    public readonly INamedTypeSymbol? TaskT;
    /// <summary>The <c>System.Threading.Tasks.ValueTask</c> type symbol.</summary>
    public readonly INamedTypeSymbol? ValueTask;
    /// <summary>The <c>System.Threading.Tasks.ValueTask{TResult}</c> type symbol.</summary>
    public readonly INamedTypeSymbol? ValueTaskT;
    /// <summary>The <c>System.Collections.Generic.IEnumerable{T}</c> type symbol.</summary>
    public readonly INamedTypeSymbol? IEnumerableT;
    /// <summary>The <c>System.Collections.Generic.IAsyncEnumerable{T}</c> type symbol.</summary>
    public readonly INamedTypeSymbol? IAsyncEnumerableT;
    /// <summary>The <c>System.IO.Pipelines.PipeReader</c> type symbol.</summary>
    public readonly INamedTypeSymbol? PipeReader;
    /// <summary>The <c>System.IO.Pipelines.PipeWriter</c> type symbol.</summary>
    public readonly INamedTypeSymbol? PipeWriter;
    /// <summary>The <c>System.IO.TextReader</c> type symbol.</summary>
    public readonly INamedTypeSymbol? TextReader;
    /// <summary>The <c>System.IO.TextWriter</c> type symbol.</summary>
    public readonly INamedTypeSymbol? TextWriter;
    /// <summary>The <c>System.Threading.CancellationToken</c> type symbol.</summary>
    public readonly INamedTypeSymbol? CancellationToken;
    /// <summary>The <c>Microsoft.Extensions.Logging.ILogger</c> type symbol.</summary>
    public readonly INamedTypeSymbol? ILogger;
    /// <summary>The <c>Microsoft.Extensions.Logging.ILogger{T}</c> type symbol.</summary>
    public readonly INamedTypeSymbol? ILoggerT;

    /// <summary>
    /// Initializes a new instance of the <see cref="KnownTypes"/> struct.
    /// </summary>
    /// <param name="compilation">The compilation to resolve types from.</param>
    public KnownTypes(Compilation compilation)
    {
        String = compilation.GetSpecialType(SpecialType.System_String);
        Byte = compilation.GetSpecialType(SpecialType.System_Byte);
        Int32 = compilation.GetSpecialType(SpecialType.System_Int32);

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

    private static bool EqualsDefinition(ITypeSymbol? type, ISymbol? symbol) =>
        type != null && symbol != null && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, symbol);

    private static bool EqualsType(ITypeSymbol? type, ISymbol? symbol) =>
        type != null && symbol != null && SymbolEqualityComparer.Default.Equals(type, symbol);

    /// <summary>Gets a value indicating whether the type is <c>string</c>.</summary>
    /// <param name="type">The type to check.</param>
    /// <param name="isNullable">Optional: Whether to check for nullability.</param>
    public bool IsString(ITypeSymbol? type, bool? isNullable = null)
    {
        if (type is not INamedTypeSymbol named || !SymbolEqualityComparer.Default.Equals(named, String)) return false;
        if (isNullable == null) return true;
        if (isNullable.Value) return type.NullableAnnotation == NullableAnnotation.Annotated;
        return type.NullableAnnotation is NullableAnnotation.NotAnnotated or NullableAnnotation.None;
    }

    /// <summary>Gets a value indicating whether the type is <c>byte</c>.</summary>
    public bool IsByte(ITypeSymbol? type) => EqualsType(type, Byte);
    /// <summary>Gets a value indicating whether the type is <c>int</c>.</summary>
    public bool IsInt32(ITypeSymbol? type) => EqualsType(type, Int32);

    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.Task</c>.</summary>
    public bool IsTask(ITypeSymbol? type) => EqualsType(type, Task);
    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.Task{TResult}</c>.</summary>
    public bool IsTaskT(ITypeSymbol? type, bool? isNullable = null)
    {
        if (type is not INamedTypeSymbol named || !EqualsDefinition(named, TaskT)) return false;
        if (isNullable == null) return true;
        var annotation = named.TypeArguments[0].NullableAnnotation;
        return isNullable.Value ? annotation == NullableAnnotation.Annotated : annotation is NullableAnnotation.NotAnnotated or NullableAnnotation.None;
    }
    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.ValueTask</c>.</summary>
    public bool IsValueTask(ITypeSymbol? type) => EqualsType(type, ValueTask);
    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.ValueTask{TResult}</c>.</summary>
    public bool IsValueTaskT(ITypeSymbol? type, bool? isNullable = null)
    {
        if (type is not INamedTypeSymbol named || !EqualsDefinition(named, ValueTaskT)) return false;
        if (isNullable == null) return true;
        var annotation = named.TypeArguments[0].NullableAnnotation;
        return isNullable.Value ? annotation == NullableAnnotation.Annotated : annotation is NullableAnnotation.NotAnnotated or NullableAnnotation.None;
    }
    /// <summary>Gets a value indicating whether the type is <c>System.Collections.Generic.IEnumerable{T}</c>.</summary>
    public bool IsIEnumerableT(ITypeSymbol? type) => EqualsDefinition(type, IEnumerableT);
    /// <summary>Gets a value indicating whether the type is <c>System.Collections.Generic.IAsyncEnumerable{T}</c>.</summary>
    public bool IsIAsyncEnumerableT(ITypeSymbol? type) => EqualsDefinition(type, IAsyncEnumerableT);

    /// <summary>Gets a value indicating whether the type is <c>System.IO.Pipelines.PipeReader</c>.</summary>
    public bool IsPipeReader(ITypeSymbol? type) => EqualsType(type, PipeReader);
    /// <summary>Gets a value indicating whether the type is <c>System.IO.Pipelines.PipeWriter</c>.</summary>
    public bool IsPipeWriter(ITypeSymbol? type) => EqualsType(type, PipeWriter);
    /// <summary>Gets a value indicating whether the type is <c>System.IO.TextReader</c>.</summary>
    public bool IsTextReader(ITypeSymbol? type) => EqualsType(type, TextReader);
    /// <summary>Gets a value indicating whether the type is <c>System.IO.TextWriter</c>.</summary>
    public bool IsTextWriter(ITypeSymbol? type) => EqualsType(type, TextWriter);
    /// <summary>Gets a value indicating whether the type is <c>System.Threading.CancellationToken</c>.</summary>
    public bool IsCancellationToken(ITypeSymbol? type) => EqualsType(type, CancellationToken);

    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.Task{String}</c>.</summary>
    public bool IsTaskString(ITypeSymbol? type, bool? isNullable = null) => IsTaskT(type, isNullable) && ((INamedTypeSymbol)type!).TypeArguments[0].SpecialType == SpecialType.System_String;
    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.ValueTask{String}</c>.</summary>
    public bool IsValueTaskString(ITypeSymbol? type, bool? isNullable = null) => IsValueTaskT(type, isNullable) && ((INamedTypeSymbol)type!).TypeArguments[0].SpecialType == SpecialType.System_String;

    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.Task{Int32}</c>.</summary>
    public bool IsTaskInt32(ITypeSymbol? type) => IsTaskT(type) && ((INamedTypeSymbol)type!).TypeArguments[0].SpecialType == SpecialType.System_Int32;
    /// <summary>Gets a value indicating whether the type is <c>System.Threading.Tasks.ValueTask{Int32}</c>.</summary>
    public bool IsValueTaskInt32(ITypeSymbol? type) => IsValueTaskT(type) && ((INamedTypeSymbol)type!).TypeArguments[0].SpecialType == SpecialType.System_Int32;

    /// <summary>Gets a value indicating whether the type is <c>System.Collections.Generic.IEnumerable{Byte}</c>.</summary>
    public bool IsIEnumerableByte(ITypeSymbol? type) => IsIEnumerableT(type) && ((INamedTypeSymbol)type!).TypeArguments[0].SpecialType == SpecialType.System_Byte;
    /// <summary>Gets a value indicating whether the type is <c>System.Collections.Generic.IAsyncEnumerable{Byte}</c>.</summary>
    public bool IsIAsyncEnumerableByte(ITypeSymbol? type) => IsIAsyncEnumerableT(type) && ((INamedTypeSymbol)type!).TypeArguments[0].SpecialType == SpecialType.System_Byte;

    /// <summary>Gets a value indicating whether the type is a logger type (<c>ILogger</c> or <c>ILogger{T}</c>).</summary>
    public bool IsLogger(ITypeSymbol? type)
    {
        if (type == null) return false;
        if (EqualsType(type, ILogger) || EqualsDefinition(type, ILoggerT)) return true;
        foreach (var iface in type.AllInterfaces)
        {
            if (EqualsType(iface, ILogger) || EqualsDefinition(iface, ILoggerT)) return true;
        }
        return false;
    }
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
