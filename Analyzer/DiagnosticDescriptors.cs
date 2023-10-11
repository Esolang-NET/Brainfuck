using Microsoft.CodeAnalysis;

namespace Brainfuck.Analyzer;

public static class DiagnosticDescriptors
{
    const string Category = "Brainfuck";
    public static readonly DiagnosticDescriptor InvalidValueParameter = new(
        id: "BF0003",
        title: "Invalid vlue parameter",
        messageFormat: "The value parameter of the attribute on the method '{0}' must not be null.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}