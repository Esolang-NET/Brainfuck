using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brainfuck.BrainfuckSequence;
using System.Collections.Immutable;
using System.IO.Pipelines;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class InputCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // stackPointer +1 (and extends stack)
                var sequences = new[] { Input }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new byte[] { 1 }, new(
                    sequences: sequences,
                    stack: ImmutableList.Create<byte>(1),
                    sequencesIndex: 1
                ));
            }
            {
                // stackPointer +1
                var sequences = new[] { Input }.AsMemory();
                var stack = ImmutableList.Create<byte>(0, 0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), Array.Empty<byte>() 
                ,new(
                    sequences: sequences,
                    stack: stack,
                    sequencesIndex: 1,
                    stackIndex: 1
                ));
            }
            static object[] ExecuteAsyncTest(BrainfuckContext context, byte[] input, BrainfuckContext accept)
                => new object[] { context, input, accept };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, byte[] input, BrainfuckContext accept)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        context = new BrainfuckContext(
            sequences: context.Sequences,
            sequencesIndex: context.SequencesIndex,
            stack: context.Stack,
            stackIndex: context.StackIndex,
            input: pipe.Reader,
            output: context.Output
        );
        accept = new BrainfuckContext(
            sequences: accept.Sequences,
            sequencesIndex: accept.SequencesIndex,
            stack: accept.Stack,
            stackIndex: accept.StackIndex,
            input: pipe.Reader,
            output: accept.Output
        );

        var waiter = new IncrementPointerCommand(context).ExecuteAsync(token);
        await pipe.Writer.WriteAsync(input, token);
        var result = await waiter;
        Assert.AreEqual(accept, result);
    }

}