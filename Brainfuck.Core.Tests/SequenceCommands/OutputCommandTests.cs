using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.IO.Pipelines;
using static Brainfuck.BrainfuckSequence;
using Command = Brainfuck.Core.SequenceCommands.OutputCommand;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class OutputCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // input set.
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
            static object[] ExecuteAsyncTest(BrainfuckContext context, byte[] output, BrainfuckContext expected)
                => new object[] { context, SerializableArrayWrapper.Create(output), expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, SerializableArrayWrapper<byte> outputExpected, BrainfuckContext expected)
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
        Assert.AreEqual(expected, actual);
        var outputActual = stream.ToArray();
        CollectionAssert.AreEqual(outputExpected.Array, outputActual);
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
