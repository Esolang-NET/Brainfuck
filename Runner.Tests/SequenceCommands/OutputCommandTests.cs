using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.Collections.Immutable;
using System.IO.Pipelines;
using static Esolang.Brainfuck.BrainfuckSequence;
using static TestShared.ArrayExtensions;
using Command = Esolang.Brainfuck.Runner.SequenceCommands.OutputCommand;

namespace Esolang.Brainfuck.Runner.SequenceCommands.Tests;

[TestClass()]
public class OutputCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteTestData
    {
        get
        {
            {
                // output set.
                var sequences = new[] { Output }.AsMemory();
                var stack = ImmutableArray.Create<byte>(1);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    new byte[] { 1 },
                    context with
                    {
                        SequencesIndex = 1,
                    }
                );
            }
            static object[] ExecuteAsyncTest(TestShared.BrainfuckContext context, byte[] output, TestShared.BrainfuckContext expected)
                => new object[] { context, output.ToSerializable(), expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public async Task ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.Array<byte> outputExpected, TestShared.BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        context = context with
        {
            Output = pipe.Writer,
        };
        expected = expected with
        {
            Output = pipe.Writer,
        };
        using var stream = new MemoryStream();
        var waiter = pipe.Reader.CopyToAsync(stream, token);
        var actual = await new Command(context).ExecuteAsync(token);
        await pipe.Writer.CompleteAsync();
        await waiter;
        stream.Seek(0, SeekOrigin.Begin);
        Assert.AreEqual<BrainfuckContext>(expected, actual);
        var outputActual = stream.ToArray();
        CollectionAssert.AreEqual((byte[])outputExpected, outputActual);
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public void ExecuteTest(TestShared.BrainfuckContext context, TestShared.Array<byte> outputExpected, TestShared.BrainfuckContext expected)
    {
        var pipe = new Pipe();
        context = context with
        {
            Output = pipe.Writer,
        };
        expected = expected with
        {
            Output = pipe.Writer,
        };
        var actual = new Command(context).Execute();
        pipe.Writer.Complete();
        Assert.AreEqual<BrainfuckContext>(expected, actual);
        var outputActual = pipe.Reader.TryRead(out var result) ? result.Buffer.ToArray() : Array.Empty<byte>();
        CollectionAssert.AreEqual((byte[])outputExpected, outputActual);
    }
    [TestMethod]
    public void ExecuteAsync_ThrowTest()
    {
        var token = TestContext.CancellationTokenSource.Token;
        var command = new Command(new BrainfuckContext(Sequences: new[] { Output }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await command.ExecuteAsync(token)); ;
    }
    [TestMethod]
    public void Execute_ThrowTest()
    {
        var command = new Command(new BrainfuckContext(Sequences: new[] { Output }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        Assert.ThrowsException<InvalidOperationException>(() => command.Execute());
    }

    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(true, command.RequiredOutput);
    }
}
