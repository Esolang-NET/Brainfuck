using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brainfuck.BrainfuckSequence;
using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class IncrementCurrentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // currentStack +1
                var sequences = new[] { IncrementCurrent }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: ImmutableList.Create<byte>(1),
                    sequencesIndex: 1
                ));
            }
            {
                // stackPointer +1 overflow 255 → 0
                var sequences = new[] { IncrementCurrent }.AsMemory();
                var stack = ImmutableList.Create(byte.MaxValue);
                yield return ExecuteAsyncTest(new(
                    sequences: sequences,
                    stack: stack
                ), new(
                    sequences: sequences,
                    stack: ImmutableList.Create(byte.MinValue),
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

        var result = await new IncrementCurrentCommand(context).ExecuteAsync(token);
        Assert.AreEqual(accept, result);
    }
}