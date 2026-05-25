using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Reflection;
using System.IO;
using System.IO.Pipelines;
using Microsoft.Extensions.Logging;

namespace Esolang.Brainfuck.Generator.VisualBasic.Tests;

[TestClass]
public class GeneratorTests
{
    public TestContext TestContext { get; set; } = default!;
#pragma warning disable MSTEST0054 // TestContext.CancellationTokenSource.Token の代わりに TestContext.CancellationToken を使用する
    CancellationToken CancellationToken => TestContext.CancellationTokenSource.Token;
#pragma warning restore MSTEST0054 // TestContext.CancellationTokenSource.Token の代わりに TestContext.CancellationToken を使用する
    Compilation baseCompilation = default!;

    [TestInitialize]
    public void InitializeCompilation()
    {
        IEnumerable<PortableExecutableReference> references;
        {
            references =
#if NET10_0_OR_GREATER
            Net100.References.All
#elif NET9_0_OR_GREATER
            Net90.References.All
#elif NET8_0_OR_GREATER
            Net80.References.All
#elif NET6_0_OR_GREATER
            Net60.References.All
#elif NET472_OR_GREATER
            Net472.References.All
#endif
#if NET47_OR_GREATER || NET5_0_OR_GREATER
                .Concat(
                    Enumerable.Empty<string>()
#if NET5_0 || NET6_0 || NET7_0 || NET8_0
                    .Append(typeof(Pipe).Assembly.Location)
                    .Append(typeof(ILogger).Assembly.Location)

#elif NET9_0_OR_GREATER
                    .Append(typeof(ILogger).Assembly.Location)
#elif NET472_OR_GREATER
                    .Append(typeof(Pipe).Assembly.Location)
                    .Append(typeof(Span<>).Assembly.Location)
                    .Append(typeof(System.Runtime.CompilerServices.Unsafe).Assembly.Location)
                    .Append(typeof(ValueTask<>).Assembly.Location)
                    .Append(typeof(IAsyncEnumerable<>).Assembly.Location)
                    .Append(typeof(ILogger).Assembly.Location)
#endif
                    .Select(x => MetadataReference.CreateFromFile(x))
                )
#endif
            ;
        }
        baseCompilation = VisualBasicCompilation.Create("generatortest",
            references: references,
            options: new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    GeneratorDriver RunGeneratorsAndUpdateCompilation(string source, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
    {
        var generator = new BrainfuckGenerator();
        var driver = VisualBasicGeneratorDriver.Create([generator.AsSourceGenerator()]);

        var compilation = baseCompilation.AddSyntaxTrees(VisualBasicSyntaxTree.ParseText(source, path: "direct.vb"));

        return driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics, cancellationToken);
    }

    void LogWriteLine(string message) => TestContext.WriteLine(message);

    void LogDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.IsEmpty) return;
        LogWriteLine($"diagnostics:\n{string.Join("\n", diagnostics)}");
    }
    void LogDiagnostics(Compilation compilation)
    {
        var diagnostics = compilation.GetDiagnostics(CancellationToken);
        LogWriteLine($"compilation-diagnostics:\n{string.Join("\n", diagnostics)}");
    }

    void LogDiagnostics(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
    {
        LogDiagnostics(diagnostics);
        LogDiagnostics(compilation);
        LogSource(compilation);
    }

    void LogSource(Compilation compilation) => LogSource(compilation.SyntaxTrees);
    void LogSource(IEnumerable<SyntaxTree> trees) => LogWriteLine($"source: \n{string.Join("\n", trees.Select(tree => $"// {tree.FilePath}\n{tree}"))}");

    void AssertNoErrors(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
    {
        Assert.IsEmpty(diagnostics);
        var diagnostics2 = compilation.GetDiagnostics(CancellationToken);
        Assert.IsEmpty(diagnostics2);
    }

    void AssertNonHiddenDiagnostics(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
    {
        var significant = diagnostics.Where(d => d.Severity > DiagnosticSeverity.Hidden).ToImmutableArray();
        Assert.IsEmpty(significant);
        var diagnostics2 = compilation.GetDiagnostics(CancellationToken).Where(d => d.Severity > DiagnosticSeverity.Hidden).ToImmutableArray();
        Assert.IsEmpty(diagnostics2);
    }

    (TestShared.AssemblyLoadContext Context, Assembly Assembly) Emit(Compilation compilation, TestShared.AssemblyLoadContext? context = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        var emitResult = compilation.Emit(stream, pdbStream: pdbStream, cancellationToken: cancellationToken);
        if (!emitResult.Success)
        {
            foreach (var diag in emitResult.Diagnostics) LogWriteLine(diag.ToString());
            Assert.IsTrue(emitResult.Success, "Compilation failed");
        }
        stream.Seek(0, SeekOrigin.Begin);
        pdbStream.Seek(0, SeekOrigin.Begin);
        
        var isNew = context is null;
        context ??= new TestShared.AssemblyLoadContext();
        try
        {
            var assembly = context.LoadFromStream(stream, pdbStream);
            return (context, assembly);
        }
        catch (Exception)
        {
            if (isNew) context?.Dispose();
            throw;
        }
    }

    [TestMethod]
    public void Generator_GeneratesAttribute()
    {
        var inputCompilation = VisualBasicCompilation.Create("TestAssembly",
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new(OutputKind.DynamicallyLinkedLibrary));
            
        VisualBasicParseOptions parseOptions = new(LanguageVersion.VisualBasic12);
        var generator = new BrainfuckGenerator();
        var driver = VisualBasicGeneratorDriver.Create(
            generators:[generator.AsSourceGenerator()],    
            driverOptions: new(default, trackIncrementalGeneratorSteps:true)
        ).WithUpdatedParseOptions(parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var compilation, out var diagnostics, cancellationToken: CancellationToken) as VisualBasicGeneratorDriver;
        Assert.IsNotNull(driver);
        try
        {
            AssertNoErrors(diagnostics, compilation);
            var runResult = driver.GetRunResult();
            AssertNoErrors(runResult.Diagnostics, compilation);
            Assert.HasCount(1, runResult.GeneratedTrees);
            Assert.Contains(v => v.ToString().Contains("GenerateBrainfuckMethodAttribute"), runResult.GeneratedTrees);
            var (context, asm) = Emit(compilation, cancellationToken: CancellationToken);
            using (context)
            {
                var attrType = asm.GetType("Esolang.Brainfuck.GenerateBrainfuckMethodAttribute");
                Assert.IsNotNull(attrType);
            }
            LogDiagnostics(diagnostics, compilation);
        }
        catch (AssertFailedException)
        {
            LogDiagnostics(diagnostics, compilation);
            throw;
        }
    }

    [TestMethod]
    public async Task Generator_EmitsValidCode_WithIOParameters()
    {
        var source = """
    Imports System.IO

    Public Partial Class TestClass
        <Esolang.Brainfuck.GenerateBrainfuckMethod("+")>
        Public Partial Sub SampleMethod(output As TextWriter, input As TextReader)
        End Sub
    End Class
    """;
        var inputCompilation = VisualBasicCompilation.Create("TestAssembly",
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TextReader).Assembly.Location)
            ],
            options: new(OutputKind.DynamicallyLinkedLibrary)
        );

        inputCompilation = inputCompilation.AddSyntaxTrees(
            VisualBasicSyntaxTree.ParseText(source, path:"direct.vb", cancellationToken: CancellationToken)
        );

        var generator = new BrainfuckGenerator();
        var driver = VisualBasicGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()], 
            driverOptions: new(default, trackIncrementalGeneratorSteps:true)
        );

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var compilation, out var diagnostics, cancellationToken: CancellationToken) as VisualBasicGeneratorDriver;
        Assert.IsNotNull(driver);
        try {
            AssertNonHiddenDiagnostics(diagnostics, compilation);

            var runResult = driver.GetRunResult();

        
            AssertNonHiddenDiagnostics(runResult.Diagnostics, compilation);
            Assert.IsNotEmpty(runResult.GeneratedTrees);

            var (context, asm) = Emit(compilation, cancellationToken: CancellationToken);
            using (context)
            {
                var classType = asm.GetType("TestClass");
                Assert.IsNotNull(classType);
                var method = classType.GetMethod("SampleMethod");
                Assert.IsNotNull(method);
            }

        } catch (AssertFailedException)
        {
            LogDiagnostics(diagnostics, compilation);
            throw;
        }
    }
}
