using Microsoft.CodeAnalysis;

namespace Esolang.Brainfuck.Generator;

/// <summary>
/// Provides diagnostic definitions reported during source generation.
/// </summary>
public static class DiagnosticDescriptors
{
    const string Category = "Brainfuck";

    /// <summary>
    /// BF0001: Invalid value parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidValueParameter = new(
        id: "BF0001",
        title: "Invalid value parameter",
        messageFormat: "The value parameter of the attribute on the method '{0}' must not be null",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0002: Unsupported return type.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidReturnType = new(
        id: "BF0002",
        title: "Unsupported return type",
        messageFormat: "The method return type '{0}' is not supported for Brainfuck code generation",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0003: Unsupported parameter type.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidParameter = new(
        id: "BF0003",
        title: "Unsupported parameter type",
        messageFormat: "The parameter '{0}' of the method has an unsupported type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0004: Duplicate parameter type.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateParameter = new(
        id: "BF0004",
        title: "Duplicate parameter type",
        messageFormat: "The parameter type '{0}' is duplicated and not supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0005: Unsupported parameter pattern.
    /// </summary>
    public static readonly DiagnosticDescriptor NotSupportParameterPattern = new(
        id: "BF0005",
        title: "Unsupported parameter pattern",
        messageFormat: "Input parameters use an unsupported combination (allowed: one of 'string', 'PipeReader', or 'TextReader')",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0006: Return type and output parameter conflict.
    /// </summary>
    public static readonly DiagnosticDescriptor NotSupportParameterAndReturnTypePattern = new(
        id: "BF0006",
        title: "Return type and output parameter conflict",
        messageFormat: "The output parameter type '{0}' cannot be combined with return type '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0007: Required output interface not provided.
    /// </summary>
    public static readonly DiagnosticDescriptor RequiredOutputInterface = new(
        id: "BF0007",
        title: "Required output interface not provided",
        messageFormat: "The Brainfuck source requires output, but the method does not provide an output mechanism",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0008: Required input interface not provided.
    /// </summary>
    public static readonly DiagnosticDescriptor RequiredInputInterface = new(
        id: "BF0008",
        title: "Required input interface not provided",
        messageFormat: "The Brainfuck source requires input, but the method does not provide an input mechanism",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0009: Input interface provided but not required.
    /// </summary>
    public static readonly DiagnosticDescriptor UnusedInputParameter = new(
        id: "BF0009",
        title: "Input interface provided but not required",
        messageFormat: "The Brainfuck source does not require input, but the method provides input parameter '{0}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    /// <summary>
    /// BF0010: Language version may be too low for generated code.
    /// </summary>
    public static readonly DiagnosticDescriptor LanguageVersionTooLow = new(
        id: "BF0010",
        title: "Language version may be too low",
        messageFormat: "The method '{0}' may require C# 8.0 or later, but the current language version is '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
