using Basic.Reference.Assemblies;
using Esolang.Brainfuck.Generator.Sequences;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections;
using TUnit.Assertions.Enums;

namespace Esolang.Brainfuck.Generator.Tests;

public class UtilsAndNestableSequenceTests
{
    [Test]
    public async Task SanitizeForFileName_ReplacesKnownTokens()
    {
        var actual = Utils.SanitizeForFileName("global::System.Collections.Generic.List<string?>");

        await Assert.That(actual).IsEqualTo("System.Collections.Generic.List_string_");
    }

    [Test]
    public async Task ToTypeDeclarationBeforeOpeningBrace_CoversKindsAndModifiers()
    {
        var compilation = CreateCompilation(
            """
            namespace T;
            public partial class GenericType<T1> {}
            public interface IAny {}
            public readonly ref struct RefStructType {}
            public record RecType(int X);
            public delegate void DAny();
            """);

        var genericType = compilation.GetTypeByMetadataName("T.GenericType`1")!;
        var interfaceType = compilation.GetTypeByMetadataName("T.IAny")!;
        var refStructType = compilation.GetTypeByMetadataName("T.RefStructType")!;
        var recordType = compilation.GetTypeByMetadataName("T.RecType")!;
        var delegateType = compilation.GetTypeByMetadataName("T.DAny")!;

        var genericDecl = Utils.ToTypeDeclarationBeforeOpeningBrace(genericType, isPartial: true);
        var interfaceDecl = Utils.ToTypeDeclarationBeforeOpeningBrace(interfaceType, isPartial: true);
        var refStructDecl = Utils.ToTypeDeclarationBeforeOpeningBrace(refStructType, isPartial: false);
        var recordDecl = Utils.ToTypeDeclarationBeforeOpeningBrace(recordType, isPartial: true);

        await Assert.That(genericDecl).IsEqualTo("public partial class GenericType<T1>");
        await Assert.That(interfaceDecl).IsEqualTo("public partial interface IAny");
        await Assert.That(refStructDecl).IsEqualTo("public readonly ref struct RefStructType");
        await Assert.That(recordDecl).IsEqualTo("public partial record class RecType");

        try
        {
            Utils.ToTypeDeclarationBeforeOpeningBrace(delegateType, isPartial: true);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }

    [Test]
    public async Task GenerateOpeningClosingTypeDefinitionCode_WithNamespaceAndNestedType()
    {
        var compilation = CreateCompilation(
            """
            namespace T;
            public class Outer
            {
                public class Inner
                {
                    public void M() {}
                }
            }
            """);

        var method = (IMethodSymbol)compilation.GetTypeByMetadataName("T.Outer+Inner")!.GetMembers("M").Single();
        var (opening, closing) = Utils.GenerateOpeningClosingTypeDefinitionCode(method);

        await Assert.That(opening).IsEqualTo("namespace T { public partial class Outer { public partial class Inner {");
        await Assert.That(closing).IsEqualTo("}}}");
    }

    [Test]
    public async Task GenerateOpeningClosingTypeDefinitionCode_WithGlobalNamespace()
    {
        var compilation = CreateCompilation(
            """
            public class C
            {
                public void M() {}
            }
            """);

        var method = (IMethodSymbol)compilation.GetTypeByMetadataName("C")!.GetMembers("M").Single();
        var (opening, closing) = Utils.GenerateOpeningClosingTypeDefinitionCode(method);

        await Assert.That(opening).IsEqualTo("public partial class C {");
        await Assert.That(closing).IsEqualTo("}");
    }

    [Test]
    public async Task NestableSequence_Enumerator_FlattensNestedSequences()
    {
        var b0 = new Sequence(0, BrainfuckSequence.Begin, "[".AsMemory());
        var s1 = new Sequence(1, BrainfuckSequence.IncrementCurrent, "+".AsMemory());
        var b2 = new Sequence(2, BrainfuckSequence.Begin, "[".AsMemory());
        var s3 = new Sequence(3, BrainfuckSequence.Output, ".".AsMemory());
        var e2 = new Sequence(4, BrainfuckSequence.End, "]".AsMemory());
        var e0 = new Sequence(5, BrainfuckSequence.End, "]".AsMemory());

        var inner = new NestableSequence([s3], b2, e2);
        var outer = new NestableSequence([s1, inner, new UnknownNestable()], b0, e0);

        var flattened = outer.ToArray();

        await Assert.That(flattened).IsEquivalentTo((Sequence[])[b0, s1, b2, s3, e2, e0], CollectionOrdering.Matching);
    }

    [Test]
    public async Task NestableSequence_NonGenericEnumerator_AndToStringWithNullNest()
    {
        var begin = new Sequence(0, BrainfuckSequence.Begin, "[".AsMemory());
        var end = new Sequence(1, BrainfuckSequence.End, "]".AsMemory());
        var seq = new NestableSequence([], begin, end);

        var nonGeneric = ((IEnumerable)seq).GetEnumerator();
        await Assert.That(nonGeneric.MoveNext()).IsTrue();

        var withNullNest = new NestableSequence(null!, begin, end);
        var text = withNullNest.ToString();
        await Assert.That(text).Contains("NestableSequence");
    }

    static CSharpCompilation CreateCompilation(string source)
    {
        IEnumerable<PortableExecutableReference> references;
#if NET10_0_OR_GREATER
        references = Net100.References.All;
#elif NET9_0_OR_GREATER
        references = Net90.References.All;
#elif NET8_0_OR_GREATER
        references = Net80.References.All;
#elif NET6_0_OR_GREATER
        references = Net60.References.All;
#elif NET472_OR_GREATER
        references = Net472.References.All;
#else
        references = Enumerable.Empty<PortableExecutableReference>();
#endif

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        return CSharpCompilation.Create(
            assemblyName: "coverage-tests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    sealed record UnknownNestable : INestableSequence;
}
