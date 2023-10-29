using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Reflection;
#if NET5_0_OR_GREATER
using System.Runtime.Loader;
using System.Threading;
#endif

namespace Brainfuck.Analyzer.Tests;

[TestClass]
public class BrainfuckMethodGeneratorTests
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
                if (fileName.EndsWith("Native.dll")) return false;
                return fileName.StartsWith("System") || (fileName is "mscorlib.dll" or "netstandard.dll");
            });

        PortableExecutableReference[] references;
        {
            // 依存DLLがある場合はそれも追加しておく
            systemAssemblies = systemAssemblies.Append(typeof(System.IO.Pipelines.Pipe).Assembly.Location);
            systemAssemblies = systemAssemblies.Append(typeof(Span<>).Assembly.Location);
            systemAssemblies = systemAssemblies.Append(typeof(System.Runtime.CompilerServices.Unsafe).Assembly.Location);
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

    GeneratorDriver RunGeneratorsAndUpdateCompilation(string source, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics)
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
            var generator = new BrainfuckMethodGenerator();
            var sourceGenerator = generator.AsSourceGenerator();
            driver = CSharpGeneratorDriver.Create(
                generators: new ISourceGenerator[] { sourceGenerator },
                driverOptions: new(default, trackIncrementalGeneratorSteps: true)
            ).WithUpdatedParseOptions(parseOptions);
        }
        var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));

        // Run the generator
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics);
    }

    class AssemblyAndScope : IDisposable
    {
        public Assembly Assembly { get; init; }
        readonly IDisposable? disposable;
        public AssemblyAndScope(Assembly assembly): this(assembly, null) { }
        public AssemblyAndScope(Assembly assembly, IDisposable? disposable) => (Assembly, this.disposable) = (assembly, disposable);
        public void Dispose() => disposable?.Dispose();
    }
    AssemblyAndScope Emit(Compilation compilation)
    {
        var cancellationToken = TestContext.CancellationTokenSource.Token;
#if NET5_0_OR_GREATER
        MemoryStream? disposable = null;
        try {
            var assemblyStream = new MemoryStream();
            disposable = assemblyStream;
            var emitResult = compilation.Emit(assemblyStream, cancellationToken: cancellationToken);
            assemblyStream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(emitResult.Success);
            var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
            return new AssemblyAndScope(assembly, disposable);
        }catch{
            disposable?.Dispose();
            throw;
        }
#else
        var dllFileName = Path.Combine(TestContext.TestRunResultsDirectory, "dynamiclinklib.dll");
        var emitResult = compilation.Emit(dllFileName, cancellationToken: cancellationToken);
        Assert.IsTrue(emitResult.Success);
        using var stream = new FileStream(dllFileName, FileMode.Open, FileAccess.Read, FileShare.None);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        memory.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(memory.ToArray());
        return new AssemblyAndScope(assembly);
#endif
    }

    [TestMethod]
    public void SourceGeneratorTest()
    {
        TestContext.CancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        var source = $$"""
        using Brainfuck;
        namespace TestProject;
        partial class TestClass
        {
            [GenerateBrainfuckMethod("+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.")]
            public static partial string SampleMethod();
        }
        """;
        var cancellationToken = TestContext.CancellationTokenSource.Token;
        RunGeneratorsAndUpdateCompilation(source, out var outputCompilation, out var diagnostics);

        if (!diagnostics.IsEmpty)
        {
            foreach (var diagnostic in diagnostics)
                TestContext.WriteLine($"{diagnostic}");
        }
        Assert.IsTrue(diagnostics.IsEmpty);
        Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
        var diagnostics2 = outputCompilation.GetDiagnostics();
        if (!diagnostics2.IsEmpty)
        {
            foreach (var diagnostic in diagnostics2)
                TestContext.WriteLine($"{diagnostic}");
        }
        Assert.IsTrue(diagnostics2.IsEmpty);
        using var emitAssembly = Emit(outputCompilation);
        var assembly = emitAssembly.Assembly;
        var testClassType = assembly.GetType("TestProject.TestClass");
        Assert.IsNotNull(testClassType);
        var sampleMethod = testClassType.GetMethod("SampleMethod");
        Assert.IsNotNull(sampleMethod);
        var returnvalue = (string?)sampleMethod.Invoke(null, Array.Empty<object?>());
        Assert.AreEqual("Hello World!", returnvalue);
    }
}
