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
            var (methodSymbol, attribute) = pair;
            if (methodSymbol is not IMethodSymbol method) return;

            var source = attribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
            if (string.IsNullOrEmpty(source))
            {
                var body = VisualBasicEmitter.EmitError("Attribute source is empty", 3);
                EmitSource(c, method, body);
                return;
            }

            var sequence = new BrainfuckSequenceEnumerable(source!).ToArray();
            var bodyText = VisualBasicEmitter.Emit(sequence, 3);
            EmitSource(c, method, bodyText);
        });
    }

    private static void EmitSource(SourceProductionContext c, IMethodSymbol method, string body)
    {
        var sb = new System.Text.StringBuilder();
        var namespaceName = method.ContainingNamespace?.ToDisplayString() ?? "Global";
        sb.AppendLine("Namespace " + namespaceName);
        sb.AppendLine("    Partial Class " + method.ContainingType.Name);
        sb.AppendLine("        Public Shared Partial Sub " + method.Name + "(output As System.IO.TextWriter, input As System.IO.TextReader)");
        sb.AppendLine("            Dim memory As Byte() = New Byte(30000) {}");
        sb.AppendLine("            Dim pointer As Integer = 0");
        sb.Append(body);
        sb.AppendLine("        End Sub");
        sb.AppendLine("    End Class");
        sb.AppendLine("End Namespace");

        c.AddSource($"{method.Name}.g.vb", sb.ToString());
    }
}
