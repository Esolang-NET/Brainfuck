using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass]
public class EndCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteAsyncTestData
    {
        get
        {
            {
                // while(true) {
                // } ← before
                // ← after
                var sequences = new[] { Begin, Comment, End, Comment }.AsMemory();
                var stack = ImmutableList.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 2
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 3,
                    }
                );
            }
            {
                // while(true) {
                // ← after
                // } ← before
                var sequences = new[] { Begin, Comment, End, Comment }.AsMemory();
                var stack = ImmutableList.Create<byte>(1);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 2
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        SequencesIndex = 1,
                    }
                );
            }
            {
                // invalid pattern 1
                var sequences = new[] { Comment, End, Comment }.AsMemory();
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
                var sequences = new[] { End, End, Comment }.AsMemory();
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

        var actual = await new EndCommand(context).ExecuteAsync(token);
        Assert.AreEqual(expected, actual);
    }
}