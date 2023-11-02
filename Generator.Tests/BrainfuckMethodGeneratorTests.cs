using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace Brainfuck.Generator.Tests;

[TestClass]
public class MethodGeneratorTests
{
    public TestContext TestContext { get; set; } = default!;
    Compilation baseCompilation = default!;

    [TestInitialize]
    public void InitializeCompilation()
    {
        // running .NET Core system assemblies dir path
        var baseAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var systemAssemblies = Directory.GetFiles(baseAssemblyPath)
            .Where(x =>
            {
                var fileName = Path.GetFileName(x);
                if (!fileName.EndsWith(".dll")) return false;
                if (fileName.EndsWith("Native.dll")) return false;
                return fileName.StartsWith("System") || (fileName is "mscorlib.dll" or "netstandard.dll");
            });

        PortableExecutableReference[] references;
        {
            // 依存DLLがある場合はそれも追加しておく
            systemAssemblies = systemAssemblies.Append(typeof(System.IO.Pipelines.Pipe).Assembly.Location);
            systemAssemblies = systemAssemblies.Append(typeof(Span<>).Assembly.Location);
            systemAssemblies = systemAssemblies.Append(typeof(System.Runtime.CompilerServices.Unsafe).Assembly.Location);
            systemAssemblies = systemAssemblies.Append(typeof(ValueTask<>).Assembly.Location);
#if !NETCOREAPP1_0_OR_GREATER && !NET5_0_OR_GREATER
            var exclude = new HashSet<string>(new string[] {
                "System.tlb",
                "System.Web.tlb",
                "System.Drawing.tlb",
                "System.Windows.Forms.tlb",
                "System.EnterpriseServices.tlb",
                "System.EnterpriseServices.Wrapper.dll",
                "System.EnterpriseServices.Thunk.dll",
            });
            systemAssemblies = systemAssemblies.Where(path => !exclude.Any(exc => path.Contains(exc)));
#endif
            references = systemAssemblies
                .Select(x => MetadataReference.CreateFromFile(x))
                .ToArray();
        }
        var compilation = CSharpCompilation.Create("generatortest",
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>{
                    { "CS1701", ReportDiagnostic.Suppress },
                }));

        baseCompilation = compilation;
    }

    GeneratorDriver RunGeneratorsAndUpdateCompilation(string source, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
    {
        var preprocessorSymbols = new string[] {
#if NETCOREAPP3_0_OR_GREATER
            "NETCOREAPP3_0_OR_GREATER",
#endif
#if NETSTANDARD2_1
            "NETSTANDARD2_1",
#endif
#if NETSTANDARD2_1_OR_GREATER
            "NETSTANDARD2_1_OR_GREATER",
#endif
#if NET5_0_OR_GREATER
            "NET5_0_OR_GREATER",
#endif
#if NET7_0_OR_GREATER
            "NET7_0_OR_GREATER" 
#endif
        };

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp11, preprocessorSymbols: preprocessorSymbols);

        GeneratorDriver driver;
        {
            var generator = new MethodGenerator();
            var sourceGenerator = generator.AsSourceGenerator();
            driver = CSharpGeneratorDriver.Create(
                generators: new ISourceGenerator[] { sourceGenerator },
                driverOptions: new(default, trackIncrementalGeneratorSteps: true)
            ).WithUpdatedParseOptions(parseOptions);
        }
        var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions, path: "direct.cs", encoding: Encoding.UTF8, cancellationToken));

        // Run the generator
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics, cancellationToken);
    }
    (TestShared.AssemblyLoadContext Context, Assembly Assembly) Emit(Compilation compilation, TestShared.AssemblyLoadContext? context = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        var emitResult = compilation.Emit(stream, pdbStream: pdbStream, cancellationToken: cancellationToken);
        if (!emitResult.Success)
            AssertDiagnostics(emitResult.Diagnostics, compilation);
        Assert.IsTrue(emitResult.Success);
        stream.Seek(0, SeekOrigin.Begin);
        pdbStream.Seek(0, SeekOrigin.Begin);
        TestContext.WriteLine($"assembly Length:{stream.Length}");
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
    void OutputSource(IEnumerable<SyntaxTree> syntaxTrees)
    {
        foreach (var tree in syntaxTrees)
        {
            TestContext.WriteLine($"FilePath:{tree.FilePath}\r\nsource:↓\r\n{tree}");
        }
    }
    void OutputDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            TestContext.WriteLine($"{diagnostic}");
    }
    void AssertDiagnostics(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
    {
        if (!diagnostics.IsEmpty)
        {
            OutputDiagnostics(diagnostics);
            OutputSource(compilation.SyntaxTrees);
        }
        Assert.IsTrue(diagnostics.IsEmpty);
    }
    static IEnumerable<object?[]> SourceGeneratorTest1Data
    {
        get
        {
            yield return SourceGeneratorTest1("0.", null);
            yield return SourceGeneratorTest1("1+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.", "Hello, world!");
            static object?[] SourceGeneratorTest1(string source, string? expected)
                => new object?[] { source, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(SourceGeneratorTest1Data))]
    public async Task SourceGeneratorTest(string source, string? expected)
    {
        TestContext.CancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        var cancellationToken = TestContext.CancellationTokenSource.Token;
        source = $$"""
        using Brainfuck;
        namespace TestProject;
        #nullable enable
        partial class TestClass
        {
            [GenerateBrainfuckMethod("{{source}}")]
            public static partial string? SampleMethod();
        }
        """;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics, TestContext.CancellationTokenSource.Token);
        AssertDiagnostics(diagnostics, outputCompilation);
        Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
        AssertDiagnostics(outputCompilation.GetDiagnostics(), outputCompilation);
        var (context, assembly) = Emit(outputCompilation, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        using (context)
        {
            await Task.Factory.StartNew(() =>
            {
                var testClassType = assembly.GetType("TestProject.TestClass");
                Assert.IsNotNull(testClassType);
                var sampleMethod = testClassType.GetMethod("SampleMethod");
                Assert.IsNotNull(sampleMethod);
                try
                {
                    var actual = (string?)sampleMethod.Invoke(null, Array.Empty<object?>());
                    Assert.AreEqual(expected, actual);
                }
                catch (Exception e) when (e is TargetInvocationException or AssertFailedException)
                {
                    OutputSource(outputCompilation.SyntaxTrees);
                    throw;
                }
            }, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
    static IEnumerable<object?[]> ReturnTypeAndParameterPatternsTestData
    {
        get
        {
            yield return ReturnTypeAndParameterPatternsTest(
                "1_1+",
                "void");
            yield return ReturnTypeAndParameterPatternsTest(
                "1_2",
                "System.Threading.Tasks.Task");
            yield return ReturnTypeAndParameterPatternsTest(
                "1_3",
                "System.Threading.Tasks.ValueTask");
            yield return ReturnTypeAndParameterPatternsTest(
                "2_1_1+.",
                "string",
                options: "#nullable disable");
            yield return ReturnTypeAndParameterPatternsTest(
                "2_1_2+.",
                "System.Threading.Tasks.Task<string>",
                options: "#nullable disable");
            yield return ReturnTypeAndParameterPatternsTest(
                "2_1_3+.",
                "System.Threading.Tasks.ValueTask<string>",
                options: "#nullable disable");
            yield return ReturnTypeAndParameterPatternsTest(
                "2_2_1+.",
                "string?",
                options: "#nullable enable");
            yield return ReturnTypeAndParameterPatternsTest(
                "2_2_2+.",
                "System.Threading.Tasks.Task<string?>",
                options: "#nullable enable");
            yield return ReturnTypeAndParameterPatternsTest(
                "2_2_3+.",
                "System.Threading.Tasks.ValueTask<string?>",
                options: "#nullable enable");
            yield return ReturnTypeAndParameterPatternsTest(
                "3_1+.",
                "System.Collections.Generic.IEnumerable<byte>");
#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER //netframework not support IAsyncEnumerable<>
            yield return ReturnTypeAndParameterPatternsTest(
                "3_2+.",
                "System.Collections.Generic.IAsyncEnumerable<byte>");
#endif
            yield return ReturnTypeAndParameterPatternsTest(
                "4_1+.",
                "void",
                "System.IO.Pipelines.PipeWriter output, System.Threading.CancellationToken cancellationToken = default"
                );
            yield return ReturnTypeAndParameterPatternsTest(
                "4_2+.",
                "System.Threading.Tasks.Task",
                "System.IO.Pipelines.PipeWriter output, System.Threading.CancellationToken cancellationToken = default"
                );
            yield return ReturnTypeAndParameterPatternsTest(
                "5_1+,",
                "void",
                "System.IO.Pipelines.PipeReader input, System.Threading.CancellationToken cancellationToken = default"
                );
            yield return ReturnTypeAndParameterPatternsTest(
                "5_2+,",
                "System.Threading.Tasks.Task",
                "System.IO.Pipelines.PipeReader input, System.Threading.CancellationToken cancellationToken = default"
                );
            yield return ReturnTypeAndParameterPatternsTest(
                "5_3+,",
                "void",
                "string input, System.Threading.CancellationToken cancellationToken = default"
                );
            yield return ReturnTypeAndParameterPatternsTest(
                "5_4+,",
                "System.Threading.Tasks.Task",
                "string input, System.Threading.CancellationToken cancellationToken = default"
                );
            static object?[] ReturnTypeAndParameterPatternsTest(string source, string returnType, string parameters = "", string options = "")
                => new object?[] { source, returnType, parameters, options };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ReturnTypeAndParameterPatternsTestData))]
    public void ReturnTypeAndParameterPatternsTest(string source, string returnType, string parameters, string options)
    {
        source = $$"""
        using Brainfuck;
        namespace TestProject;
        {{options}}
        partial class TestClass
        {
            [GenerateBrainfuckMethod("{{source}}")]
            public static partial {{returnType}} SampleMethod({{parameters}});
        }
        """;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics, TestContext.CancellationTokenSource.Token);
        AssertDiagnostics(diagnostics, outputCompilation);
        Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
        AssertDiagnostics(outputCompilation.GetDiagnostics(), outputCompilation);
    }
    static IEnumerable<object?[]> DiagnoticsTestData
    {
        get
        {
            // BF0001: GenerateBrainfuckMethod required first parameter.
            yield return DiagnoticsTest("BF0001", "", "void");
            // BF0002: not support return type int.
            yield return DiagnoticsTest("BF0002", "2_1", "int");
            // BF0002: not support return type string (in #nullable enable)
            yield return DiagnoticsTest("BF0002", "2_2.", "string", options: "#nullable enable"); ;
            // BF0002: not support return type string (in #nullable disabled and not output bf source)
            yield return DiagnoticsTest("BF0002", "2_3", "string", options: "#nullable disable");
            // BF0003: not support parameter type int.
            yield return DiagnoticsTest("BF0003", "3_1", "void", "int param1");
            // BF0003: not support parameter type string (source no input)
            yield return DiagnoticsTest("BF0003", "3_2", "void", "string input");
            // BF0003: not support parameter type System.IO.Pipelines.PipeReader (source no input)
            yield return DiagnoticsTest("BF0003", "3_3", "void", "System.IO.Pipelines.PipeReader input");
            // BF0003: not support parameter type System.IO.Pipelines.PipeWriter (source no output)
            yield return DiagnoticsTest("BF0003", "3_4", "void", "System.IO.Pipelines.PipeWriter output");
            // BF0004: duplicate parameter CancellationToken
            yield return DiagnoticsTest("BF0004", "4_1.", "string", "System.Threading.CancellationToken token1, System.Threading.CancellationToken token2");
            // BF0004: duplicate parameter string
            yield return DiagnoticsTest("BF0004", "4_2,", "void", "string input1, string input2");
            // BF0004: duplicate parameter System.IO.Pipelines.PipeReader
            yield return DiagnoticsTest("BF0004", "4_3,", "void", "System.IO.Pipelines.PipeReader input1, System.IO.Pipelines.PipeReader input2");
            // BF0004: duplicate parameter System.IO.Pipelines.PipeWriter
            yield return DiagnoticsTest("BF0004", "4_4.", "void", "System.IO.Pipelines.PipeWriter output1, System.IO.Pipelines.PipeWriter output2");
            // BF0005: duplicate parameter System.IO.Pipelines.PipeReader and string
            yield return DiagnoticsTest("BF0005", "5_1,", "void", "System.IO.Pipelines.PipeReader input1, string input2");
            // BF0005: duplicate parameter string and System.IO.Pipelines.PipeReader
            yield return DiagnoticsTest("BF0005", "5_2,", "void", "string input1, System.IO.Pipelines.PipeReader input2");
            // BF0006: duplicate return string and parameter System.IO.Pipelines.PipeWriter
            yield return DiagnoticsTest("BF0006", "6_1.", "string", "System.IO.Pipelines.PipeWriter output");
            // BF0006: duplicate return IEnumerable<byte> and parameter System.IO.Pipelines.PipeWriter
            yield return DiagnoticsTest("BF0006", "6_2.", "System.Threading.Tasks.Task<string>", "System.IO.Pipelines.PipeWriter output");
            // BF0006: duplicate return ValueTask<string> and parameter System.IO.Pipelines.PipeWriter
            yield return DiagnoticsTest("BF0006", "6_3.", "System.Threading.Tasks.ValueTask<string>", "System.IO.Pipelines.PipeWriter output");
            // BF0006: duplicate return IEnumerable<byte> and parameter System.IO.Pipelines.PipeWriter
            yield return DiagnoticsTest("BF0006", "6_4.", "System.Collections.Generic.IEnumerable<byte>", "System.IO.Pipelines.PipeWriter output");
#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER  //netframework not support IAsyncEnumerable<>
            // BF0006: duplicate return IAsyncEnumerable<byte> and parameter System.IO.Pipelines.PipeWriter
            yield return DiagnoticsTest("BF0006", "6_5.", "System.Collections.Generic.IAsyncEnumerable<byte>", "System.IO.Pipelines.PipeWriter output");
#endif
            // BF0007: no outuput
            yield return DiagnoticsTest("BF0007", "7_1.", "void");
            // BF0007: no outuput
            yield return DiagnoticsTest("BF0007", "7_2.", "System.Threading.Tasks.Task");
            // BF0007: no outuput
            yield return DiagnoticsTest("BF0007", "7_3.", "System.Threading.Tasks.ValueTask");
            // BF0008: no input
            yield return DiagnoticsTest("BF0008", "8_1,", "void");
            static object?[] DiagnoticsTest(string expected, string source, string returnType, string parameters = "", string options = "")
                => new object?[] { expected, source, returnType, parameters, options };
        }
    }
    [TestMethod]
    [DynamicData(nameof(DiagnoticsTestData))]
    public void DiagnoticsTest(string expected, string source, string returnType, string parameters, string options)
    {
        source = $$"""
        using Brainfuck;
        namespace TestProject;
        {{options}}
        partial class TestClass
        {
            [GenerateBrainfuckMethod("{{source}}")]
            public static partial {{returnType}} SampleMethod({{parameters}});
        }
        """;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics, TestContext.CancellationTokenSource.Token);
        Assert.IsFalse(diagnostics.IsEmpty);
        CollectionAssert.AreEqual(new[] { expected }, diagnostics.Select(v => v.Id).ToArray());
        Assert.AreEqual(2, outputCompilation.SyntaxTrees.Count());
    }
    [TestMethod]
    public void DiagnoticsTest_NoArgumentConstructor()
    {
        var source = $$"""
        using Brainfuck;
        namespace TestProject;
        partial class TestClass
        {
            [GenerateBrainfuckMethod()]
            public static partial void SampleMethod();
        }
        """;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics);
        Assert.IsFalse(diagnostics.IsEmpty);
        CollectionAssert.AreEqual(new[] { "BF0001" }, diagnostics.Select(v => v.Id).ToArray());
        Assert.AreEqual(2, outputCompilation.SyntaxTrees.Count());
    }
    static IEnumerable<object?[]> ModuleSignatureTestData
    {
        get
        {
            yield return ModuleSignatureTest("abstract partial class TestAbstractPartialClass");
            yield return ModuleSignatureTest("sealed partial class TestSealedPartialClass");
            yield return ModuleSignatureTest("partial struct TestPartialStruct");
            yield return ModuleSignatureTest("ref partial struct TestRefPartialStruct");
            yield return ModuleSignatureTest("partial class TestClass: System.Collections.Generic.List<(string Value1, int Value2)>");

            static object?[] ModuleSignatureTest(string signature)
                => new object?[] { signature };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ModuleSignatureTestData))]
    public void ModuleSignatureTest(string signature)
    {
        var source = $$"""
        using Brainfuck;
        namespace TestProject;
        {{signature}}
        {
            [GenerateBrainfuckMethod("0")]
            public static partial void SampleMethod();
        }
        """;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics, TestContext.CancellationTokenSource.Token);
        AssertDiagnostics(diagnostics, outputCompilation);
        Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
        AssertDiagnostics(outputCompilation.GetDiagnostics(), outputCompilation);
    }

    [TestMethod]
    public void AttributeSubParameterTest()
    {
        var source = $$"""
        using Brainfuck;
        namespace TestProject;
        partial class TestClass
        {
            [GenerateBrainfuckMethod("😀😁😂🤣😃😄😅😅😅😅😆😆", IncrementPointer = "😀", DecrementPointer = "😁", IncrementCurrent = "😂", DecrementCurrent = "🤣", Output = "😃", Input = "😄", Begin = "😅", End = "😆")]
            public static partial string SampleMethod(string input);
        }
        """;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics, TestContext.CancellationTokenSource.Token);
        AssertDiagnostics(diagnostics, outputCompilation);
        Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
        AssertDiagnostics(outputCompilation.GetDiagnostics(), outputCompilation);
    }
}
