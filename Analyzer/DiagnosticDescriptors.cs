using Microsoft.CodeAnalysis;

namespace Brainfuck.Analyzer;

public static class DiagnosticDescriptors
{
    const string Category = "Brainfuck";
    /// <summary>
    /// BF0001: Invalid vlue parameter: The value parameter of the attribute on the method '{0}' must not be null.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidValueParameter = new(
        id: "BF0001",
        title: "Invalid vlue parameter",
        messageFormat: "The value parameter of the attribute on the method '{0}' must not be null",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    /// <summary>
    /// BF0002: not support return type: The method is support return type {0} is not support.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidReturnType = new(
        id: "BF0002",
        title: "not support return type",
        messageFormat: "The method is support return type {0} is not support",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    /// <summary>
    /// BF0003: not support parameter type: The parameter of the method '{0}' not support type.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidParameter = new(
        id: "BF0003",
        title: "not support parameter type",
        messageFormat: "The parameter of the method '{0}' not support type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    /// <summary>
    /// BF0004: not support duplicate parameter type: The parameter of the method '{0}' duplicate type.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateParameter = new(
        id: "BF0004",
        title: "not support duplicate parameter type",
        messageFormat: "The parameter of the method '{0}' duplicate type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    /// <summary>
    /// BF0005: not support parameter pattern: The parameter of the method 'System.IO.Pipelines.PipeReader' and 'string' not support pattern.
    /// </summary>
    public static readonly DiagnosticDescriptor NotSupportParameterPattern = new(
        id: "BF0005",
        title: "not support parameter pattern",
        messageFormat: "The parameter of the method 'System.IO.Pipelines.PipeReader' and 'string' not support pattern",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    public static readonly DiagnosticDescriptor NotSupportParameterAndReturnTypePattern = new(
        id: "BF0006",
        title: "not support parameter and return type pattern",
        messageFormat: "The parameter of the method parameter '{0}' and return type '{1}' not support pattern",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}
