using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    [TestMethod]
    public void SourceGeneratorTest()
    {
        InitializeCompilation();
        var source = $$"""
        using Brainfuck;
        namespace TestProject;
        partial class TestClass
        {
            [GenerateBrainfuckMethod("++++++++++++++++++++++++++++++++++++.")]
            public static partial string SampleMethod(string input);
        }
        """;
        var preprocessorSymbols = new[] { "NET7_0_OR_GREATER" };

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
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

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
        var result = driver.GetRunResult().Results.Single();
        Assert.IsNotNull(result);
    }
}
