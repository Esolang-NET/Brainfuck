using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;

namespace Esolang.Brainfuck.Generator.VisualBasic.Tests;

[TestClass]
public class GeneratorTests
{
    public TestContext TestContext { get; set; } = default!;

    void LogWriteLine(string message) => TestContext.WriteLine(message);

    void LogDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.IsEmpty) return;
        foreach (var diagnostic in diagnostics)
            LogWriteLine($"{diagnostic}");
    }

    void AssertNoErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        if (!diagnostics.IsEmpty)
        {
            LogDiagnostics(diagnostics);
        }
        Assert.IsTrue(diagnostics.IsEmpty, $"Expected 0 diagnostics, found: {string.Join(", ", diagnostics.Select(d => d.Id))}");
    }

    [TestMethod]
    public void Generator_GeneratesAttribute()
    {
        var inputCompilation = VisualBasicCompilation.Create("TestAssembly");
        var generator = new BrainfuckGenerator();
        var driver = VisualBasicGeneratorDriver.Create([generator.AsSourceGenerator()]);

        driver = (VisualBasicGeneratorDriver)driver.RunGenerators(inputCompilation);

        var runResult = driver.GetRunResult();

        try
        {
            AssertNoErrors(runResult.Diagnostics);
            Assert.HasCount(1, runResult.GeneratedTrees);
            Assert.Contains(v => v.ToString().Contains("GenerateBrainfuckMethodAttribute"), runResult.GeneratedTrees);
        }
        catch (AssertFailedException)
        {
            LogDiagnostics(runResult.Diagnostics);
            foreach (var tree in runResult.GeneratedTrees)
            {
                LogWriteLine($"FilePath: {tree.FilePath}");
                LogWriteLine(tree.ToString());
            }
            throw;
        }
    }

    [TestMethod]
    public void Generator_ReportsErrorForNonPartialMethod()
    {
        var source = """
    Imports Esolang.Brainfuck

    Public Class TestClass
        <GenerateBrainfuckMethod("+")>
        Public Shared Sub NonPartialMethod()
        End Sub
    End Class
    """;
        var inputCompilation = VisualBasicCompilation.Create("TestAssembly",
            syntaxTrees: [VisualBasicSyntaxTree.ParseText(source)],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        var generator = new BrainfuckGenerator();
        var driver = VisualBasicGeneratorDriver.Create([generator.AsSourceGenerator()]);

        driver = (VisualBasicGeneratorDriver)driver.RunGenerators(inputCompilation);

        var runResult = driver.GetRunResult();

        Assert.IsTrue(runResult.Diagnostics.Any(d => d.Id == "BF0011"), "Expected BF0011 diagnostic");
    }
    }

