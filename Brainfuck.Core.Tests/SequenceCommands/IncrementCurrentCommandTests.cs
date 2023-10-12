using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

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
                BrainfuckContext before = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    before,
                    before with
                    {
                        Stack = ImmutableList.Create<byte>(1),
                        SequencesIndex = 1,
                    }
                );
            }
            {
                // stackPointer +1 overflow 255 → 0
                var sequences = new[] { IncrementCurrent }.AsMemory();
                var stack = ImmutableList.Create(byte.MaxValue);
                BrainfuckContext before = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    before,
                    before with
                    {
                        Stack = ImmutableList.Create(byte.MinValue),
                        SequencesIndex = 1,
                    }
                );
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