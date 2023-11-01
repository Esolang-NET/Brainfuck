using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace Brainfuck.Generator;

/// <summary>Provides utility functions.</summary>
public static class Utils
{
    public static string SanitizeForFileName(string str) => str
            .Replace("global::", "")
            .Replace("?", "")
            .Replace("<", "_")
            .Replace(">", "_");

    /// <summary>Gets a type declaration string such as <c>"static partial class&lt;T&gt;"</c>.</summary>
    public static string ToTypeDeclarationBeforeOpeningBrace(INamedTypeSymbol namedTypeSymbol, bool isPartial)
    {
        if (!namedTypeSymbol.IsType) { throw new NotSupportedException(); }

        var result = new StringBuilder();
        result.Append(SyntaxFacts.GetText(namedTypeSymbol.DeclaredAccessibility));
        result.Append(' ');
        if (namedTypeSymbol.IsAbstract && namedTypeSymbol.TypeKind != TypeKind.Interface) { result.Append("abstract "); }
        if (namedTypeSymbol.IsStatic) { result.Append("static "); }
        if (namedTypeSymbol.IsReadOnly) { result.Append("readonly "); }
        if (namedTypeSymbol.IsRefLikeType) { result.Append("ref "); }
        if (isPartial) { result.Append("partial "); }
        if (namedTypeSymbol.IsRecord) { result.Append("record "); }

        var typeKind = namedTypeSymbol.TypeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Interface => "interface",
            TypeKind.Struct => "struct",
            _ => throw new ArgumentException($"Unknwon {nameof(namedTypeSymbol.TypeKind)}: {namedTypeSymbol.TypeKind}", nameof(namedTypeSymbol)),
        };
        result.Append(typeKind);
        result.Append(' ');
        result.Append(namedTypeSymbol.Name);

        if (namedTypeSymbol.IsGenericType)
        {
            result.Append('<');
            result.Append(string.Join(",", namedTypeSymbol.TypeParameters));
            result.Append('>');
        }

        return result.ToString();
    }

    /// <summary>Gets a string pair such as <c>("namespace N { public partial class C {", "}}")</c>.</summary>
    public static (string CodeForOpeningDefinition, string CodeForClosingDefinition) GenerateOpeningClosingTypeDefinitionCode(IMethodSymbol methodSymbol)
    {
        var openingDefinitionCode = new StringBuilder();
        var closingBracketCount = 0;
        if (!methodSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            openingDefinitionCode.Append($$"""namespace {{methodSymbol.ContainingNamespace}} {""");
            closingBracketCount++;
        }

        var typeDeclaretionChain = new List<INamedTypeSymbol>();
        for (var currentTypeSymbol = methodSymbol.ContainingType;
            currentTypeSymbol != null;
            currentTypeSymbol = currentTypeSymbol.ContainingType)
        {
            typeDeclaretionChain.Add(currentTypeSymbol);
        }

        foreach (var typeSymbol in typeDeclaretionChain.AsEnumerable().Reverse()) // enumerate from an outermost type to an innermost type
        {
            if (openingDefinitionCode.Length > 0)
            {
                openingDefinitionCode.Append(' ');
            }

            openingDefinitionCode.Append(ToTypeDeclarationBeforeOpeningBrace(typeSymbol, true));
            openingDefinitionCode.Append(" {");
            closingBracketCount++;
        }

        return (openingDefinitionCode.ToString(), new string('}', count: closingBracketCount));
    }
}
