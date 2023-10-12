using Microsoft.VisualStudio.TestTools.UnitTesting;

/* プロジェクト 'Brainfuck.Core.Tests (net48)' からのマージされていない変更
前:
using static Brainfuck.BrainfuckSequence;
後:
using System.Collections;
*/
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

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
                BrainfuckContext before = new(
                    Sequences: sequences,
                    Stack: stack,
                    StackIndex: 1
                );
                yield return ExecuteAsyncTest(
                    before,
                    before with
                    {
                        SequencesIndex = 1,
                        StackIndex = 0,
                    }
                );
            }
            {
                // no op pattern.
                var sequences = new[] { DecrementPointer }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                BrainfuckContext before = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    before,
                    before with
                    {
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

        var result = await new DecrementPointerCommand(context).ExecuteAsync(token);
        Assert.AreEqual(accept, result);
    }
}