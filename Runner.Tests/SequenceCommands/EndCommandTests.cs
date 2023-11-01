using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;
using Command = Brainfuck.Runner.SequenceCommands.EndCommand;

namespace Brainfuck.Runner.SequenceCommands.Tests;

[TestClass]
public class EndCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteTestData
    {
        get
        {
            {
                // while(true) {
                // } ← before
                // ← after
                var sequences = new[] { Begin, Comment, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
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
                var sequences = new[] { Begin, Comment, Begin, Comment, End, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(1);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack,
                    SequencesIndex: 5
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
                // loop out end -> other
                var sequences = new[] { Comment, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
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
                // loop out end -> end
                var sequences = new[] { End, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
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
            {
                // invalid pattern 3
                // invalid loop skip loop
                var sequences = new[] { End, End, Comment }.AsMemory();
                var stack = ImmutableArray.Create<byte>(1);
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
            static object?[] ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
                => new object?[] { context, expected };
        }
    }


    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public async Task ExecuteAsyncTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var actual = await new Command(context).ExecuteAsync(token);
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public void ExecuteTest(TestShared.BrainfuckContext context, TestShared.BrainfuckContext expected)
    {
        var actual = new Command(context).Execute();
        Assert.AreEqual<BrainfuckContext>(expected, actual);
    }

    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredOutput);
    }
}
