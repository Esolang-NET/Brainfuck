using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Brainfuck.Analyzer;
public partial class BrainfuckMethodGenerator
{
    static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var methodSymbol = (IMethodSymbol)source.TargetSymbol;
        var methodDeclarationSyntax = (MethodDeclarationSyntax)source.TargetNode;
        if (GetSources(context, methodSymbol, methodDeclarationSyntax) is not { } sequences)
            return;
        var builder = new StringBuilder();
        foreach(var (sequence, _) in sequences)
        {
            
        }
    }
    static BrainfuckSequencer? GetSources(
        SourceProductionContext context,
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodDeclarationSyntax
    )
    {

        var attributeData = methodSymbol.GetAttributes().Single(
            x => x.AttributeClass?.ToDisplayString() == ClassNameBrainfuckAttribution
        );

        if (attributeData.ConstructorArguments.Length != 1)
            throw new InvalidOperationException($"ConstructorArguments.Length is {attributeData.ConstructorArguments.Length}");

        var typedConstantForValueParameter = attributeData.ConstructorArguments[0];
        if (typedConstantForValueParameter.IsNull)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidValueParameter,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name));
            return null;
        }
        var source = typedConstantForValueParameter.Value as string;
        if (string.IsNullOrEmpty(source))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidValueParameter,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name));
            return null;
        }
        var incrementPointer = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.IncrementPointer), BrainfuckOptionsDefault.IncrementPointer);
        var decrementPointer = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.DecrementPointer), BrainfuckOptionsDefault.DecrementPointer);
        var incrementCurrent = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.IncrementCurrent), BrainfuckOptionsDefault.IncrementCurrent);
        var decrementCurrent = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.DecrementCurrent), BrainfuckOptionsDefault.DecrementCurrent);
        var output = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.Output), BrainfuckOptionsDefault.Output);
        var input = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.Input), BrainfuckOptionsDefault.Input);
        var begin = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.Begin), BrainfuckOptionsDefault.Begin);
        var end = GetNamedArgumentOrDefault(nameof(BrainfuckOptions.End), BrainfuckOptionsDefault.End);
        return new BrainfuckSequencer(source!.AsMemory(), new BrainfuckOptions
        {
            IncrementPointer = incrementPointer,
            DecrementPointer = decrementPointer,
            IncrementCurrent = incrementCurrent,
            DecrementCurrent = decrementCurrent,
            Output = output,
            Input = input,
            Begin = begin,
            End = end,
        });
        T GetNamedArgumentOrDefault<T>(string name, T defaultValue)
        {
            // ImmutbaleArray<T> does not have a Find method...
            foreach (var namedArgument in attributeData.NamedArguments)
            {
                if (StringComparer.Ordinal.Equals(namedArgument.Key, name))
                {
                    return (T)namedArgument.Value.Value!;
                }
            }
            return defaultValue;
        }

    }
}
