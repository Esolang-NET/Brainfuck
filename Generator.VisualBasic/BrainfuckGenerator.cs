using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

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

        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => node is Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodStatementSyntax,
            transform: (ctx, ct) =>
            {
                var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol((Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodStatementSyntax)ctx.Node, cancellationToken: ct);
                if (methodSymbol is null) return (default(ISymbol), default(AttributeData));
                var attribute = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "GenerateBrainfuckMethodAttribute");
                return (methodSymbol, attribute);
            })
            .Where(x => x.Item1 != null && x.Item2 != null);

        context.RegisterSourceOutput(provider.Combine(context.CompilationProvider), (c, data) =>
        {
            var (pair, compilation) = data;
            var (methodSymbol, _) = pair;
            if (methodSymbol is not IMethodSymbol method) return;

            var knownTypes = new KnownTypes(compilation);

            // Simple validation: check if return type is supported
            if (method.ReturnType.SpecialType != SpecialType.System_Void &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.String) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.Task) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.TaskInt) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.TaskString) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.ValueTask) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.ValueTaskInt) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.ValueTaskString) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.IEnumerableByte) &&
                !SymbolEqualityComparer.Default.Equals(method.ReturnType, knownTypes.IAsyncEnumerableByte))
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidReturnType, method.Locations[0], method.ReturnType.ToDisplayString()));
            }

            // BF0011: Method must be partial
            if (!method.IsPartialDefinition)
            {
                c.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MethodMustBePartial, method.Locations[0], method.Name));
            }
        });
    }
}
