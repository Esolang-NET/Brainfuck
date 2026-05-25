using Microsoft.CodeAnalysis;

namespace Esolang.Brainfuck.Generator;

/// <summary>
/// A source generator that generates Brainfuck methods for Visual Basic.
/// </summary>
[Generator(LanguageNames.VisualBasic)]
public class BrainfuckGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(c => c.AddSource("GenerateBrainfuckMethodAttribute.g.vb", """
            Namespace Esolang.Brainfuck
                <System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple:=False, Inherited:=False)>
                Public NotInheritable Class GenerateBrainfuckMethodAttribute
                Inherits System.Attribute

                Public Sub New(ByVal source As String)
                    Me.Source = source
                End Sub

                Public Property Source As String
                Public Property IncrementPointer As String = "+"
                Public Property DecrementPointer As String = "-"
                Public Property IncrementCurrent As String = ">"
                Public Property DecrementCurrent As String = "<"
                Public Property Output As String = "."
                Public Property Input As String = ","
                Public Property Begin As String = "["
                Public Property [End] As String = "]"
                End Class
            End Namespace
            """));

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Esolang.Brainfuck.GenerateBrainfuckMethodAttribute",
            predicate: (node, _) => node is Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodStatementSyntax,
            transform: (ctx, ct) => ctx);

        var compilationProvider = context.CompilationProvider;
        var knownTypesProvider = compilationProvider.Select((c, _) => new KnownTypes(c));

        var combined = provider.Combine(knownTypesProvider);

        context.RegisterSourceOutput(combined, (c, data) =>
        {
            var (ctx, types) = data;
            var methodSymbol = (IMethodSymbol)ctx.TargetSymbol;
            var attribute = ctx.Attributes.FirstOrDefault();

            if (!methodSymbol.IsPartialDefinition)
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MethodMustBePartial, methodSymbol.Locations[0], methodSymbol.Name));
                return;
            }

            var source = attribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
            if (string.IsNullOrEmpty(source))
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidValueParameter, methodSymbol.Locations[0], methodSymbol.Name));
                return;
            }

            var sequence = new BrainfuckSequenceEnumerable(source!).ToArray();
            var enumerable = new BrainfuckSequenceEnumerable(source!);
            
            if (methodSymbol.ReturnsVoid == false)
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidReturnType, methodSymbol.Locations[0], methodSymbol.ReturnType.ToDisplayString()));
                return;
            }

            if (!TryGetParameterOptions(c, methodSymbol, types, enumerable, out var options))
            {
                return;
            }

            // BF0007: Required output interface not provided.
            if (enumerable.RequiredOutput && options.VariableTextWriter == null)
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.RequiredOutputInterface, methodSymbol.Locations[0]));
            }

            // BF0008: Required input interface not provided.
            if (enumerable.RequiredInput && options.VariableTextReader == null)
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.RequiredInputInterface, methodSymbol.Locations[0]));
            }

            var bodyText = VisualBasicEmitter.Emit(sequence, 3, options.VariableTextWriter, options.VariableTextReader);
            EmitSource(c, methodSymbol, options, bodyText);
        });
    }

    private readonly record struct ParameterOptions(
        string? VariableTextWriter,
        string? VariableTextReader
    );

    private static bool TryGetParameterOptions(SourceProductionContext context, IMethodSymbol methodSymbol, KnownTypes types, BrainfuckSequenceEnumerable enumerable, out ParameterOptions options)
    {
        options = default;
        var hasError = false;

        string? variableTextWriter = null;
        string? variableTextReader = null;

        foreach (var param in methodSymbol.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(param.Type, types.TextWriter))
            {
                if (variableTextWriter != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateParameter, param.Locations[0], param.Type.ToDisplayString()));
                    hasError = true;
                }
                variableTextWriter = param.Name;
            }
            else if (SymbolEqualityComparer.Default.Equals(param.Type, types.TextReader))
            {
                if (variableTextReader != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateParameter, param.Locations[0], param.Type.ToDisplayString()));
                    hasError = true;
                }
                variableTextReader = param.Name;

                // BF0009: Input interface provided but not required.
                if (!enumerable.RequiredInput)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnusedInputParameter, param.Locations[0], param.Name));
                }
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidParameter, param.Locations[0], param.Name));
                hasError = true;
            }
        }

        options = new ParameterOptions(variableTextWriter, variableTextReader);
        return !hasError;
    }

    private static void EmitSource(SourceProductionContext c, IMethodSymbol method, ParameterOptions options, string body)
    {
        var sb = new System.Text.StringBuilder();
        var hasNamespace = method.ContainingNamespace?.IsGlobalNamespace == false;
        var namespaceName = hasNamespace ? method.ContainingNamespace!.ToDisplayString() : null;
        
        // In VB.NET, partial methods MUST be Private.
        var accessibility = "Private";
        var staticModifier = method.IsStatic ? " Shared" : "";

        var parameters = new System.Collections.Generic.List<string>();
        if (options.VariableTextWriter != null) parameters.Add($"{options.VariableTextWriter} As System.IO.TextWriter");
        if (options.VariableTextReader != null) parameters.Add($"{options.VariableTextReader} As System.IO.TextReader");
        var parameterList = string.Join(", ", parameters);

        var indent = "";
        if (hasNamespace)
        {
            sb.AppendLine("Namespace " + namespaceName);
            indent = "    ";
        }

        sb.AppendLine($$"""
        {{indent}}Partial Class {{method.ContainingType.Name}}
        """);
        sb.AppendLine($$"""
        {{indent}}    {{accessibility}}{{staticModifier}} Sub {{method.Name}}({{parameterList}})
        """);
        sb.AppendLine($$"""
        {{indent}}        Dim memory As Byte() = New Byte(30000) {}
        {{indent}}        Dim pointer As Integer = 0
        {{body}}
        {{indent}}    End Sub
        {{indent}}End Class
        """);

        if (hasNamespace)
        {
            sb.AppendLine("End Namespace");
        }

        c.AddSource($"{method.Name}.g.vb", sb.ToString());
    }
}
