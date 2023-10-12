using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class CommentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // noop
                var sequences = new[] { Comment }.AsMemory();
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
            static object?[] ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext expected)
                => new object?[] { context, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteAsyncTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var actual = await new CommentCommand(context).ExecuteAsync(token);
        Assert.AreEqual(expected, actual);
    }
}