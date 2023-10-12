using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class BeginCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // while(false) { ← before
                // }
                // ← after
                var sequences = new[] { Begin, Comment, End }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                BrainfuckContext before = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    before,
                    before with
                    {
                        SequencesIndex = 3,
                    });
            }
            {
                // while(true) { ← before
                // ← after
                // }
                var sequences = new[] { Begin, Comment, End }.AsMemory();
                var stack = ImmutableList.Create<byte>(1);
                BrainfuckContext before = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    before,
                    before with
                    {
                        SequencesIndex = 1,
                    });
            }
            {
                // invalid pattern 1
                var sequences = new[] { Comment, Begin, Comment, Comment }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 1
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 2,
                    }
                );
            }
            {
                // invalid pattern 2
                var sequences = new[] { Begin, Begin, End }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
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

        var result = await new BeginCommand(context).ExecuteAsync(token);
        Assert.AreEqual(accept, result);
    }
}