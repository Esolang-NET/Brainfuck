using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.IO.Pipelines;
using static Brainfuck.BrainfuckSequence;

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
                var stack = ImmutableList.Create<byte>(1);
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
            static object[] ExecuteAsyncTest(BrainfuckContext context, byte[] output, BrainfuckContext accept)
                => new object[] { context, SerializableArrayWrapper.Create(output), accept };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, SerializableArrayWrapper<byte> output, BrainfuckContext accept)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        context = context with
        {
            Output = pipe.Writer,
        };
        accept = accept with
        {
            Output = pipe.Writer,
        };
        using var stream = new MemoryStream();
        var waiter = pipe.Reader.CopyToAsync(stream, token);
        var result = await new OutputCommand(context).ExecuteAsync(token);
        await pipe.Writer.CompleteAsync();
        await waiter;
        stream.Seek(0, SeekOrigin.Begin);
        Assert.AreEqual(accept, result);
        var outputResult = stream.ToArray();
        CollectionAssert.AreEqual(output.Array, outputResult);
    }
}