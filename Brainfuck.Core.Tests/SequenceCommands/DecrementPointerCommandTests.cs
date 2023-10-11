using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brainfuck.BrainfuckSequence;
using System.Collections.Immutable;


namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass]
public class DecrementPointerCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // stackPointer -1 
                var sequences = new[] { DecrementPointer }.AsMemory();
                var stack = ImmutableList.Create<byte>(0, 0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack,
                    stackIndex: 1
                ), new(
                    sequences: sequences,
                    stack: stack,
                    sequencesIndex: 1
                ));
            }
            {
                // no op pattern.
                var sequences = new[] { DecrementPointer }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: stack,
                    sequencesIndex: 1
                ));
            }
            static object[] ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext accept)
                => new object[] { context, accept };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext accept)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var result = await new DecrementPointerCommand(context).ExecuteAsync(token);
        Assert.AreEqual(accept, result);
    }
}