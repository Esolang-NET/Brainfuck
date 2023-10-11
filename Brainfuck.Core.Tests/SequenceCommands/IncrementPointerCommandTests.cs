using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brainfuck.BrainfuckSequence;
using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass]
public class IncrementPointerCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // stackPointer +1 (and extends stack)
                var sequences = new[] { IncrementPointer }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: ImmutableList.Create<byte>(0, 0),
                    sequencesIndex: 1,
                    stackIndex: 1
                ));
            }
            {
                // stackPointer +1
                var sequences = new[] { IncrementPointer }.AsMemory();
                var stack = ImmutableList.Create<byte>(0, 0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: stack,
                    sequencesIndex: 1,
                    stackIndex: 1
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

        var result = await new IncrementPointerCommand(context).ExecuteAsync(token);
        Assert.AreEqual(accept, result);
    }
}