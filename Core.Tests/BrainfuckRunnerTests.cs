using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Pipelines;
using System.Text;

namespace Brainfuck.Tests;

[TestClass]
public class BrainfuckRunnerTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> RunAndOutputStringTestData
    {
        get
        {
            yield return RunAndOutputStringTest(
                source: "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.",
                expected: "Hello, world!"
            );
            yield return RunAndOutputStringTest(
                source: "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.+.+.>++++++++++.",
                expected: "ABC\n"
            );
            yield return RunAndOutputStringTest(
                source: "++++++[>++++++++<-]++++++++++[>.+<-]",
                expected: "0123456789"
            );
            yield return RunAndOutputStringTest(
                source: "++++++[>++++++++<-]++++++++++[>.+<-]",
                expected: "0123456789"
            );
            yield return RunAndOutputStringTest(
                source: "+[,.]",
                input: "1234567890",
                expected: "1234567890\0"
            );
            static object?[] RunAndOutputStringTest(string source, string? input = default, string? expected = default)
                => new object?[] { source, input, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(RunAndOutputStringTestData))]
    public async Task RunAndOutputStringAsyncTest(string source, string? input, string? expected = default)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        var enumerable = new BrainfuckSequenceEnumerable(source);
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckRunner(sequences, input: pipe.Reader);
        var awaiter = runner.RunAndOutputStringAsync(token);
        if (!string.IsNullOrEmpty(input))
        {
            await pipe.Writer.WriteAsync(Encoding.UTF8.GetBytes(input), token);
            await pipe.Writer.CompleteAsync();
        }
        var actual = await awaiter;
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    [DynamicData(nameof(RunAndOutputStringTestData))]
    public void RunAdnOutputStringTest(string source, string? input, string? expected = default)
    {
        var pipe = new Pipe();
        var enumerable = new BrainfuckSequenceEnumerable(source);
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckRunner(sequences, input: pipe.Reader);
        if (!string.IsNullOrEmpty(input))
        {
            var input2 = Encoding.UTF8.GetBytes(input);
            var dest = pipe.Writer.GetSpan(input2.Length);
            input2.CopyTo(dest);
            pipe.Writer.Advance(input2.Length);
            pipe.Writer.Complete();
        }
        var actual = runner.RunAndOutputString();
        Assert.AreEqual(expected, actual);
    }
}
