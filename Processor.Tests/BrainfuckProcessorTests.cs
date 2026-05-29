using Esolang.Processor;
using System.IO.Pipelines;
using System.Text;

namespace Esolang.Brainfuck.Processor.Tests;

[TestClass]
public class BrainfuckProcessorTests(TestContext TestContext)
{
#pragma warning disable MSTEST0054 // TestContext.CancellationTokenSource.Token の代わりに TestContext.CancellationToken を使用する
    CancellationToken CancellationToken => TestContext.CancellationTokenSource.Token;
#pragma warning restore MSTEST0054 // TestContext.CancellationTokenSource.Token の代わりに TestContext.CancellationToken を使用する
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
                => [source, input, expected];
        }
    }
    [TestMethod]
    [DynamicData(nameof(RunAndOutputStringTestData))]
    public async Task RunAndOutputStringAsyncTest(string source, string? input, string? expected = default)
    {
        var pipe = new Pipe();
        var enumerable = new BrainfuckSequenceEnumerable(source);
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckProcessor(sequences, input: pipe.Reader);
        var awaiter = runner.RunAndOutputStringAsync(CancellationToken);
        if (!string.IsNullOrEmpty(input))
        {
            await pipe.Writer.WriteAsync(Encoding.UTF8.GetBytes(input), CancellationToken);
            await pipe.Writer.CompleteAsync();
        }
        var actual = await awaiter;
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    [DynamicData(nameof(RunAndOutputStringTestData))]
    public async Task RunAndOutputStringTest(string source, string? input, string? expected = default)
    {
        var pipe = new Pipe();
        var enumerable = new BrainfuckSequenceEnumerable(source.AsMemory());
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckProcessor(sequences, input: pipe.Reader);
        var awaiter = Task<string?>.Factory.StartNew(() => runner.RunAndOutputString(), CancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
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
        var runner = new BrainfuckProcessor("]");
        var (sequence, _, _) = runner;
        CollectionAssert.AreEqual(new[] { BrainfuckSequence.End }, sequence.ToArray());
        TestContext.WriteLine(runner.ToString());
    }
    [TestMethod]
    public void BrainfuckRunnerWithOptionTest()
    {
        {
            TestShared.BrainfuckOptions options = new();
            var runner = new BrainfuckProcessor("]", options);
            var (sequence, _, _) = runner;
            CollectionAssert.AreEqual(new[] { BrainfuckSequence.End }, sequence.ToArray());
            TestContext.WriteLine(runner.ToString());
        }
        {
            TestShared.BrainfuckOptions options = new();
            var runner = new BrainfuckProcessor("[", options);
            var (sequence, _, _) = runner;
            CollectionAssert.AreEqual(new[] { BrainfuckSequence.Begin }, sequence.ToArray());
            TestContext.WriteLine(runner.ToString());
        }

    }

    [TestMethod]
    public void RunToEnd_TextIo_ReturnsZeroAndWritesOutput()
    {
        var runner = new BrainfuckProcessor("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.");
        using var output = new StringWriter();

#pragma warning disable CS0618
        var exitCode = runner.RunToEnd(null, output, CancellationToken);
#pragma warning restore CS0618

        Assert.AreEqual(0, exitCode);
        Assert.AreEqual("A", output.ToString());
    }

    [TestMethod]
    public async Task RunToEndAsync_PipeIo_ReturnsZeroAndWritesOutput()
    {
        var runner = new BrainfuckProcessor("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.");
        var output = new Pipe();

        var exitCode = await runner.RunToEndAsync(PipeReader.Create(Stream.Null), output.Writer, CancellationToken);
        await output.Writer.CompleteAsync();

        var result = await output.Reader.ReadAsync(CancellationToken);
        using var buffer = new MemoryStream();
        foreach (var segment in result.Buffer)
            buffer.Write(segment.ToArray(), 0, segment.Length);
        var text = Encoding.UTF8.GetString(buffer.ToArray());
        output.Reader.AdvanceTo(result.Buffer.End);
        await output.Reader.CompleteAsync();

        Assert.AreEqual(0, exitCode);
        Assert.AreEqual("A", text);
    }

    [TestMethod]
    public async Task RunToEndAsync_TextIo_ReturnsZeroAndWritesOutput()
    {
        var runner = new BrainfuckProcessor("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.");
        using var input = new StringReader("");
        using var output = new StringWriter();

        var exitCode = await runner.RunToEndAsync(input: input, output: output, cancellationToken: CancellationToken);

        Assert.AreEqual(0, exitCode);
        Assert.AreEqual("A", output.ToString());
    }

    [TestMethod]
    public void RunToEnd_PipeIo_ReturnsZeroAndWritesOutput()
    {
        var runner = new BrainfuckProcessor("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.");
        var input = PipeReader.Create(Stream.Null);
        var output = new Pipe();

#pragma warning disable CS0618
        var exitCode = runner.RunToEnd(input, output.Writer, CancellationToken);
#pragma warning restore CS0618
        output.Writer.Complete();

        if (output.Reader.TryRead(out var result))
        {
            var text = Encoding.UTF8.GetString(result.Buffer.First.Span.ToArray());
            Assert.AreEqual("A", text);
            output.Reader.AdvanceTo(result.Buffer.End);
        }
        else
        {
            Assert.Fail("Output was not produced");
        }
        Assert.AreEqual(0, exitCode);
    }
}
