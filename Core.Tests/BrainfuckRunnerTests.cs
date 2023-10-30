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
                source: "+[[-],.]",
                input: "1234567890",
                expected: "1234567890"
            );
            yield return RunAndOutputStringTest(
                source: "++",
                expected: null
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
    public async Task RunAndOutputStringTest(string source, string? input, string? expected = default)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        var enumerable = new BrainfuckSequenceEnumerable(source.AsMemory());
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckRunner(sequences, input: pipe.Reader);
        var awaiter = Task<string?>.Factory.StartNew(() => runner.RunAndOutputString(), token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        WriteIsNotNullOrEmpty(input, pipe);
        var actual = await awaiter;
        Assert.AreEqual(expected, actual);
        static void WriteIsNotNullOrEmpty(string? input, Pipe pipe)
        {
            if (string.IsNullOrEmpty(input))
                return;
            var input2 = Encoding.UTF8.GetBytes(input);
            var dest = pipe.Writer.GetSpan(input2.Length);
            input2.CopyTo(dest);
            pipe.Writer.Advance(input2.Length);
            pipe.Writer.Complete();
        }
    }
    [TestMethod]

    public void BrainfuckRunnerTest()
    {
        var runner = new BrainfuckRunner("]");
        var (sequence, _, _) = runner;
        CollectionAssert.AreEqual(new[] { BrainfuckSequence.End }, sequence.ToArray());
        TestContext.WriteLine(runner.ToString());
    }
    [TestMethod]
    public void BrainfuckRunnerWithOptionTest()
    {
        {
            BrainfuckOptions options = new();
            var runner = new BrainfuckRunner("]", options);
            var (sequence, _, _) = runner;
            CollectionAssert.AreEqual(new[] { BrainfuckSequence.End }, sequence.ToArray());
            TestContext.WriteLine(runner.ToString());
        }
        {
            TestShared.BrainfuckOptions options = new();
            var runner = new BrainfuckRunner("[", options);
            var (sequence, _, _) = runner;
            CollectionAssert.AreEqual(new[] { BrainfuckSequence.Begin }, sequence.ToArray());
            TestContext.WriteLine(runner.ToString());
        }

    }
}
